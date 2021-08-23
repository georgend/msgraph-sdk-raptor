# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $AppSettingsPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/appsettings.json"),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

Install-Az
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$appSettings = Get-AppSettings -AppSettingsPath $AppSettingsPath


#Create printer
$storedSecretContent = Get-AzKeyVaultSecret -VaultName $appSettings.AzureKeyVaultName -Name "csrData" -AsPlainText  # Retrieve printer CSR content from azure key-Vault
$storedSecretTransportKey = Get-AzKeyVaultSecret -VaultName $appSettings.AzureKeyVaultName -Name "transportKey" -AsPlainText  # Retrieve printer CSR transportKey from azure key-Vault
$printerData = Get-RequestData -ChildEntity "printer"
$printerData.certificateSigningRequest.content = $storedSecretContent
$printerData.certificateSigningRequest.transportKey = $storedSecretTransportKey
$printerHeaderResp = Request-DelegatedResource -Uri "print/printers/create" -Method "POST" -Body $printerData
$printerOperationUrl = $printerHeaderResp."Operation-Location" | Select-Object -First 1
$printerOperationRequestUri = $printerOperationUrl.Split("v1.0/")[1]
$createPrinterOps = Request-DelegatedResource -Uri $printerOperationRequestUri
if (@("running", "notStarted") -contains $createPrinterOps.status.state){
    start-Sleep -Milliseconds 1000  # wait for 1second before retrying
    $createPrinterOps = Request-DelegatedResource -Uri $printerOperationRequestUri
}
if ($createPrinterOps.status.state -ne "succeeded") {
    Write-Error  -Message "Printer creation failed. Status=$($createPrinterOps.status.state)"
    $printer_id = $null
}
else{
    $printer = $createPrinterOps.printer
    $printer_id = $printer.id
}
$printer_id
$identifiers.printer._value = $printer_id

#create PrintJob
$printJobData = Get-RequestData -ChildEntity "printJob"
$printJob = Request-DelegatedResource -Uri "print/printers/$($printer_id)/jobs" -Method "POST" -Body $printJobData
$printJob.id
$identifiers.printer.printJob._value = $printJob.id
$printJob.documents[0].id
$identifiers.printer.printJob.printDocument._value = $printJob.documents[0].id

# create printTaskDefinition
$printTaskDefinitionData = Get-RequestData -ChildEntity "printTaskDefinition"
Connect-MgGraph -CertificateThumbprint $appSettings.CertificateThumbprint -ClientId $appSettings.ClientID -TenantId $appSettings.TenantID
$printTaskDefinition = Invoke-RequestHelper -Uri "print/taskDefinitions" -Method "POST" -Body $printTaskDefinitionData
$printTaskDefinition.id

#Create PrintTaskTrigger
$printTaskTriggerData = Get-RequestData -ChildEntity "printTaskTrigger"
$printTaskTriggerData."definition@odata.bind" += $printTaskDefinition.id
$printTaskTrigger = Request-DelegatedResource -Uri "print/printers/$($printer_id)/taskTriggers" -Method "POST" -Body $printTaskTriggerData
$printTaskTrigger.id
$identifiers.printer.printTaskTrigger._value = $printTaskTrigger.id

# save identifiers
$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
