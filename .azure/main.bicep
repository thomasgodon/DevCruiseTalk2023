
param location string = resourceGroup().location
param resourceTags object = {
  Purpose: 'devCruise'
}
var azureEventHubsDataSender = '2b629674-e913-4c01-ae53-ef4638d8f975'

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
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'devcruisestorageaccount'
  location: location
  tags: resourceTags
  sku: {
    name: 'Standard_RAGRS'
  }
  kind: 'StorageV2'
}

// create iot devices function
resource devCruiseIotDevicesFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'devCruiseIotDevices'
  tags: resourceTags
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: devCruiseAppServicePlan.id
    httpsOnly: true
    enabled: true
    clientAffinityEnabled: false
  }
}

// create iot devices function
resource devCruiseTelemetryConsumerFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'devCruiseTelemetryConsumer'
  tags: resourceTags
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: devCruiseAppServicePlan.id
    httpsOnly: true
    enabled: true
    clientAffinityEnabled: false
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
resource assignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(devCruiseTelemetryEventHub.id, resourceGroup().id, azureEventHubsDataSender, devCruiseIotCentralApplication.id)
  scope: devCruiseTelemetryEventHub
  properties: {
    description: 'Add role to eventhub'
    principalId: devCruiseIotCentralApplication.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', azureEventHubsDataSender)
  }
}
