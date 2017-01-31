Import-Module PSScriptAnalyzer
$ruleName = "PSUseWhitespace"
$ruleConfiguration = @{
    Enable = $true
    CheckOpenBrace = $true
}

$settings = @{
    IncludeRules = @($ruleName)
    Rules = @{
        PSUseWhitespace = $ruleConfiguration
    }
}

Describe "UseWhitespace" {
    Context "When no whitespace is present before if block open brace" {
        BeforeAll {
            $def = @'
if ($true){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }
    }
}
