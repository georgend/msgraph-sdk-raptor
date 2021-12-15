# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


#Connect To Microsoft Graph Using ClientId, TenantId and Certificate in AppSettings
Connect-DefaultTenant -AppSettings $appSettings

#Get Agreements https://docs.microsoft.com/en-us/graph/api/agreement-list?view=graph-rest-1.0&tabs=http
$agreement = Get-RequestData -ChildEntity "Agreement"
$currentAgreement = Invoke-RequestHelper -Uri "identityGovernance/termsOfUse/agreements" -Method GET |
        Where-Object { $_.displayName -eq $agreement.displayName } |
        Select-Object -First 1

if($null -eq $currentAgreement){
    $currentAgreement = Invoke-RequestHelper -Uri "identityGovernance/termsOfUse/agreements" -Method POST -Body $agreement
    $currentAgreement.id
}

$identifiers.agreement._value = $currentAgreement.id

#Get identityProvider  https://docs.microsoft.com/en-us/graph/api/resources/identityprovider?view=graph-rest-1.0
# This API has been DEPRECATED
$identityProvider = Get-RequestData -ChildEntity "IdentityProvider"
$currentIdentityProvider = Invoke-RequestHelper -Uri "identityProviders" -Method GET |
        Where-Object { $_.type -eq $identityProvider.type} |
        Select-Object -First 1

# To avoid storing Secrets in Files, Generate them on the fly.
if($null -eq $currentIdentityProvider){
    $identityProvider.clientId = New-Guid
    $identityProvider.clientSecret = Get-RandomAlphanumericString -length 10
    $currentIdentityProvider = Invoke-RequestHelper -Uri "identity/identityProviders" -Method POST -Body $identityProvider
    $currentIdentityProvider.id
}

$identifiers.identityProvider._value = $currentIdentityProvider.id

#Get IdentityProviderBase  https://docs.microsoft.com/en-us/graph/api/identityproviderbase-get?view=graph-rest-1.0&tabs=csharp
$identityProviderBase = Get-RequestData -ChildEntity "IdentityProviderBase"
$currentIdentityProviderBase = Invoke-RequestHelper -Uri "identity/identityProviders" -Method GET |
        Where-Object { $_.type -eq $identityProviderBase.type} |
        Select-Object -First 1

# To avoid storing Secrets in Files, Generate them on the fly.
if($null -eq $currentIdentityProviderBase){
    $identityProviderBase.clientId = New-Guid
    $identityProviderBase.clientSecret = Get-RandomAlphanumericString -length 10
    $currentIdentityProviderBase = Invoke-RequestHelper -Uri "identity/identityProviders" -Method POST -Body $identityProviderBase
    $currentIdentityProviderBase.id
}

$identifiers.identityProviderBase._value = $currentIdentityProviderBase.id

#Get certificateBasedAuthConfiguration https://docs.microsoft.com/en-us/graph/api/certificatebasedauthconfiguration-get?view=graph-rest-1.0&tabs=http
$certificateBasedAuthConfiguration = Get-RequestData -ChildEntity "CertificateBasedAuthConfiguration"
$currentCertificateBasedAuthConfiguration = Invoke-RequestHelper -Uri "organization/$($identifiers.organization._value)/certificateBasedAuthConfiguration" -Method GET |
        Select-Object -First 1

if($null -eq $currentCertificateBasedAuthConfiguration || $currentCertificateBasedAuthConfiguration.value.Count -lt 1){
    $selfSignedCertificate = New-Certificate
    $certificateBasedAuthConfiguration.certificateAuthorities[0].certificate = $selfSignedCertificate
    $currentCertificateBasedAuthConfiguration = Invoke-RequestHelper -Uri "organization/$($identifiers.organization._value)/certificateBasedAuthConfiguration" -Method POST -Body $certificateBasedAuthConfiguration
    $currentCertificateBasedAuthConfiguration.id
}

$identifiers.organization.certificateBasedAuthConfiguration._value = $currentCertificateBasedAuthConfiguration.id

# Get organizationalBrandingProperties https://docs.microsoft.com/en-us/graph/api/organizationalbrandingproperties-create?view=graph-rest-1.0&tabs=http
$organizationalBrandingProperties = Get-RequestData -ChildEntity "Branding"
$currentOrganizationalBrandingProperties = Invoke-RequestHelper -Uri "organization/$($identifiers.organization._value)/branding" -Method GET

$currentOrganizationalBrandingProperties = Invoke-RequestHelper -Uri  "organization/$($identifiers.organization._value)/branding" -Method PATCH -Body $organizationalBrandingProperties
$currentOrganizationalBrandingProperties.id

# Get organizationalBrandingProperties https://docs.microsoft.com/en-us/graph/api/organizationalbrandingproperties-create?view=graph-rest-1.0&tabs=http
# French
$organizationalBrandingPropertiesLocalizationsFrench = Get-RequestData -ChildEntity "LocalizationsFrench"
$currentOrganizationalBrandingPropertiesLocalizationsFrench = Invoke-RequestHelper -Uri "organization/$($identifiers.organization._value)/branding/localizations/$($organizationalBrandingPropertiesLocalizationsFrench.id)" -Method GET

if($null -eq $currentOrganizationalBrandingPropertiesLocalizationsFrench){
    $currentOrganizationalBrandingPropertiesLocalizationsFrench = Invoke-RequestHelper -Uri  "organization/$($identifiers.organization._value)/branding/localizations" -Method POST -Body $organizationalBrandingPropertiesLocalizationsFrench
    $currentOrganizationalBrandingPropertiesLocalizationsFrench.id
}
# German
$organizationalBrandingPropertiesLocalizationsGerman = Get-RequestData -ChildEntity "LocalizationsGerman"
$currentOrganizationalBrandingPropertiesLocalizationsGerman = Invoke-RequestHelper -Uri "organization/$($identifiers.organization._value)/branding/localizations/$($organizationalBrandingPropertiesLocalizationsGerman.id)" -Method GET

if($null -eq $currentOrganizationalBrandingPropertiesLocalizationsGerman){
    $currentOrganizationalBrandingPropertiesLocalizationsGerman = Invoke-RequestHelper -Uri  "organization/$($identifiers.organization._value)/branding/localizations" -Method POST -Body $organizationalBrandingPropertiesLocalizationsGerman
    $currentOrganizationalBrandingPropertiesLocalizationsGerman.id
}
$identifiers.organization.organizationalBrandingLocalization._value= $currentOrganizationalBrandingPropertiesLocalizationsGerman.id

# Create Connected Organization https://docs.microsoft.com/en-us/graph/api/entitlementmanagement-post-connectedorganizations?view=graph-rest-1.0&tabs=http
$connectedOrganizationData = Get-RequestData -ChildEntity "ConnectedOrganization"
$connectedOrganizationUrl = "identityGovernance/entitlementManagement/connectedOrganizations"
$currentConnectedOrganization = Invoke-RequestHelper -Uri $connectedOrganizationUrl -Method GET |
        Where-Object { $_.displayName -eq $connectedOrganizationData.displayName } |
        Select-Object -First 1

if($null -eq $currentConnectedOrganization){
    $currentConnectedOrganization = Invoke-RequestHelper -Uri $connectedOrganizationUrl -Method POST -Body $connectedOrganizationData
    $currentConnectedOrganization.id
}

$identifiers.connectedOrganization._value = $currentConnectedOrganization.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
