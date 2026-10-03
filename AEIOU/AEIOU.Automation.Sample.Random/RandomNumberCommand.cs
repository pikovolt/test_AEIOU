using System;
using System.Collections.Generic;
using System.Globalization;
using AEIOU.Automation;

namespace AEIOU.Automation.Sample.Random
{
    /// <summary>Example external command that fills every selected cell with a random integer.</summary>
    public sealed class RandomNumberCommand : IAutomationCommand
    {
        public const string CommandId = "sample.random-number";
        private readonly System.Random random;

        public RandomNumberCommand() : this(new System.Random()) { }

        internal RandomNumberCommand(System.Random random)
        {
            if (random == null) throw new ArgumentNullException("random");
            this.random = random;
        }

        public AutomationCommandDescriptor Descriptor
        {
            get
            {
                return new AutomationCommandDescriptor(CommandId, "ランダム整数",
                    AutomationContract.MajorVersion, AutomationContract.MinorVersion, new[]
                    {
                        new AutomationParameterDefinition("minimum", "最小値", AutomationParameterType.Int32,
                            "1", true, Int32.MinValue, Int32.MaxValue, null),
                        new AutomationParameterDefinition("maximum", "最大値", AutomationParameterType.Int32,
                            "100", true, Int32.MinValue, Int32.MaxValue, null)
                    });
            }
        }

        public AutomationResult Execute(AutomationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            int minimum;
            int maximum;
            if (!Int32.TryParse(request.Parameters["minimum"], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out minimum) ||
                !Int32.TryParse(request.Parameters["maximum"], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out maximum))
                return AutomationResult.Failure("最小値と最大値には整数を指定してください。");
            if (minimum > maximum)
                return AutomationResult.Failure("最小値は最大値以下にしてください。");

            List<AutomationChange> changes = new List<AutomationChange>();
            AutomationSelection selection = request.Selection;
            for (int column = selection.Left; column < selection.Left + selection.ColumnCount; column++)
                for (int row = selection.Top; row < selection.Top + selection.RowCount; row++)
                {
                    long range = (long)maximum - minimum + 1L;
                    int value = (int)(minimum + (long)(random.NextDouble() * range));
                    changes.Add(new AutomationChange(row, column,
                        value.ToString(CultureInfo.InvariantCulture)));
                }
            return AutomationResult.Success(changes, null);
        }
    }
}
