Import-Module PSScriptAnalyzer
$globalMessage = "Found global variable 'Global:1'."
$globalName = "PSPossibleGlobalRedefine"

$nonInitializedMessage = "Variable 'globalVars' is not initialized. Non-global variables must be initialized. To fix a violation of this rule, please initialize non-global variables."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\PossibleRedefinitionOfGlobalVariableInLocalScope.ps1

$globalViolations = $violations | Where-Object {$_.RuleName -eq $globalName}

$noViolations = Invoke-ScriptAnalyzer $directory\PossibleRedefinitionOfGlobalVariableInLocalScope.ps1
$noGlobalViolations = $noViolations | Where-Object {$_.RuleName -eq $globalName}

Describe "AvoidGlobalVars" {
    Context "When there are violations" {
        It "has 4 Possible redefinition of global variable in local scope" {
            $globalViolations.Count | Should Be 4
        }
                
        It "has the correct description message" {
            $globalViolations[0].Message | Should Match $globalMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noGlobalViolations.Count | Should Be 0
        }
    }
}

<#
# PSAvoidUninitializedVariable rule has been deprecated - Hence not a valid test case
Describe "AvoidUnitializedVars" {
    Context "When there are violations" {
        It "has 5 avoid using unitialized variable violations" {
            $nonInitializedViolations.Count | Should Be 5
        }

        It "has the correct description message" {
            $nonInitializedViolations[0].Message | Should Match $nonInitializedMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noUninitializedViolations.Count | Should Be 0
        }
    }    
}
#>