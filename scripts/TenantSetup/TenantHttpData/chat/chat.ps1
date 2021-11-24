# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


$userId = Get-DefaultAdminUserId
$content = Get-RequestData -ChildEntity "chat"
$content.members[0].'user@odata.bind' += $userId
$user2 = Invoke-RequestHelper -Uri "users"  |Select-Object -First 2
$content.members[1].'user@odata.bind' += $user2[0].id
$content.members[2].'user@odata.bind' += $user2[1].id
$chat = Request-DelegatedResource -Uri "chats?`$filter=chatType eq 'group'" -ScopeOverride "Chat.Read" | Select-Object -First 1
if (!$chat){
    $chat = Request-DelegatedResource -Uri "chats" -Method "POST" -Body $content -ScopeOverride "Chat.ReadWrite"
}
$chat.id
$identifiers.chat._value = $chat.id
$identifiers.user.chat._value = $chat.id

$conversationMember = Request-DelegatedResource -Uri "chats/$($identifiers.chat._value)/members" -ScopeOverride "ChatMember.Read" | Select-Object -Last 1
$conversationMember.id
$identifiers.chat.conversationMember._value = $conversationMember.id

$teamsAppInstallation = Request-DelegatedResource -Uri "chats/$($identifiers.chat._value)/installedApps?`$expand=teamsApp&`$filter=teamsApp/displayName eq 'Word'" -ScopeOverride "TeamsAppInstallation.ReadForChat" | Select-Object -First 1
$teamsAppInstallation.id
$identifiers.chat.teamsAppInstallation._value = $teamsAppInstallation.id

$wordTeamsApp = Request-DelegatedResource -Uri "appCatalogs/teamsApps?`$filter=displayName eq 'Word'" -ScopeOverride "AppCatalog.Read.All"
$wordTeamsApp.id
$teamsTabContent = Get-RequestData -ChildEntity "teamsTab"
$teamsTabContent.'teamsApp@odata.bind' += $wordTeamsApp.id
$tabsUri = "chats/$($identifiers.chat._value)/tabs"
$teamsTab = Request-DelegatedResource -Uri $tabsUri -ScopeOverride "TeamsTab.Read.All"| Select-Object -First 1
if(!$teamsTab){
    $teamsTab = Request-DelegatedResource -Uri $tabsUri -Method "POST" -Body $teamsTabContent -ScopeOverride "TeamsTab.ReadWriteForChat"
}
$teamsTab.id
$identifiers.chat.teamsTab._value = $teamsTab.id

# get or create chat message
$chatMessage = Request-DelegatedResource -Uri "chats/$($chat.id)/messages?`$top=1" -ScopeOverride "Chat.Read"
if (!$chatMessage){
    $chatBody = Get-RequestData -ChildEntity "chatMessage"
    $imageFilePath = Join-Path $PSScriptRoot ".\chatImage.png" -Resolve
    if(Test-Path -Path $imageFilePath){
        $base64EncodedImg =[System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes($imageFilePath))
        $chatBody.hostedContents[0].contentBytes = $base64EncodedImg
        $chatMessage = Request-DelegatedResource -Uri "chats/$($chat.id)/messages" -Method "POST" -Body $chatBody -ScopeOverride "Chat.ReadWrite"
    }
    else
    {
        Write-Error "Can't find path $($imageFilePath)"
        exit
    }
}
$chatMessage.id
$identifiers.chat.chatMessage._value = $chatMessage.id
$identifiers.user.chat.chatMessage._value = $chatMessage.id

# Get Chat message hosted content created in chat message creation above
$hostedContent = Request-DelegatedResource -Uri "chats/$($chat.id)/messages/$($chatMessage.id)/hostedContents?`$top=1" -ScopeOverride "Chat.Read"
$hostedContent.id
$identifiers.chat.chatMessage.hostedContent._value = $hostedContent.id
$identifiers.chat.chatMessage.chatMessageHostedContent._value = $hostedContent.id
$identifiers.user.chat.chatMessage.hostedContent._value = $hostedContent.id

$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
