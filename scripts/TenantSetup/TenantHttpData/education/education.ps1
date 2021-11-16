# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath
$appSettings = Get-AppSettings

#Connect To Microsoft Graph Using Education ClientId, TenantId and Certificate in AppSettings
Connect-EduTenant -AppSettings $appSettings


$educationClass = Invoke-RequestHelper -Uri "education/classes" -Method GET |
    Where-Object { $_.displayName -eq "Physical Science" } |
    Select-Object -First 1
$educationClass.id
$identifiers.educationClass._value = $educationClass.id

# Get or create EducationCategory
$educationCategoryUri = "education/classes/$($educationClass.id)/assignmentCategories"
$educationCategory = Invoke-RequestHelper -Uri ($educationCategoryUri + "/?`$top=1")
if (!$educationCategory){
    $educationCategoryData = Get-RequestData -ChildEntity "educationCategory"
    $educationCategory = Request-DelegatedResource -IsEducation $true -Uri $educationCategoryUri -Method "POST" -Body $educationCategoryData -ScopeOverride "EduAssignments.ReadWrite"
}
$educationCategory.id
$identifiers.educationClass.educationCategory._value = $educationCategory.id

# Get educationAssignment
$educationAssignment = Invoke-RequestHelper -Uri "education/classes/$($educationClass.id)/assignments?`$filter=displayName eq 'Midterm'" -Method GET |
    Select-Object -First 1
$educationAssignment.id
$identifiers.educationClass.educationAssignment._value = $educationAssignment.id

#Get AssignmentResource
$educationAssignmentResource = Invoke-RequestHelper -Uri "education/classes/$($educationClass.id)/assignments/$($educationAssignment.id)/resources?`$top=1"
$educationAssignmentResource.id
$identifiers.educationClass.educationAssignment.educationAssignmentResource._value = $educationAssignmentResource.id

# Get assigment submission
$educationSubmission = Invoke-RequestHelper -Uri "education/classes/$($educationClass.id)/assignments/$($educationAssignment.id)/submissions?`$filter=status eq 'submitted'" -Method GET |
    Select-Object -First 1
$educationSubmission.id
$identifiers.educationClass.educationAssignment.educationSubmission._value = $educationSubmission.id

$educationSubmissionResource = Invoke-RequestHelper -Uri "education/classes/$($educationClass.id)/assignments/$($educationAssignment.id)/submissions/$($educationSubmission.id)/resources?`$top=1" -Method GET |
    Select-Object -First 1
$educationSubmissionResource.id
$identifiers.educationClass.educationAssignment.educationSubmission.educationSubmissionResource._value = $educationSubmissionResource.id

# Get or create educationSchool
$educationSchoolUri = "education/schools"
$educationSchool = Invoke-RequestHelper -Uri ($educationSchoolUri + "/?`$top=1")
if(!$educationSchool){
    $educationSchoolData = Get-RequestData -ChildEntity "educationSchool"
    $educationSchool = Invoke-RequestHelper -Uri $educationSchoolUri -Body $educationSchoolData -Method "POST"

}
$educationSchool.id
$identifiers.educationSchool._value = $educationSchool.id

# Get or create EducationRubric
$educationRubric = Request-DelegatedResource -Uri "education/me/rubrics?`$top=1" -IsEducation $true -ScopeOverride "EduAssignments.ReadWrite"
if (!$educationRubric){
    $rubricData = Get-RequestData -ChildEntity "educationRubric"
    $educationRubric =  Request-DelegatedResource -Uri "education/me/rubrics" -IsEducation $true -Method "POST" -Body $rubricData -ScopeOverride "EduAssignments.ReadWrite"
}
$educationRubric.id
$identifiers.educationRubric._value = $educationRubric.id
$identifiers.educationUser.educationRubric._value = $educationRubric.id


$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
