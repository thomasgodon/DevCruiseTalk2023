
param location string = resourceGroup().location
param resourceTags object = {
  Purpose: 'devCruise'
}
var azureEventHubsDataSender = '2b629674-e913-4c01-ae53-ef4638d8f975'
var azureEventHubsDataReceiver = 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'

var telemetryConsumerFunctionAppName = 'devCruiseTelemetryConsumer'
var iotDevicesFunctionAppName = 'devCruiseIotDevices'

// create app service plan
resource devCruiseAppServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'devCruiseServicePlan'
  tags: resourceTags
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

// create storage account (needed for azure function)
resource devCruiseStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'devcruisestorageaccount'
  location: location
  tags: resourceTags
  sku: {
    name: 'Standard_RAGRS'
  }
  kind: 'StorageV2'
}

// create iot devices function app
var devCruiseIotDevicesAppConfig = [
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~4'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: 'dotnet'
  }
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${devCruiseStorageAccount.name};AccountKey=${devCruiseStorageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
]

resource devCruiseIotDevicesFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: iotDevicesFunctionAppName
  tags: resourceTags
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: devCruiseAppServicePlan.id
    httpsOnly: true
    enabled: true
    clientAffinityEnabled: false
    siteConfig:{
      http20Enabled: true
      ftpsState: 'Disabled'
      appSettings: devCruiseIotDevicesAppConfig
    }
  }
}

// create event hub namespace
resource devCruiseEventHubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: 'devCruiseEventHubs'
  location: location
  tags: resourceTags
  sku: {
    capacity: 1
    name: 'Standard'
    tier: 'Standard'
  }
}

// create event hub
resource devCruiseTelemetryEventHub 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' = {
  parent:  devCruiseEventHubNamespace
  name:  'devCruiseTelemetry'
  properties: {
    messageRetentionInDays: 1
    partitionCount: 1
  }
}

// create iot central
resource devCruiseIotCentralApplication 'Microsoft.IoTCentral/iotApps@2021-06-01' = {
  name: 'devcruise'
  location: location
  properties: {
    displayName: 'devcruise'
    subdomain: 'devcruise'
    template: 'iotc-pnp-preview@1.0.0'
  }
  sku: {
    name: 'ST0'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// set event hub data sender role for iot central
resource iotCentralDataSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(devCruiseTelemetryEventHub.id, resourceGroup().id, azureEventHubsDataSender, devCruiseIotCentralApplication.id)
  scope: devCruiseTelemetryEventHub
  properties: {
    description: 'Add role to eventhub'
    principalId: devCruiseIotCentralApplication.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureEventHubsDataSender)
  }
}

// create telemetry consumer function consumer group
resource createTelemetryEventHubConsumerGroup 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2022-01-01-preview' = {
  name: 'telemetry-consumer-function'
  parent: devCruiseTelemetryEventHub
}

// create telemetry consumer function app
var telemetryConsumerAppConfig = [
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~4'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: 'dotnet'
  }
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${devCruiseStorageAccount.name};AccountKey=${devCruiseStorageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
  {
    name: 'EventHubOptions:ConnectionString__fullyQualifiedNamespace'
    value: '${devCruiseEventHubNamespace.name}.servicebus.windows.net'
  }
  {
    name: 'EventHubOptions:EventHubName'
    value: devCruiseTelemetryEventHub.name
  }
  {
    name: 'EventHubOptions:ConsumerGroup'
    value: createTelemetryEventHubConsumerGroup.name
  }
]

resource devCruiseTelemetryConsumerFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: telemetryConsumerFunctionAppName
  tags: resourceTags
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: devCruiseAppServicePlan.id
    httpsOnly: true
    enabled: true
    clientAffinityEnabled: false
    siteConfig:{
      http20Enabled: true
      ftpsState: 'Disabled'
      appSettings: telemetryConsumerAppConfig
    }
  }
}

// set event hub data reader role for telemetry consumer function app
resource telemetryConsumerDataReceiverAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(devCruiseTelemetryEventHub.id, resourceGroup().id, azureEventHubsDataReceiver, devCruiseTelemetryConsumerFunctionApp.id)
  scope: devCruiseTelemetryEventHub
  properties: {
    description: 'Add role to eventhub'
    principalId: devCruiseTelemetryConsumerFunctionApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureEventHubsDataReceiver)
  }
}
