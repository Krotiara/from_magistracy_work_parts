using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CableWalker.Simulator.Mission.Commands;
using UnityEngine;

namespace CableWalker.Simulator.Mission.Generator
{
    public static class CommandFormatterExtension
    {
        public static string Format(this Command command)
        {
            var descriptor = CommandManager.GetDescriptor(command);
            var alias = descriptor.Aliases.First();
            
            var builder = new StringBuilder();
            builder.Append(alias);
            builder.Append('(');

            var first = true;

            foreach (var (type, name) in descriptor.Args)
            {
                if (!first)
                    builder.Append(", ");
                first = false;
                
                var property = descriptor.CommandType.GetProperty(name);
                
                if (property == null)
                {
                    Debug.LogError($"Поле {name} не найдено.");
                    return null;
                }
                
                if (property.PropertyType != type)
                {
                    Debug.LogError($"Поле {name} имеет неверный тип.");
                    return null;
                }

                var value = property.GetValue(command);
                if (value is string)
                {
                    builder.Append("\"");
                    builder.Append(value);
                    builder.Append("\"");
                }
                else if (value is float fl)
                    builder.Append(fl.ToString(new NumberFormatInfo { NumberDecimalSeparator = "." }));
                else
                    builder.Append(value);
            }

            builder.Append(')');

            return builder.ToString();
        }

        public static string Format(this IEnumerable<Command> commands)
        {
            var builder = new StringBuilder();
            foreach (var command in commands)
                builder.AppendLine(command.Format());
            return builder.ToString();
        }
    }
}
