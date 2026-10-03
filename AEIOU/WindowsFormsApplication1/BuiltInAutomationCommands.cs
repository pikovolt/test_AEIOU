using System;
using System.Collections.Generic;
using System.Globalization;
using AEIOU.Automation;

namespace AEIOU
{
    public sealed class ReplaceCommand : IAutomationCommand
    {
        public const string BeforeParameter = "before";
        public const string AfterParameter = "after";

        public AutomationCommandDescriptor Descriptor
        {
            get
            {
                return new AutomationCommandDescriptor("builtin.replace", "置換",
                    AutomationContract.MajorVersion, AutomationContract.MinorVersion,
                    new[]
                    {
                        Parameter(BeforeParameter, "置換前"),
                        Parameter(AfterParameter, "置換後")
                    });
            }
        }

        public AutomationResult Execute(AutomationRequest request)
        {
            string before;
            string after;
            if (!request.Parameters.TryGetValue(BeforeParameter, out before) || before.Length == 0)
                return AutomationResult.Failure("変換前指定がない.");
            if (!request.Parameters.TryGetValue(AfterParameter, out after) || after.Length == 0)
                return AutomationResult.Failure("変換後指定がない.");

            List<AutomationChange> changes = new List<AutomationChange>();
            int column = request.Selection.Left;
            foreach (AutomationCell cell in request.Cells)
                if (cell.Column == column && cell.Value.Length != 0 && cell.Value == before)
                    changes.Add(new AutomationChange(cell.Row, cell.Column, after));
            return AutomationResult.Success(changes, null);
        }

        private static AutomationParameterDefinition Parameter(string id, string name)
        {
            return new AutomationParameterDefinition(id, name, AutomationParameterType.String,
                String.Empty, true, null, null, null);
        }
    }

    public sealed class ReverseCommand : IAutomationCommand
    {
        public AutomationCommandDescriptor Descriptor
        {
            get
            {
                return new AutomationCommandDescriptor("builtin.reverse", "反転",
                    AutomationContract.MajorVersion, AutomationContract.MinorVersion,
                    new AutomationParameterDefinition[0]);
            }
        }

        public AutomationResult Execute(AutomationRequest request)
        {
            int column = request.Selection.Left;
            List<AutomationCell> populatedCells = new List<AutomationCell>();
            foreach (AutomationCell cell in request.Cells)
                if (cell.Column == column && cell.Value.Length != 0)
                    populatedCells.Add(cell);
            populatedCells.Sort(delegate(AutomationCell left, AutomationCell right)
            {
                return left.Row.CompareTo(right.Row);
            });

            List<AutomationChange> changes = new List<AutomationChange>();
            for (int index = 0; index < populatedCells.Count; index++)
                changes.Add(new AutomationChange(populatedCells[index].Row, column,
                    populatedCells[populatedCells.Count - index - 1].Value));
            return AutomationResult.Success(changes, null);
        }
    }

    public sealed class ArithmeticCommand : IAutomationCommand
    {
        public const string OperatorParameter = "operator";
        public const string OperandParameter = "operand";

        public AutomationCommandDescriptor Descriptor
        {
            get
            {
                return new AutomationCommandDescriptor("builtin.arithmetic", "四則演算",
                    AutomationContract.MajorVersion, AutomationContract.MinorVersion,
                    new[]
                    {
                        new AutomationParameterDefinition(OperatorParameter, "演算子",
                            AutomationParameterType.Choice, "+", true, null, null,
                            new[] { "+", "-", "*", "/" }),
                        new AutomationParameterDefinition(OperandParameter, "値",
                            AutomationParameterType.Int32, "0", true, null, null, null)
                    });
            }
        }

        public AutomationResult Execute(AutomationRequest request)
        {
            string operation;
            string operandText;
            int operand;
            if (!request.Parameters.TryGetValue(OperatorParameter, out operation) ||
                (operation != "+" && operation != "-" && operation != "*" && operation != "/"))
                return AutomationResult.Failure("１文字目には\"+-*/\"記号のいずれか１文字の入力が必要");
            if (!request.Parameters.TryGetValue(OperandParameter, out operandText) ||
                !Int32.TryParse(operandText, out operand))
                return AutomationResult.Failure("入力された値を数値に変換できませんでした.");
            List<AutomationCell> cells = new List<AutomationCell>(request.Cells);
            cells.Sort(delegate(AutomationCell left, AutomationCell right)
            {
                int columnOrder = left.Column.CompareTo(right.Column);
                return columnOrder != 0 ? columnOrder : left.Row.CompareTo(right.Row);
            });

            List<AutomationChange> changes = new List<AutomationChange>();
            foreach (AutomationCell cell in cells)
            {
                if (cell.Value.Length == 0 || cell.Value == request.EmptyCellValue) continue;
                int current;
                if (!Int32.TryParse(cell.Value, out current))
                    return AutomationResult.Failure("セルの値を数値に変換できませんでした.");
                if ((operation == "*" || operation == "/") && current == 0) continue;
                if (operation == "/" && operand == 0)
                    return AutomationResult.Failure("0で除算することはできません.");

                int calculated;
                if (operation == "+") calculated = unchecked(current + operand);
                else if (operation == "-") calculated = unchecked(current - operand);
                else if (operation == "*") calculated = unchecked(current * operand);
                else calculated = current / operand;
                changes.Add(new AutomationChange(cell.Row, cell.Column,
                    calculated.ToString(CultureInfo.CurrentCulture)));
            }
            return AutomationResult.Success(changes, null);
        }
    }
}
