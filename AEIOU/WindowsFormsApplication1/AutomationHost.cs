using System;
using System.Collections.Generic;
using AEIOU.Automation;

namespace AEIOU
{
    /// <summary>Executes untrusted command output only after validating the complete change set.</summary>
    public sealed class AutomationHost
    {
        private readonly int maximumChangeCount;

        public AutomationHost(int maximumChangeCount)
        {
            if (maximumChangeCount <= 0) throw new ArgumentOutOfRangeException("maximumChangeCount");
            this.maximumChangeCount = maximumChangeCount;
        }

        public AutomationHostResult Execute(IAutomationCommand command, AutomationRequest request,
            IAutomationChangeTarget target)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (request == null) throw new ArgumentNullException("request");
            if (target == null) throw new ArgumentNullException("target");

            AutomationCommandDescriptor descriptor = command.Descriptor;
            if (descriptor == null)
                return AutomationHostResult.Rejected("The command has no descriptor.");
            if (descriptor.ContractMajorVersion != AutomationContract.MajorVersion ||
                descriptor.ContractMinorVersion > AutomationContract.MinorVersion)
                return AutomationHostResult.Rejected("The command contract version is not supported.");

            AutomationResult result;
            try
            {
                result = command.Execute(request);
            }
            catch (Exception exception)
            {
                return AutomationHostResult.Faulted(descriptor.Id, exception);
            }

            string validationError = Validate(result, request);
            if (validationError != null)
                return AutomationHostResult.Rejected(validationError);
            if (!result.Succeeded)
                return AutomationHostResult.Rejected(result.Error);
            if (result.Changes.Count == 0)
                return AutomationHostResult.Applied(0, result.Message);

            // The target performs the generation comparison and write as one operation so that
            // the sheet cannot change between the final check and the write group.
            if (!target.TryApply(request.SheetGeneration, result.Changes, descriptor.DisplayName))
                return AutomationHostResult.Rejected("The sheet changed while the command was running.");
            return AutomationHostResult.Applied(result.Changes.Count, result.Message);
        }

        private string Validate(AutomationResult result, AutomationRequest request)
        {
            if (result == null) return "The command returned no result.";
            if (!result.Succeeded)
                return String.IsNullOrEmpty(result.Error) ? "The command failed without an error message." : null;
            if (result.Changes.Count > maximumChangeCount)
                return "The command returned too many changes.";

            HashSet<string> coordinates = new HashSet<string>(StringComparer.Ordinal);
            foreach (AutomationChange change in result.Changes)
            {
                if (change == null) return "The change set contains a null change.";
                if (change.Row < 0 || change.Row >= request.RowCount ||
                    change.Column < 0 || change.Column >= request.ColumnCount)
                    return "The change set contains an out-of-range cell.";
                if (change.Value == null) return "The change set contains a null value.";
                string coordinate = change.Row.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" +
                    change.Column.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (!coordinates.Add(coordinate)) return "The change set contains a duplicate cell.";
            }
            return null;
        }
    }

    public interface IAutomationChangeTarget
    {
        bool TryApply(long expectedGeneration, IList<AutomationChange> changes, string operationName);
    }

    public sealed class AutomationHostResult
    {
        private AutomationHostResult(bool succeeded, int appliedChangeCount, string error, string message, Exception exception)
        {
            Succeeded = succeeded;
            AppliedChangeCount = appliedChangeCount;
            Error = error;
            Message = message;
            Exception = exception;
        }

        public bool Succeeded { get; private set; }
        public int AppliedChangeCount { get; private set; }
        public string Error { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        internal static AutomationHostResult Applied(int count, string message)
        {
            return new AutomationHostResult(true, count, null, message, null);
        }

        internal static AutomationHostResult Rejected(string error)
        {
            return new AutomationHostResult(false, 0, error, null, null);
        }

        internal static AutomationHostResult Faulted(string commandId, Exception exception)
        {
            return new AutomationHostResult(false, 0,
                "Automation command '" + commandId + "' failed.", null, exception);
        }
    }
}
