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

        private static void AssertRejectedWithoutWrites(IEnumerable<AutomationChange> changes)
        {
            FakeTarget target = new FakeTarget(7);
            AutomationHostResult result = Host().Execute(Command(delegate { return AutomationResult.Success(changes, null); }), Request(), target);
            Assert(!result.Succeeded && target.ApplyCount == 0 && target.Values.Count == 0, "invalid set must write zero cells");
        }

        private static AutomationHost Host() { return new AutomationHost(10); }

        private static AutomationRequest Request()
        {
            return new AutomationRequest(2, 2, 7, new AutomationSelection(0, 0, 2, 2),
                new[] { new AutomationCell(0, 0, "") }, new Dictionary<string, string>(), "KARA");
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
    }
}
