# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
# - reads data from default data pack in CDX's Enterprise Data Pack.
# - updates identifiers.json file with IDs obtained from the tenant whose
#   credentials are given in the appsettings.json file.
# - uses application permissions to access the data.

Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../RaptorUtils.ps1" -Resolve
. $raptorUtils

$appSettings = Get-AppSettings
$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath

$domain = Get-CurrentDomain -AppSettings $appSettings

#Connect To Microsoft Graph Using ClientId, TenantId and Certificate in AppSettings
Connect-DefaultTenant -AppSettings $appSettings

$identifiers.domain._value = $domain

$user = Get-DefaultAdminUser
$identifiers.user._value = $user.id
$identifiers.directoryObject._value = $user.id

$unifiedRoleDefinition = Invoke-RequestHelper -Uri "roleManagement/directory/roleDefinitions?`$filter=displayName eq 'Groups Administrator'" |
    Select-Object -First 1
$unifiedRoleDefinition.id
$identifiers.unifiedRoleDefinition._value = $unifiedRoleDefinition.id

$unifiedRoleAssignment = Invoke-RequestHelper -Uri "roleManagement/directory/roleAssignments?`$filter=principalId eq '$($user.id)'" |
    Select-Object -First 1
$unifiedRoleAssignment.id
$identifiers.unifiedRoleAssignment._value = $unifiedRoleAssignment.id

$calendarPermission = Invoke-RequestHelper -Uri "users/$($user.id)/calendar/calendarPermissions" |
    Select-Object -First 1
$calendarPermission.id
$identifiers.user.calendarPermission._value = $calendarPermission.id

$userScopeTeamsAppInstallation = Invoke-RequestHelper -Uri "users/$($user.id)/teamwork/installedApps?`$expand=teamsAppDefinition" |
    Where-Object { $_.teamsAppDefinition.displayName -eq "Teams" }
    Select-Object -First 1
$userScopeTeamsAppInstallation.id
$identifiers.user.userScopeTeamsAppInstallation._value = $userScopeTeamsAppInstallation.id

$team = Invoke-RequestHelper -Uri "groups" |
    Where-Object { $_.resourceProvisioningOptions -eq "Team" -and $_.displayName -eq "U.S. Sales"} |
    Select-Object -First 1
$team.id
$identifiers.team._value = $team.id
$identifiers.group._value = $team.id

$channel = Invoke-RequestHelper -Uri "teams/$($team.id)/channels" |
    Where-Object { $_.displayName -eq "Sales West"}
    Select-Object -First 1
$channel.id
$identifiers.team.channel._value = $channel.id

$member = Invoke-RequestHelper -Uri "teams/$($team.id)/channels/$($channel.id)/members" |
    Where-Object { $_.displayName -eq "MOD Administrator"}
    Select-Object -First 1
$member.id
$identifiers.team.channel.conversationMember._value = $member.id
$identifiers.team.conversationMember._value = $member.id

$installedApp = Invoke-RequestHelper -Uri "teams/$($team.id)/installedApps" |
    Select-Object -First 1
$installedApp.id
$identifiers.team.teamsAppInstallation._value = $installedApp.id

$drive = Invoke-RequestHelper -Uri "drives" |
    Where-Object { $_.createdBy.user.displayName -eq $admin } |
    Select-Object -First 1
$drive.id
$identifiers.drive._value = $drive.id

$driveItem = Invoke-RequestHelper -Uri "drives/$($drive.id)/root/children" |
    Where-Object { $_.name -eq "Blog Post preview.docx" } |
    Select-Object -First 1
$driveItem.id
$identifiers.drive.driveItem._value = $driveItem.id

$thumbnailSet = Invoke-RequestHelper -Uri "drives/$($drive.id)/items/$($driveItem.id)/thumbnails" |
    Select-Object -First 1
$thumbnailSet.id
$identifiers.driveItem.thumbnailSet._value = $thumbnailSet.id

$permission = Invoke-RequestHelper -Uri "drives/$($drive.id)/items/$($driveItem.id)/permissions" |
    Select-Object -First 1
