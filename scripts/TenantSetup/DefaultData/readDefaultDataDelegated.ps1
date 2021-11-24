# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
# - reads data from default data pack in CDX's Enterprise Data Pack.
# - updates identifiers.json file with IDs obtained from the tenant whose
#   credentials are given in the appsettings.json file.
# - uses delegated permissions to access the data.

Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

$raptorUtils = Join-Path $PSScriptRoot "../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath

$calendarGroup = Request-DelegatedResource -Uri "me/calendarGroups" |
    Where-Object { $_.name -eq "My Calendars" } |
    Select-Object -First 1
$calendarGroup.id
$identifiers.calendarGroup._value = $calendarGroup.id

$todoTaskList = Request-DelegatedResource -Uri "me/todo/lists" -ScopeOverride "Tasks.Read" |
Select-Object -First 1
$todoTaskList.id
$identifiers.todoTaskList._value = $todoTaskList.id

# no data
# $todoTask = Request-DelegatedResource -Uri "me/todo/lists/$($todoTaskList.id)/tasks" -ScopeOverride "Tasks.Read"

# no data
# $linkedResource = Request-DelegatedResource -Uri "me/todo/lists/$($todoTaskList.id)/tasks/$($todoTask.id)/linkedResources" -ScopeOverride "Tasks.Read"

# no data
# $contactFolder = Request-DelegatedResource -Uri "me/contactFolders" # already in readDefaultData under user/contactFolders. Remove this??


$mailFolder = Request-DelegatedResource -Uri "me/mailFolders/inbox"
$mailFolder.id
$identifiers.mailFolder._value = $mailFolder.id

#Get Group with plans
$groupWithPlan = Request-DelegatedResource -Uri "groups" |
    Where-Object {$_.displayName -eq "Mark 8 Project Team"}
    Select-Object -First 1
$groupWithPlan.id
#Get Planner via Group
$plannerPlan = Request-DelegatedResource -Uri "groups/$($groupWithPlan.id)/planner/plans" |
    Where-Object {$_.title -eq "Mark8 project tracking"}
    Select-Object -First 1

$plannerPlan.id
$identifiers.plannerPlan._value=$plannerPlan.Id

$plannerTask = Request-DelegatedResource -Uri "planner/plans/$($plannerPlan.id)/tasks" |
    Where-Object {$_.title -eq "Organize Catering"}
    Select-Object -First 1

$plannerTask.id
$identifiers.plannerTask._value=$plannerTask.Id

$plannerBucket = Request-DelegatedResource -Uri "planner/plans/$($plannerPlan.id)/buckets" |
    Where-Object {$_.name -eq "After party"}
    Select-Object -First 1

$plannerBucket.id
$identifiers.plannerBucket._value=$plannerBucket.Id

# tenant agnostic data
$printEndpointId = "mpsdiscovery"
$printEndpointId
$identifiers.printService.printServiceEndpoint._value = $printEndpointId

$printService = Request-DelegatedResource -Uri "print/services" -ScopeOverride "Printer.Read.All" |
    Where-Object { $_.endpoints[0].id -eq $printEndpointId } |
    Select-Object -First 1

$printService.id
$identifiers.printService._value = $printService.id

$groupSettingTemplate = Request-DelegatedResource -Uri "groupSettingTemplates" |
    Where-Object { $_.displayName -eq "Prohibited Names Settings" } |
    Select-Object -First 1

$groupSettingTemplate.id
$identifiers.groupSettingTemplate._value = $groupSettingTemplate.id

$presence = Request-DelegatedResource -Uri "users/$($identifiers.user._value)/presence"
$presence.id
$identifiers.presence._value = $presence.id

$teamsApp = Request-DelegatedResource -Uri "appCatalogs/teamsApps" |
    Where-Object { $_.displayName -eq "Teams" } |
    Select-Object -First 1
$teamsApp.id
$identifiers.teamsApp._value = $teamsApp.id

# Use delegated permissions for channelMessage and replies to bypass protected api restricion for application permission.
#Use Team and channel already in identifiers.json to get chnnel message and replies
$teamId = $identifiers.team._value
$teamId
$channelId = $identifiers.team.channel._value
$channelId
# Get channel message default data
$channelMessage = Request-DelegatedResource -Uri "teams/$teamId/channels/$channelId/messages" -ScopeOverride "ChannelMessage.Read.All" |
    Where-Object { $_.from.user.displayName -eq "Lynne Robbins"} |
    Select-Object -First 1
$channelMessage.id
$identifiers.team.channel.chatMessage._value = $channelMessage.id

# Get channel message replies default data
$messageReply = Request-DelegatedResource -Uri "teams/$teamId/channels/$channelId/messages/$($channelMessage.id)/replies?`$top=1" -ScopeOverride "ChannelMessage.Read.All"
$messageReply.id
$identifiers.team.channel.chatMessage.reply._value = $messageReply.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath

# bad request
# $driveItemWorkbookOperations = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/operations"

# no data
# $connector = Request-DelegatedResource -Uri "print/connectors" -ScopeOverride "PrintConnector.Read.All"

# no data
# $printShares = Request-DelegatedResource -Uri "print/shares"

# 500
# Request-DelegatedResource -Uri "reports/dailyPrintUsageByPrinter"

# 500
# Request-DelegatedResource -Uri "reports/dailyPrintUsageByUser"

# no data
# Request-DelegatedResource -Uri "identityGovernance/termsOfUse/agreements"
# Request-DelegatedResource -Uri "identityGovernance/appConsent/appConsentRequests"
