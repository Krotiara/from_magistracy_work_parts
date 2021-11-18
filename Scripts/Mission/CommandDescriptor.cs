using System;
using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Commands;

namespace CableWalker.Simulator.Mission
{
    public class CommandDescriptor
    {
        private readonly List<string> aliases;
        private readonly List<(Type, string)> args;

        public IEnumerable<string> Aliases => aliases;
        public IEnumerable<(Type, string)> Args => args;
        public Type CommandType { get; private set; }

        private CommandDescriptor()
        {
            aliases = new List<string>();
            args = new List<(Type, string)>();
        }

        public static CommandDescriptor Build<TCommand>() where TCommand : Command
        {
            var commandType = typeof(TCommand);
            var commandDescriptor = new CommandDescriptor { CommandType = commandType };

            var properties = commandType.GetProperties()
                .Where(p => p.CanRead);
            foreach (var property in properties)
                commandDescriptor.args.Add((property.PropertyType, property.Name));
            
            return commandDescriptor;
        }
        
        public CommandDescriptor AddAlias(string alias)
        {
            aliases.Add(alias);
            return this;
        }
    }
}
