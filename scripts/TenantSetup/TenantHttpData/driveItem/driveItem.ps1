# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $workbookFilePath = (Join-Path $PSScriptRoot "./raptorWorkbookDriveItem.xlsx" -Resolve),
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)

$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath


$uploadFileName = "raptorWorkbookDriveItemTest.xlsx"

# no need to check the existence of driveItem, as it will be created if it does not exist
# if it exists, it will just generate a new version
$driveItem = Request-DelegatedResource -Uri "me/drive/root:/$($uploadFileName):/content" -Method "PUT" -FilePath $workbookFilePath -scopeOverride "Files.ReadWrite.All"
# create a second version of the file
$driveItemVersion2 =  Request-DelegatedResource -Uri "me/drive/root:/$($uploadFileName):/content" -Method "PUT" -FilePath $workbookFilePath -scopeOverride "Files.ReadWrite.All"

$driveItem.id
$driveItemVersion2.id
$identifiers.driveItem._value = $driveItem.id
$identifiers.driveItem.driveItemVersion._value = "1.0" # standard first version number

$driveItemPermission = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/permissions" |
    Select-Object -First 1
$driveItemPermission.id
$identifiers.driveItem.permission._value = $driveItemPermission.id


$driveItemThumbnail = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/thumbnails?`$top=1" |
    Select-Object -First 1
$driveItemThumbnail.id
$identifiers.driveItem.thumbnailSet._value = $driveItemThumbnail.id
$identifiers.driveItem.thumbnailSet.thumbnailSet._value = "small"


$driveItemWorkbookTable = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables" |
    Where-Object { $_.name -eq "Table1" } |
    Select-Object -First 1
$driveItemWorkbookTable.id
$identifiers.driveItem.workbookTable._value = $driveItemWorkbookTable.id


$tableColumn = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/columns" |
    Select-Object -First 1
$tableColumn.id
$identifiers.driveItem.workbookTable.workbookTableColumn._value = $tableColumn.id


$tableRow = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/rows" |
    Select-Object -First 1
$tableRow.index
$identifiers.driveItem.workbookTable.workbookTableRow._value = "itemAt(index=$($tableRow.index))"


$worksheet = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets" |
    Where-Object { $_.name -eq "Sheet1" } |
    Select-Object -First 1
$worksheet.id
$identifiers.driveItem.workbookWorksheet._value = $worksheet.id
$identifiers.workbookWorksheet._value = $worksheet.id


$workbookNamedItem = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/names/test2"
$workbookNamedItem.name
$identifiers.driveItem.workbookNamedItem._value = $workbookNamedItem.name

$namedItemFormatBorder = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/names/$($workbookNamedItem.name)/range/format/borders?`$top=1" |
    Select-Object -First 1
$namedItemFormatBorder.id
$identifiers.driveItem.workbookNamedItem.workbookRangeBorder._value = $namedItemFormatBorder.id


$workbookChart = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts" |
    Select-Object -First 1
$workbookChart.name
$identifiers.driveItem.workbookWorksheet.workbookChart._value = $workbookChart.name


$workbookChartSeries = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts/$($workbookChart.name)/series"|
    Select-Object -First 1
$workbookChartSeries."@odata.id"
$series_id = $workbookChartSeries."@odata.id".Split("series/")[1]
$identifiers.driveItem.workbookWorksheet.workbookChart.workbookChartSeries._value = $series_id


$workbookChartPoint = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts/$($workbookChart.name)/series/$($series_id)/points"|
    Select-Object -First 1
$workbookChartPoint."@odata.id"
$chartPoint_id = $workbookChartPoint."@odata.id".split("points/")[1]
$identifiers.driveItem.workbookWorksheet.workbookChart.workbookChartSeries.workbookChartPoint._value = $chartPoint_id


$workbookComment = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/comments" |
    Select-Object -First 1
$workbookComment.id
$identifiers.driveItem.workbookComment._value = $workbookComment.id


$workbookCommentReply = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/comments/$($workbookComment.id)/replies" |
    Select-Object -First 1
$workbookCommentReply.id
$identifiers.driveItem.workbookComment.workbookCommentReply._value = $workbookCommentReply.id


$operationData = Get-RequestData -ChildEntity "driveItemWorkbookOperation"
$operationHeaders = @{"Prefer"="respond-async"}
$driveItemWorkbookOperation = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/createSession" -Method "POST" -Headers $operationHeaders -Body $operationData
$driveItemWorkbookOperation.id
$identifiers.driveItem.workbookOperation._value = $driveItemWorkbookOperation.id


$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
