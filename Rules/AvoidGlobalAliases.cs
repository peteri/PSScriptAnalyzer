﻿using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class AvoidGlobalAliases : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> records;
        private string fileName;

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            ast.Visit(this);

            return records;
        }

        #region VisitCommand functions
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (!IsNewAliasCmdlet(commandAst))
            {
                return AstVisitAction.SkipChildren;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            if (IsScopeParameterForNewAliasCmdlet(commandParameterAst))
            {
                if ((commandParameterAst.Argument != null)   // if the cmdlet looks like -Scope:Global check Parameter.Argument
                    && (commandParameterAst.Argument.ToString().Equals("Global", StringComparison.OrdinalIgnoreCase)))
                {
                    records.Add(new DiagnosticRecord(
                                    string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesError),
                                    commandParameterAst.Extent,
                                    GetName(),
                                    DiagnosticSeverity.Warning,
                                    fileName,
                                    commandParameterAst.ParameterName));
                }
                else
                {
                    var nextAst = FindNextAst(commandParameterAst);

                    if ((nextAst is StringConstantExpressionAst)   // if the cmdlet looks like -Scope Global
                        && (((StringConstantExpressionAst)nextAst).Value.ToString().Equals("Global", StringComparison.OrdinalIgnoreCase)))
                    {
                        records.Add(new DiagnosticRecord(
                                string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesError),
                                ((StringConstantExpressionAst)nextAst).Extent,
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName,
                                ((StringConstantExpressionAst)nextAst).Value));
                    }
                }
            }

            return AstVisitAction.SkipChildren;
        }
        #endregion

        private Ast FindNextAst(Ast ast)
        {
            IEnumerable<Ast> matchingLevelAsts = ast.Parent.FindAll(item => item is Ast, true);

            Ast currentClosest = null;
            foreach (var matchingLevelAst in matchingLevelAsts)
            {
                if (currentClosest == null)
                {
                    if (IsAstAfter(ast, matchingLevelAst))
                    {
                        currentClosest = matchingLevelAst;
                    }
                }
                else
                {
                    if ((IsAstAfter(ast, matchingLevelAst)) && (IsAstAfter(matchingLevelAst, currentClosest)))
                    {
                        currentClosest = matchingLevelAst;
                    }
                }
            }

            return currentClosest;
        }

        private bool IsAstAfter(Ast ast1, Ast ast2)
        {
            if (ast1.Extent.EndLineNumber > ast2.Extent.StartLineNumber)  // ast1 ends on a line after ast2 starts
            {
                return false;
            }
            else if (ast1.Extent.EndLineNumber == ast2.Extent.StartLineNumber)
            {
                if (ast2.Extent.StartColumnNumber > ast1.Extent.EndColumnNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else   // ast2 starts on a line after ast 1 ends
            {
                return true;
            }
        }

        private bool IsScopeParameterForNewAliasCmdlet(CommandParameterAst commandParameterAst)
        {
            if (commandParameterAst == null || commandParameterAst.ParameterName == null)
            {
                return false;
            }

            if (commandParameterAst.ParameterName.Equals("Scope", StringComparison.OrdinalIgnoreCase)
                && (commandParameterAst.Parent is CommandAst)
                && IsNewAliasCmdlet((CommandAst)commandParameterAst.Parent))
            {
                return true;
            }

            return false;
        }

        private bool IsNewAliasCmdlet(CommandAst commandAst)
        {
            if (commandAst == null || commandAst.GetCommandName() == null)
            {
                return false;
            }

            var AliasList = Helper.Instance.CmdletNameAndAliases("New-Alias");
            if (AliasList.Contains(commandAst.GetCommandName()))
            {
                return true;
            }

            return false;
        }

        public string GetCommonName()
        {
             return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesDescription);
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesName);
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
