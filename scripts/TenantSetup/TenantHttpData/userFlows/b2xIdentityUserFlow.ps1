# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath

# Create an api configuration for use in creating a userflow if it doesn't exist.
$apiConnectorData = Get-RequestData -ChildEntity "apiConnector"
$apiConnector = Request-DelegatedResource -Uri "identity/apiConnectors?`$filter=displayName eq '$($apiConnectorData.displayName)'" -ScopeOverride "APIConnectors.ReadWrite.All"
if (!$apiConnector){
    $apiConnectorData.authenticationConfiguration.password = Get-RandomAlphanumericString -length 10
    $apiConnector = Request-DelegatedResource -Uri "identity/apiConnectors" -Method "POST" -Body $apiConnectorData -ScopeOverride "APIConnectors.ReadWrite.All"
}
$apiConnector.id
$identifiers.identityApiConnector._value = $apiConnector.id

$b2xuserFlowData = Get-RequestData -ChildEntity "b2xIdentityUserFlow"
$b2xIdentityUserFlow = Request-DelegatedResource -Uri "identity/b2xUserFlows?`$filter=id eq 'B2X_1_$($b2xuserFlowData.id)'" -ScopeOverride "IdentityUserFlow.Read.All"
if (!$b2xIdentityUserFlow){
    $b2xuserFlowData.apiConnectorConfiguration.postAttributeCollection."@odata.id" += $apiConnector.id
    $b2xIdentityUserFlow = Request-DelegatedResource -Uri "identity/b2xUserFlows" -Method "POST" -Body $b2xuserFlowData -ScopeOverride "IdentityUserFlow.ReadWrite.All"
}
$b2xIdentityUserFlow.id
$identifiers.b2xIdentityUserFlow._value = $b2xIdentityUserFlow.id

$identityUserFlowAttributeAssignment = Request-DelegatedResource -Uri "identity/b2xUserFlows/$($b2xIdentityUserFlow.id)/userAttributeAssignments?`$top=1" -ScopeOverride "IdentityUserFlow.Read.All"
$identityUserFlowAttributeAssignment.id
$identifiers.b2xIdentityUserFlow.identityUserFlowAttributeAssignment._value = $identityUserFlowAttributeAssignment.id

$userLanguage = Request-DelegatedResource -Uri "identity/b2xUserFlows/$($b2xIdentityUserFlow.id)/languages?`$filter=displayName eq 'français'" -ScopeOverride "IdentityUserFlow.Read.All"
$userLanguage.id
$identifiers.b2xIdentityUserFlow.userFlowLanguageConfiguration._value = $userLanguage.id

$userLanguagePage = Request-DelegatedResource -Uri "identity/b2xUserFlows/B2X_1_Partner/languages/fr/defaultPages?`$top=1" -ScopeOverride "IdentityUserFlow.Read.All"
$userLanguagePage.id
$identifiers.b2xIdentityUserFlow.userFlowLanguageConfiguration.userFlowLanguagePage._value = $userLanguagePage.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
