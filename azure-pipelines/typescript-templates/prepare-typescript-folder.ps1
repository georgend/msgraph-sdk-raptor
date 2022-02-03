########
# Pack and build msgraph-sdk-code
########

Write-Output "====================================================="
Write-Output "Raptor Log: NPM Login"
Write-Output "====================================================="

$CWD = Get-Location
Set-Location $CWD/msgraph-sdk-typescript

Write-Output "Raptor Log: Packaging typescript folder"

npm pack


## duplicate with a simple name
Copy-Item msgraph-sdk-typescript-*.tgz msgraph-sdk-typescript.tgz

Write-Output "Raptor Log: Completed packaging typescript folder"

# Create a test folder

Write-Output "Raptor Log: Setting up testing folder"

$GRAPH_FILE= "$CWD/msgraph-sdk-typescript/msgraph-sdk-typescript.tgz"

if (Test-Path -Path $GRAPH_FILE -PathType Leaf) {

    Write-Output "Raptor Log: NPM was sucessfull!"

    if (Test-Path -Path "$CWD/typescript-tests"){
        Remove-Item -Path "$CWD/typescript-tests" -Recurse -Force
    }

    New-Item -Path $CWD -Name "typescript-tests" -ItemType "directory"
    Set-Location $CWD/typescript-tests

    Write-Output "Raptor Log: Set up local npm folder"

    Copy-Item ${CWD}/msgraph-sdk-raptor/azure-pipelines/typescript-templates/tsconfig.json tsconfig.json
    Copy-Item ${CWD}/msgraph-sdk-raptor/azure-pipelines/typescript-templates/.npmrc .npmrc


    Write-Output "Raptor Log: NPM User $env:NPM_USERNAME "

    npm install -g npm-cli-login
    npm-cli-login -u $env:NPM_USERNAME -p $env:NPM_PASSWORD -e $env:NPM_EMAIL -r https://npm.pkg.github.com -s @microsoft

    Copy-Item $CWD/msgraph-sdk-typescript/msgraph-sdk-typescript.tgz msgraph-sdk-typescript.tgz

    npm install msgraph-sdk-typescript.tgz

    Write-Output "Raptor Log: Completed setup of test folder"

    Set-Location $CWD
    exit 0
} else {

    Write-Output "Raptor Log: NPM file not found"

    Set-Location $CWD
    exit 1
}
