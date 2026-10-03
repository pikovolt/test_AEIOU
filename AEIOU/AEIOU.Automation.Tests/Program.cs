using System;
using System.Collections.Generic;
using AEIOU.Automation;

namespace AEIOU.Automation.Tests
{
    internal static class Program
    {
        private static int failures;

        private static int Main()
        {
            Run("valid changes are applied once", ValidChangesAreAppliedOnce);
            Run("empty changes do not open a write group", EmptyChangesDoNotApply);
            Run("out-of-range result is rejected atomically", OutOfRangeIsRejected);
            Run("duplicate result is rejected atomically", DuplicateIsRejected);
            Run("null value is rejected atomically", NullValueIsRejected);
            Run("change limit is rejected atomically", ChangeLimitIsRejected);
            Run("generation mismatch is rejected atomically", GenerationMismatchIsRejected);
            Run("command exception is isolated", CommandExceptionIsIsolated);
            Run("built-in commands resolve through registry", BuiltInsResolveThroughRegistry);
            Run("registry rejects duplicate IDs", RegistryRejectsDuplicateIds);
            Run("host rejects unknown IDs", HostRejectsUnknownIds);
            Run("host validates descriptor parameters", HostValidatesDescriptorParameters);
            Run("replace matches only the left column exactly", ReplaceMatchesLeftColumnExactly);
            Run("replace rejects empty values", ReplaceRejectsEmptyValues);
            Run("reverse preserves empty positions", ReversePreservesEmptyPositions);
            Run("arithmetic handles all operators and ignored cells", ArithmeticHandlesOperatorsAndIgnoredCells);
            Run("arithmetic rejects invalid cells atomically", ArithmeticRejectsInvalidCellsAtomically);
            Run("arithmetic rejects division by zero", ArithmeticRejectsDivisionByZero);
            Run("arithmetic ignores division by zero for ignored cells", ArithmeticIgnoresDivisionByZeroForIgnoredCells);
            Run("sequential number preserves step and skip behavior", SequentialNumberPreservesBehavior);
            Run("sequential number rejects zero step", SequentialNumberRejectsZeroStep);
            Run("repeat number handles insert skip loop and columns", RepeatNumberHandlesParameters);
            Run("repeat number clears selection and rejects invalid ranges", RepeatNumberClearsAndValidates);
            Run("automation session replaces only its last result", AutomationSessionReplacesOnlyItsLastResult);
            Console.WriteLine(failures == 0 ? "All automation host tests passed." : failures + " test(s) failed.");
            return failures == 0 ? 0 : 1;
        }

