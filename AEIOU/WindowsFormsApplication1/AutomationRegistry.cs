using System;
using System.Collections.Generic;
using AEIOU.Automation;

namespace AEIOU
{
    /// <summary>Stores automation commands by their stable descriptor ID.</summary>
    public sealed class AutomationRegistry
    {
        private readonly Dictionary<string, IAutomationCommand> commands =
            new Dictionary<string, IAutomationCommand>(StringComparer.Ordinal);

        public bool TryRegister(IAutomationCommand command, out string error)
        {
            if (command == null) throw new ArgumentNullException("command");
            AutomationCommandDescriptor descriptor = command.Descriptor;
            if (descriptor == null)
            {
                error = "The command has no descriptor.";
                return false;
            }
            if (commands.ContainsKey(descriptor.Id))
            {
                error = "Automation command ID '" + descriptor.Id + "' is already registered.";
                return false;
            }
            commands.Add(descriptor.Id, command);
            error = null;
            return true;
        }

        public bool TryGet(string commandId, out IAutomationCommand command)
        {
            if (commandId == null) throw new ArgumentNullException("commandId");
            return commands.TryGetValue(commandId, out command);
        }
    }

    public static class BuiltInAutomationRegistry
    {
        public static AutomationRegistry Create()
        {
            AutomationRegistry registry = new AutomationRegistry();
            Register(registry, new ReplaceCommand());
            Register(registry, new ReverseCommand());
            Register(registry, new ArithmeticCommand());
            Register(registry, new SequentialNumberCommand());
            Register(registry, new RepeatNumberCommand());
            return registry;
        }

        private static void Register(AutomationRegistry registry, IAutomationCommand command)
        {
            string error;
            if (!registry.TryRegister(command, out error)) throw new InvalidOperationException(error);
        }
    }
}