$permission.id
$identifiers.driveItem.permission._value = $permission.id

$application = Invoke-RequestHelper -Uri "applications" |
    Where-Object { $_.displayName -eq "Salesforce" } |
    Select-Object -First 1
$application.id
$identifiers.application._value = $application.id

$applicationTemplate = Invoke-RequestHelper -Uri "applicationTemplates?`$filter=(displayName eq 'Microsoft Teams')" |
    Select-Object -First 1
$applicationTemplate.id
$identifiers.applicationTemplate._value = $applicationTemplate.id

$directoryAudit = Invoke-RequestHelper -Uri "auditLogs/directoryAudits?`$top=1" |
    Select-Object -First 1
$directoryAudit.id
$identifiers.directoryAudit._value = $directoryAudit.id

$signIn = Invoke-RequestHelper -Uri "auditLogs/signIns?`$top=1" |
    Select-Object -First 1
$signIn.id
$identifiers.signIn._value = $signIn.id

$directoryRole = Invoke-RequestHelper -Uri "directoryRoles" |
    Where-Object { $_.displayName -eq "Global Administrator" }
    Select-Object -First 1
$directoryRole.id
$identifiers.directoryRole._value = $directoryRole.id

$directoryRoleTemplate = Invoke-RequestHelper -Uri "directoryRoleTemplates" |
    Where-Object { $_.displayName -eq "Global Administrator" }
    Select-Object -First 1
$directoryRoleTemplate.id
$identifiers.directoryRoleTemplate._value = $directoryRoleTemplate.id

$conversation = Invoke-RequestHelper -Uri "groups/$($team.id)/conversations" |
    Where-Object { $_.topic -eq "The new U.S. Sales group is ready" }
    Select-Object -First 1
$conversation.id
$identifiers.group.conversation._value = $conversation.id

$conversationThread = Invoke-RequestHelper -Uri "groups/$($team.id)/conversations/$($conversation.id)/threads"
    Select-Object -First 1
$conversationThread.id
$identifiers.group.conversationThread._value = $conversationThread.id

$post = Invoke-RequestHelper -Uri "groups/$($team.id)/conversations/$($conversation.id)/threads/$($conversationThread.id)/posts" |
    Where-Object { $_.from.emailAddress.name -eq "U.S. Sales" } |
    Select-Object -First 1
$post.id
$identifiers.group.conversationThread.post._value = $post.id

$conditionalAccessPolicy = Invoke-RequestHelper -Uri "identity/conditionalAccess/policies" |
    Where-Object { $_.displayName -eq "Office 365 App Control" }
    Select-Object -First 1
$conditionalAccessPolicy.id
$identifiers.conditionalAccessPolicy._value = $conditionalAccessPolicy.id

$oauth2PermissionGrant = Invoke-RequestHelper -Uri "oauth2PermissionGrants" |
    Where-Object { $_.scope.Trim() -eq "user_impersonation" } |
    Select-Object -First 1
$oauth2PermissionGrant.id
$identifiers.oAuth2PermissionGrant._value = $oauth2PermissionGrant.id

$organization = Invoke-RequestHelper -Uri "organization" |
    Where-Object { $_.displayName -eq "Contoso" } |
    Select-Object -First 1
$organization.id
$identifiers.organization._value = $organization.id

# id is not dynamic per tenant
$identifiers.permissionGrantPolicy._value = "microsoft-all-application-permissions"
$identifiers.secureScoreControlProfile._value = "OneAdmin"

$schemaExtension = Invoke-RequestHelper -Uri "schemaExtensions?`$filter=description eq 'Global settings'" |
    Select-Object -First 1
$schemaExtension.id
$identifiers.schemaExtension._value = $schemaExtension.id

$secureScore = Invoke-RequestHelper -Uri "security/secureScores?`$top=1" |
    Select-Object -First 1
$secureScore.id
$identifiers.secureScore._value = $secureScore.id

$subscribedSku = Invoke-RequestHelper -Uri "subscribedSkus" |
    Where-Object { $_.skuPartNumber -eq 'ENTERPRISEPACK'}
    Select-Object -First 1