        private static void ValidChangesAreAppliedOnce()
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = Host().Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "A"), new AutomationChange(1, 1, "B") }, "done");
            }), Request(), target);
            Assert(result.Succeeded && result.AppliedChangeCount == 2, "success result expected");
            Assert(target.ApplyCount == 1 && target.Values.Count == 2, "one complete write group expected");
        }

        private static void EmptyChangesDoNotApply()
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = Host().Execute(Command(delegate { return AutomationResult.Success(new AutomationChange[0], null); }), Request(), target);
            Assert(result.Succeeded && target.ApplyCount == 0, "empty result must not touch target");
        }

        private static void OutOfRangeIsRejected()
        {
            AssertRejectedWithoutWrites(new[] { new AutomationChange(0, 0, "valid"), new AutomationChange(2, 0, "invalid") });
        }

        private static void DuplicateIsRejected()
        {
            AssertRejectedWithoutWrites(new[] { new AutomationChange(0, 0, "A"), new AutomationChange(0, 0, "B") });
        }

        private static void NullValueIsRejected()
        {
            AssertRejectedWithoutWrites(new[] { new AutomationChange(0, 0, null) });
        }

        private static void ChangeLimitIsRejected()
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = new AutomationHost(1).Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "A"), new AutomationChange(0, 1, "B") }, null);
            }), Request(), target);
            Assert(!result.Succeeded && target.ApplyCount == 0, "oversized result must be atomic");
        }

        private static void GenerationMismatchIsRejected()
        {
            FakeTarget target = new FakeTarget(8);
            AutomationHostResult result = Host().Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "A") }, null);
            }), Request(), target);
            Assert(!result.Succeeded && target.ApplyCount == 0 && target.Values.Count == 0, "stale request must not write");
        }

        private static void CommandExceptionIsIsolated()
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = Host().Execute(Command(delegate { throw new InvalidOperationException("boom"); }), Request(), target);
            Assert(!result.Succeeded && result.Exception is InvalidOperationException && target.ApplyCount == 0, "exception must not escape or write");
        }

        private static void BuiltInsResolveThroughRegistry()
        {
            AutomationRegistry registry = BuiltInAutomationRegistry.Create();
            IAutomationCommand command;
            Assert(registry.TryGet(ReplaceCommand.CommandId, out command) && command is ReplaceCommand,
                "replace command should be registered by stable ID");
            Assert(registry.TryGet(ReverseCommand.CommandId, out command) && command is ReverseCommand,
                "reverse command should be registered by stable ID");
            Assert(registry.TryGet(ArithmeticCommand.CommandId, out command) && command is ArithmeticCommand,
                "arithmetic command should be registered by stable ID");
            Assert(registry.TryGet(SequentialNumberCommand.CommandId, out command) && command is SequentialNumberCommand,
                "sequential command should be registered by stable ID");
            Assert(registry.TryGet(RepeatNumberCommand.CommandId, out command) && command is RepeatNumberCommand,
                "repeat command should be registered by stable ID");
        }

        private static void RegistryRejectsDuplicateIds()
        {
            AutomationRegistry registry = new AutomationRegistry();
            string error;
            Assert(registry.TryRegister(Command(delegate { return AutomationResult.Success(new AutomationChange[0], null); }), out error),
                "first registration should succeed");
            Assert(!registry.TryRegister(Command(delegate { return AutomationResult.Success(new AutomationChange[0], null); }), out error) &&
                error.IndexOf("already registered") >= 0, "duplicate ID should be rejected without replacement");
        }

        private static void HostRejectsUnknownIds()
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = new AutomationHost(10, new AutomationRegistry()).Execute(
                "missing.command", Request(), target);
            Assert(!result.Succeeded && target.ApplyCount == 0, "unknown IDs must not execute or write");
        }

        private static void HostValidatesDescriptorParameters()
        {
            AutomationRegistry registry = new AutomationRegistry();
            ParameterCommand command = new ParameterCommand();
            string error;
            Assert(registry.TryRegister(command, out error), "parameter command registration should succeed");
            AutomationHost host = new AutomationHost(10, registry);
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult missing = host.Execute(ParameterCommand.CommandId, Request(), target);
            AutomationHostResult invalid = host.Execute(ParameterCommand.CommandId,
                Request(Parameters("count", "not-an-int", "enabled", "true")), target);
            AutomationHostResult unknown = host.Execute(ParameterCommand.CommandId,
                Request(Parameters("count", "1", "extra", "value")), target);
            Assert(!missing.Succeeded && !invalid.Succeeded && !unknown.Succeeded,
                "missing, malformed, and unknown parameters should be rejected");
            Assert(command.ExecuteCount == 0 && target.ApplyCount == 0,
                "invalid parameters must be rejected before command execution");
        }

        private static void ReplaceMatchesLeftColumnExactly()
        {
            AutomationResult result = new ReplaceCommand().Execute(CommandRequest(
                new[] { "1", "10", "1", "", "1", "1", "1", "1" },
                Parameters(ReplaceCommand.BeforeParameter, "1", ReplaceCommand.AfterParameter, "X")));
            Assert(result.Succeeded && result.Changes.Count == 2, "only exact matches in the left column should change");
            Assert(result.Changes[0].Row == 0 && result.Changes[0].Column == 0 && result.Changes[0].Value == "X", "first match expected");
            Assert(result.Changes[1].Row == 2 && result.Changes[1].Column == 0 && result.Changes[1].Value == "X", "second match expected");
        }

        private static void ReplaceRejectsEmptyValues()
        {
            AutomationResult missingBefore = new ReplaceCommand().Execute(CommandRequest(
                new[] { "1" }, Parameters(ReplaceCommand.BeforeParameter, "", ReplaceCommand.AfterParameter, "X"), 1, 1));
            AutomationResult missingAfter = new ReplaceCommand().Execute(CommandRequest(
                new[] { "1" }, Parameters(ReplaceCommand.BeforeParameter, "1", ReplaceCommand.AfterParameter, ""), 1, 1));
            Assert(!missingBefore.Succeeded && !missingAfter.Succeeded, "both empty parameters must be rejected");
        }

        private static void ReversePreservesEmptyPositions()
        {
            AutomationResult result = new ReverseCommand().Execute(CommandRequest(
                new[] { "1", "", "2", "", "3", "A", "B", "C", "D", "E" },
                new Dictionary<string, string>(), 5, 2));
            Assert(result.Succeeded && result.Changes.Count == 3, "only populated cells in the left column should be written");
            Assert(result.Changes[0].Row == 0 && result.Changes[0].Value == "3", "first value should be reversed");
            Assert(result.Changes[1].Row == 2 && result.Changes[1].Value == "2", "empty positions should remain empty");
            Assert(result.Changes[2].Row == 4 && result.Changes[2].Value == "1", "last value should be reversed");
        }

        private static void ArithmeticHandlesOperatorsAndIgnoredCells()
        {
            AutomationResult added = Arithmetic("+", "3", new[] { "1", "-2", "", "KARA" });
            AutomationResult subtracted = Arithmetic("-", "3", new[] { "1", "-2", "", "KARA" });
            AutomationResult multiplied = Arithmetic("*", "4", new[] { "2", "-3", "0", "KARA" });
            AutomationResult divided = Arithmetic("/", "2", new[] { "5", "-5", "0", "KARA" });
            Assert(Values(added) == "4,1", "addition values differ");
            Assert(Values(subtracted) == "-2,-5", "subtraction values differ");
            Assert(Values(multiplied) == "8,-12", "multiplication should skip zero and KARA");
            Assert(Values(divided) == "2,-2", "integer division should skip zero and KARA");
        }

        private static void ArithmeticRejectsInvalidCellsAtomically()
        {
            AutomationResult result = Arithmetic("+", "1", new[] { "1", "X", "2" });
            Assert(!result.Succeeded && result.Changes.Count == 0, "a nonnumeric cell must reject the entire result");
        }

        private static void ArithmeticRejectsDivisionByZero()
        {
            AutomationResult result = Arithmetic("/", "0", new[] { "1", "0", "KARA" });
            Assert(!result.Succeeded && result.Changes.Count == 0, "division by zero must be a validation failure");
        }

        private static void ArithmeticIgnoresDivisionByZeroForIgnoredCells()
        {
            AutomationResult result = Arithmetic("/", "0", new[] { "0", "", "KARA" });
            Assert(result.Succeeded && result.Changes.Count == 0, "zero, empty and KARA cells should be ignored before division");
        }

        private static void SequentialNumberPreservesBehavior()
        {
            AutomationResult compact = Sequential("10", "2", "false", 4);
            AutomationResult skipped = Sequential("10", "2", "true", 4);
            AutomationResult negative = Sequential("5", "-2", "true", 4);
            Assert(Values(compact) == "10,11", "step controls row interval when skip is off");
            Assert(Values(skipped) == "10,12", "skip should also apply step to the number");
            Assert(Values(negative) == "5,3", "negative steps should preserve legacy numbering");
        }

        private static void SequentialNumberRejectsZeroStep()
        {
            AutomationResult result = Sequential("1", "0", "false", 4);
            Assert(!result.Succeeded && result.Changes.Count == 0, "S-08 must be an input error");
        }

        private static AutomationResult Sequential(string start, string step, string skip, int rows)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(SequentialNumberCommand.StartParameter, start);
            parameters.Add(SequentialNumberCommand.StepParameter, step);
            parameters.Add(SequentialNumberCommand.SkipParameter, skip);
            string[] values = new string[rows];
            for (int index = 0; index < values.Length; index++) values[index] = String.Empty;
            return new SequentialNumberCommand().Execute(CommandRequest(values, parameters, rows, 1));
        }

        private static void RepeatNumberHandlesParameters()
        {
            AutomationResult inserted = Repeat("1", "2", "1", "1", "0", "X", 4, 1);
            AutomationResult skipped = Repeat("1", "4", "1", "1", "1", "", 3, 1);
            AutomationResult columns = Repeat("1", "3", "1", "1", "0", "", 3, 2);
            Assert(Values(inserted) == "1,X,2,X", "insert values should alternate with numbers");
            Assert(Values(skipped) == "1,3,1", "skip behavior should match P-03");
            Assert(Values(columns) == "1,2,3,1,2,3", "numbering should continue and wrap across columns");
        }

        private static void RepeatNumberClearsAndValidates()
        {
            AutomationResult cleared = Repeat("3", "3", "1", "1", "0", "", 3, 1);
            Assert(cleared.Succeeded && Values(cleared) == "3,,", "unused selected cells should be cleared in the same result");
            Assert(!Repeat("1", "3", "0", "1", "0", "", 3, 1).Succeeded, "P-08 must reject zero interval");
            Assert(!Repeat("3", "1", "1", "1", "0", "", 3, 1).Succeeded, "P-09 must reject descending ranges");
            Assert(!Repeat("1", "3", "2", "1", "0", "", 3, 1).Succeeded, "P-10 must reject out-of-range writes atomically");
        }

        private static AutomationResult Repeat(string start, string end, string interval,
            string loop, string skip, string insert, int rows, int columns)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(RepeatNumberCommand.StartParameter, start);
            parameters.Add(RepeatNumberCommand.EndParameter, end);
            parameters.Add(RepeatNumberCommand.RowIntervalParameter, interval);
            parameters.Add(RepeatNumberCommand.LoopParameter, loop);
            parameters.Add(RepeatNumberCommand.SkipParameter, skip);
            parameters.Add(RepeatNumberCommand.InsertParameter, insert);
            string[] values = new string[rows * columns];
            for (int index = 0; index < values.Length; index++) values[index] = "old";
            return new RepeatNumberCommand().Execute(CommandRequest(values, parameters, rows, columns));
        }

        private static void AutomationSessionReplacesOnlyItsLastResult()
        {
            SessionTarget target = new SessionTarget(7);
            AutomationSession session = new AutomationSession(Host());
            AutomationHostResult first = session.Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "first") }, null);
            }), Request(), target);
            AutomationHostResult second = session.Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "second") }, null);
            }), Request(), target);
            Assert(first.Succeeded && second.Succeeded && target.ReplaceCount == 2,
                "a session should replace its own first application");
            target.AllowPrevious = false;
            AutomationHostResult blocked = session.Execute(Command(delegate
            {
                return AutomationResult.Success(new[] { new AutomationChange(0, 0, "third") }, null);
            }), Request(), target);
            Assert(!blocked.Succeeded && target.ReplaceCount == 2,
                "an intervening operation must prevent a session from undoing arbitrary history");
        }

        private static AutomationResult Arithmetic(string operation, string operand, string[] values)
        {
            return new ArithmeticCommand().Execute(CommandRequest(values,
                Parameters(ArithmeticCommand.OperatorParameter, operation, ArithmeticCommand.OperandParameter, operand),
                values.Length, 1));
        }

        private static string Values(AutomationResult result)
        {
            List<string> values = new List<string>();
            foreach (AutomationChange change in result.Changes) values.Add(change.Value);
            return String.Join(",", values.ToArray());
        }

        private static Dictionary<string, string> Parameters(string firstKey, string firstValue,
            string secondKey, string secondValue)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(firstKey, firstValue);
            parameters.Add(secondKey, secondValue);
            return parameters;
        }

        private static AutomationRequest CommandRequest(string[] values, IDictionary<string, string> parameters)
        {
            return CommandRequest(values, parameters, 4, 2);
        }

        private static AutomationRequest CommandRequest(string[] values, IDictionary<string, string> parameters,
            int rows, int columns)
        {
            List<AutomationCell> cells = new List<AutomationCell>();
            int index = 0;
            for (int column = 0; column < columns; column++)
                for (int row = 0; row < rows; row++)
                    cells.Add(new AutomationCell(row, column, values[index++]));
            return new AutomationRequest(rows, columns, 1, new AutomationSelection(0, 0, rows, columns),
                cells, parameters, "KARA");
        }

        private static void AssertRejectedWithoutWrites(IEnumerable<AutomationChange> changes)
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = Host().Execute(Command(delegate { return AutomationResult.Success(changes, null); }), Request(), target);
            Assert(!result.Succeeded && target.ApplyCount == 0 && target.Values.Count == 0, "invalid set must write zero cells");
        }

        private static AutomationHost Host() { return new AutomationHost(10); }

        private static AutomationRequest Request()
        {
            return Request(new Dictionary<string, string>());
        }

        private static AutomationRequest Request(IDictionary<string, string> parameters)
        {
            return new AutomationRequest(2, 2, 7, new AutomationSelection(0, 0, 2, 2),
                new[] { new AutomationCell(0, 0, "") }, parameters, "KARA");
        }

        private static IAutomationCommand Command(Func<AutomationResult> execute) { return new FakeCommand(execute); }

        private static void Run(string name, Action test)
        {
            try { test(); Console.WriteLine("PASS: " + name); }
            catch (Exception exception) { failures++; Console.Error.WriteLine("FAIL: " + name + " - " + exception.Message); }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private sealed class FakeCommand : IAutomationCommand
        {
            private readonly Func<AutomationResult> execute;
            public FakeCommand(Func<AutomationResult> execute) { this.execute = execute; }
            public AutomationCommandDescriptor Descriptor
            {
                get { return new AutomationCommandDescriptor("test.command", "Test", 1, 0, new AutomationParameterDefinition[0]); }
            }
            public AutomationResult Execute(AutomationRequest request) { return execute(); }
        }

        private sealed class ParameterCommand : IAutomationCommand
        {
            public const string CommandId = "test.parameters";
            public int ExecuteCount;
            public AutomationCommandDescriptor Descriptor
            {
                get
                {
                    return new AutomationCommandDescriptor(CommandId, "Parameters", 1, 0, new[]
                    {
                        new AutomationParameterDefinition("count", "Count", AutomationParameterType.Int32,
                            "1", true, 0, 10, null),
                        new AutomationParameterDefinition("enabled", "Enabled", AutomationParameterType.Boolean,
                            "false", true, null, null, null)
                    });
                }
            }
            public AutomationResult Execute(AutomationRequest request)
            {
                ExecuteCount++;
                return AutomationResult.Success(new AutomationChange[0], null);
            }
        }

        private sealed class FakeTarget : IAutomationChangeTarget
        {
            private readonly long generation;
            public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
            public int ApplyCount;
            public FakeTarget(long generation) { this.generation = generation; }
            public bool TryApply(long expectedGeneration, IList<AutomationChange> changes, string operationName)
            {
                if (expectedGeneration != generation) return false;
                ApplyCount++;
                foreach (AutomationChange change in changes) Values[change.Row + ":" + change.Column] = change.Value;
                return true;
            }
        }

        private sealed class SessionTarget : IAutomationSessionTarget
        {
            private readonly long generation;
            private object lastApplication;
            public bool AllowPrevious = true;
            public int ReplaceCount;

            public SessionTarget(long generation) { this.generation = generation; }

            public bool TryReplace(long expectedGeneration, object previousApplication,
                IList<AutomationChange> changes, string operationName, out object application)
            {
                application = null;
                if (expectedGeneration != generation || previousApplication != null &&
                    (!AllowPrevious || !Object.ReferenceEquals(previousApplication, lastApplication))) return false;
                application = new object();
                lastApplication = application;
                ReplaceCount++;
                return true;
            }
        }
    }
}
