BeforeAll {
    $results = Invoke-ScriptAnalyzer -Path ./scripts/ -Recurse -ReportSummary -Settings PSScriptAnalyzerSettings.psd1

}
Describe "PSScriptAnalyzer analysis" {
    It "Should not have Script Errors" {
        $results | Should -BeNullOrEmpty -Because "Scripts should not have linting Errors"
    }
    It "Should not violate rule: <_>" -ForEach (Get-ScriptAnalyzerRule) {
        foreach($result in $results){
            $result.RuleName | Should -Not -Contain $_.RuleName -Because "Message: $($result.Message) Source => $($result.ScriptName):$($result.Line)"
        }
    }
}
