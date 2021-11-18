using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CableWalker.Simulator.Mission.Parser
{
    public static class CommandParser
    {
        private static string TrimStart(string line, ref int charIndex)
        {
            var trimmedLine = line.TrimStart();
            charIndex += line.Length - trimmedLine.Length;
            return trimmedLine;
        }

        public class ParseResult
        {
            public ParseResult(ParseStatus status, int breakChar, Type command, object[] args)
            {
                Status = status;
                BreakChar = breakChar;
                Command = command;
                Args = args;
            }
            
            public ParseStatus Status { get; }
            public int BreakChar { get; }
            public Type Command { get; }
            public object[] Args { get; }
        }

        public static ParseResult ParseLine(string line)
        {
            if (line.Trim() == string.Empty)
                return new ParseResult(ParseStatus.Empty, 0, null, null);

            var charIndex = 0;
            Type command = null;
            var argsList = new List<object>();

            var alias = string.Empty;
            for (var i = line.Length; i > 0; i--)
            {
                alias = line.Substring(0, i);
                command = CommandManager.GetCommandTypeByAlias(alias);
                if (command != null)
                    break;
            }

            if (command == null)
                return new ParseResult(ParseStatus.UnknownCommand, charIndex, null, null);
            charIndex += alias.Length;
            line = line.Substring(alias.Length);

            line = TrimStart(line, ref charIndex);
            
            if (line.Length == 0)
                return new ParseResult(ParseStatus.UnexpectedLineEnd, charIndex, null, null);
            
            if (line[0] != '(')
                return new ParseResult(ParseStatus.MissingOpenBracket, charIndex, null, null);
            line = line.Substring(1);
            charIndex++;
            
            var argLimiter = 0;
            while (argLimiter < byte.MaxValue)
            {
                argLimiter++;
                
                line = TrimStart(line, ref charIndex);
                            
                if (line.Length == 0)
                    return new ParseResult(ParseStatus.UnexpectedLineEnd, charIndex, null, null);
                
                if (line.StartsWith("true"))
                {
                    line = line.Substring(4);
                    charIndex += 4;
                    argsList.Add(true);
                }
                else if (line.StartsWith("false"))
                {
                    line = line.Substring(5);
                    charIndex += 5;
                    argsList.Add(false);
                }
                else if (line.StartsWith("\""))
                {
                    var str = new StringBuilder();
                    line = line.Substring(1);
                    charIndex++;
                    while (!line.StartsWith("\""))
                    {
                        if (line.Length == 0)
                            return new ParseResult(ParseStatus.UnexpectedLineEnd, charIndex, null, null);
                        
                        str.Append(line[0]);
                        line = line.Substring(1);
                        charIndex++;
                    }
                    line = line.Substring(1);
                    charIndex++;
                    argsList.Add(str.ToString());
                }
                else
                {
                    var maxFloatLength = line.Length;
                    var floatValue = 0.0f;
                    var formatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };
                    while (maxFloatLength > 0 && !float.TryParse(line.Substring(0, maxFloatLength), NumberStyles.Float, formatInfo, out floatValue))
                        maxFloatLength--;

                    var maxIntLength = line.Length;
                    var intValue = 0;
                    while (maxIntLength > 0 && !int.TryParse(line.Substring(0, maxIntLength), out intValue))
                        maxIntLength--;

                    var wasNumber = false;
                    if (maxIntLength > 0)
                    {
                        if (maxFloatLength > 0)
                        {
                            if (intValue == floatValue)
                            {
                                argsList.Add(intValue);
                                line = line.Substring(maxIntLength);
                                wasNumber = true;
                            }
                        }
                        else
                        {
                            argsList.Add(intValue);
                            line = line.Substring(maxIntLength);
                            wasNumber = true;
                        }
                    }

                    if (!wasNumber && maxFloatLength > 0)
                    {
                        argsList.Add(floatValue);
                        line = line.Substring(maxFloatLength);
                    }
                }

                line = TrimStart(line, ref charIndex);
                
                if (line.Length == 0)
                    return new ParseResult(ParseStatus.UnexpectedLineEnd, charIndex, null, null);

                if (line[0] == ',')
                {
                    line = line.Substring(1);
                    charIndex++;
                }
                else
                    break;
            }

            line = TrimStart(line, ref charIndex);
            
            if (line[0] != ')')
                return new ParseResult(ParseStatus.MissingCloseBracket, charIndex, null, null);

            return new ParseResult(ParseStatus.Correct, line.Length, command, argsList.ToArray());
        }
    }
}
