param(
    [Parameter(Mandatory=$true)]
    [string]$trxFilePath,
    [Parameter(Mandatory=$true)]
    [ValidateSet("Failed", "Passed")]
    [string]$outcome,
    [string]$txtOutputFilePath
)

if (!(Test-Path $trxFilePath))
{
    Write-Error "File not found at $trxFilePath";
    exit
}

[xml]$xmlContent = Get-Content $trxFilePath
$result = $xmlContent.TestRun.Results.UnitTestResult |
  Where-Object { $_.outcome -eq $outcome } |
  Select-Object -ExpandProperty testName |
  Sort-Object

Set-Content -Path $txtOutputFilePath -Value $result
$result