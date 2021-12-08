# This is the settings file that configures rules for linting of powershell scripts
# across the entire raptor project
@{
    # Do not analyze the following rules.
    # Note: if a rule is in both IncludeRules and ExcludeRules, the rule
    # will be excluded.
    ExcludeRules = @(
        'PSAvoidUsingWriteHost',
        'PSUseBOMForUnicodeEncodedFile'
    )
}
