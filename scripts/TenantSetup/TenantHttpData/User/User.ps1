# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$userId = Get-DefaultAdminUserId

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate in AppSettings
Connect-DefaultTenant -AppSettings $appSettings

$contactData = Get-RequestData -ChildEntity "Contact"
$contactUrl = "/users/$($userId)/contacts"
$currentContact = Request-DelegatedResource -Uri $contactUrl |
    Where-Object { $_.displayName -eq $contactData.displayName } |
    Select-Object -First 1

if($null -eq $currentContact) {
    $currentContact = Request-DelegatedResource -Uri $contactUrl -Body $contactData -Method POST
    $currentContact.id
}

$identifiers.contact._value = $currentContact.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