$subscribedSku.id
$identifiers.subscribedSku._value = $subscribedSku.id

$site = Invoke-RequestHelper -Uri "sites?search=site" |
    Where-Object { $_.displayName -eq 'The Landing' }
    Select-Object -First 1
$site.id
$identifiers.site._value = $site.id

$siteList = Invoke-RequestHelper -Uri "sites/$($site.id)/lists" |
    Where-Object {$_.displayName -eq "Demo Docs"}
    Select-Object -First 1
$siteList.id
$identifiers.site.list._value=$siteList.id

$siteListItem = Invoke-RequestHelper -Uri "sites/$($site.id)/lists/$($siteList.id)/items" |
    Select-Object -First 1
$siteListItem.id
$identifiers.site.list.listItem._value=$siteListItem.id

$siteListItemVersion = Invoke-RequestHelper -Uri "sites/$($site.id)/lists/$($siteList.id)/items/$($siteListItem.id)/versions" |
    Select-Object -First 1
$siteListItemVersion.id
$identifiers.site.list.listItem.listItemVersion._value=$siteListItemVersion.id

$termStoreGroup =  Invoke-RequestHelper -Uri "sites/$($site.id)/termStore/groups" |
    Select-Object -First 1
$termStoreGroup.id
$identifiers.site."termStore.group"._value = $termStoreGroup.id

$termStoreSet =  Invoke-RequestHelper -Uri "sites/$($site.id)/termStore/groups/$($termStoreGroup.id)/sets" |
    Select-Object -First 1
$termStoreSet.id
# set appears in two paths
$identifiers.site."termStore.set"._value = $termStoreSet.id
$identifiers.site."termStore.group"."termStore.set"._value = $termStoreSet.id

$termStoreTerm = Invoke-RequestHelper -Uri "sites/$($site.id)/termStore/sets/$($termStoreSet.id)/terms" |
    Select-Object -First 1
$termStoreTerm.id
# set appears in two paths
$identifiers.site."termStore.set"."termStore.term"._value = $termStoreTerm.id
$identifiers.site."termStore.group"."termStore.set"."termStore.term"._value = $termStoreTerm.id

$contentType = Invoke-RequestHelper -Uri "sites/$($site.id)/contentTypes?`$filter=name eq 'Phone Call Memo'" |
    Select-Object -First 1
$contentType.id
$identifiers.site.contentType._value = $contentType.id

$columnDefinition = Invoke-RequestHelper -Uri "sites/$($site.id)/contentTypes/$($contentType.id)/columns?`$filter=displayName eq 'Title'" |
    Select-Object -First 1
$columnDefinition.id
$identifiers.site.contentType.columnDefinition._value = $columnDefinition.id

#Missing Permission. Need to Create Permission on Root Site
#Azure AD Permission Issue.
#https://docs.microsoft.com/en-us/graph/api/site-post-permissions?view=graph-rest-1.0&tabs=http
# $sitePermission = Invoke-RequestHelper -Uri "sites/$($site.id)/permissions" |
#     Select-Object -First 1
# $sitePermission.id
# $identifiers.site.permission._value=$sitePermission.id

$servicePrincipal = Invoke-RequestHelper -Uri "servicePrincipals" |
    Where-Object {$_.displayName -eq "Microsoft Insider Risk Management"}
    Select-Object -First 1
$servicePrincipal.id
$identifiers.servicePrincipal._value = $servicePrincipal.id

$permissionGrantPolicy = Invoke-RequestHelper -Uri "policies/permissionGrantPolicies" |
    Where-Object {$_.displayName -eq "All application permissions, for any client app"}
    Select-Object -First 1
$permissionGrantPolicy.id
$identifiers.permissionGrantPolicy._value = $permissionGrantPolicy.id

#Tenant has no messages with Attachments
$message = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/messages?`$orderBy=createdDateTime asc" |
    Select-Object -First 1
$message.id
$identifiers.message._value = $message.id

#When Message with attachment is created, this should work
#TODO: Create Message with Attachment
$attachmentMessage = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/messages?`$filter=hasAttachments eq true" |
    Select-Object -First 1
