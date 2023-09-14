# The transformation query specified here will be used to change each exported 
# message into a different format. You can get started using the example below,
# and learn more about the language in documentation:
# https://aka.ms/dataexporttransformation
{
    schema: "default@v1",
    applicationId: .applicationId,
    deviceId: .device.id,
    templateId: .device.templateId,
    messageSource: .messageSource,
    enqueuedTime: .enqueuedTime,
    telemetry: .telemetry | map({ key: .name, value: .value }) | from_entries,
    messageProperties: .messageProperties,
    enrichments: .enrichments,
    component: .component,
    deviceName: .device.name,
    module: .module
}