$attachment = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/messages/$($attachmentMessage.id)/attachments" |
    Select-Object -First 1
$identifiers.message.attachment._value = $attachment.id

#OData Invoke-RequestHelperuest is not Supported.
# $messageExtensions = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/messages/$($identifiers.message._value)/extensions" |
#     Select-Object -First 1
# $messageExtensions.id
# $identifiers.message.extension._value = $messageExtensions.id

$calendarGroup = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/calendarGroups" |
    Where-Object {$_.name -eq "My Calendars"}
    Select-Object -First 1

$calendarGroup.id
$identifiers.calendarGroup._value=$calendarGroup.id

$orgContact = Invoke-RequestHelper -Uri "contacts" |
    Select-Object -First 1

$orgContact.id
$identifiers.orgContact._value = $orgContact.id

#Contact Folder is Missing from Tenant
$contactFolder = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/contactFolders" |
    Select-Object -First 1

$contactFolder.id
$identifiers.contactFolder._value=$contactFolder.id

$place = Invoke-RequestHelper -Uri "places/microsoft.graph.room" |
    Where-Object {$_.displayName -eq "Conf Room Rainier"}
    Select-Object -First 1
$place.id
#Places can also be obtained
#$place = Invoke-RequestHelper -Uri "places/$($place.id)"
$identifiers.place._value = $place.id

#Outlook Categories are pre-defined https://docs.microsoft.com/en-us/graph/api/resources/outlookcategory?view=graph-rest-1.0
$outlookCategory = Invoke-RequestHelper -Uri "users/$($identifiers.user._value)/outlook/masterCategories" |
    Where-Object {$_.displayName -eq "Red Category"} |
    Select-Object -First 1
$outlookCategory.id
$identifiers.outlookCategory._value = $outlookCategory.id

$roleAssignment = Invoke-RequestHelper -Uri "/roleManagement/directory/roleAssignments?`$filter=roleDefinitionId eq '62e90394-69f5-4237-9190-012177145e10'" |
    Select-Object -First 1
$roleAssignment.principalId
$identifiers.roleAssignmentPrincipal._value = $roleAssignment.principalId

# constant existing value in the tenant
$identifier.identityUserFlowAttribute._value = "city"

$serviceUpdateMessage = Invoke-RequestHelper -Uri "admin/serviceAnnouncement/messages" |
    Select-Object -First 1
$serviceUpdateMessage.id
$identifiers.serviceUpdateMessage._value = $serviceUpdateMessage.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath

# data missing
# $test = req -url "contracts"

# permission missing
# $test = req -url "dataPolicyOperations"

# data missing - my teams login in one device caused a data entry
# but the actual data is missing
# $test = req -url "devices"

# data missing
# $test = req -url "directory/administrativeUnits"

# can't query
# $test = req -url "directory/deletedItems"

# $educationClasses = req -url "education/classes"
# $educationClasses

# $educationSchools = req -url "education/schools"
# $educationSchools

# access denied
# $test = req -url "groups/$($team.id)/events"

# request not supported
# $extension = req -url "groups/$($team.id)/conversations/$($conversation.id)/threads/$($conversationThread.id)/posts/$($post.id)/extensions" |
#     Select-Object -First 1
# $extension.id
# $identifiers.group.conversationThread.post.extension._value = $extension.id

# no data
# $namedLocation = req -url "identity/conditionalAccess/namedLocations" |
#     Select-Object -First 1

# no data
# $threatAssessmentRequest = req -url "informationProtection/threatAssessmentRequests" |
#     Select-Object -First 1

# 404 No HTTP resource was found that matches the request URI 'https://mface.windowsazure.com/odata/authenticationMethodsPolicy/authenticationMethodConfigurations
# $test = req -url "policies/authenticationMethodsPolicy/authenticationMethodConfigurations"

# no data
# $test = req -url "subscriptions"

# no data
# req -url "users/$($user.id)/authentication/microsoftAuthenticatorMethods"

# no data
# req -url "users/$($user.id)/authentication/windowsHelloForBusinessMethods"

# $test = req -url "subscriptions"
