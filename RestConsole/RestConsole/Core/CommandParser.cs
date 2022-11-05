using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestConsole.Core;

public static class CommandParser
{
    #region Regex
    private static readonly Regex CmdLineRegex = new Regex(
        "^(?<command>SETUP|GET|POST|PUT|PATCH|DELETE)\\s+(?<path>.+" +
        ")",
         RegexOptions.CultureInvariant
        | RegexOptions.Compiled
    );
    #endregion

    public static IEnumerable<Command> Parse(string source)
    {
        using var stringReader = new StringReader(source);

        var lineIndex = 0;
        var matches = new LinkedList<(int LineIndex, Match Match)>();
        while (stringReader.ReadLine() is { } line)
        {
            var match = CmdLineRegex.Match(line);
            if (match.Success)
            {
                matches.AddLast((lineIndex, match));
            }

            lineIndex++;
        }

        var currentMatchedLine = matches.First;
        while (currentMatchedLine != null)
        {
            switch (currentMatchedLine.Value.Match.Groups["command"].Value)
            {
                case "SETUP":
                    yield return new SetupCommand(currentMatchedLine.Value.Match,
                        currentMatchedLine.Next != null
                            ? source.Substring(currentMatchedLine.Value.Match.Index, currentMatchedLine.Next.Value.Match.Index - currentMatchedLine.Value.Match.Index)
                            : source[currentMatchedLine.Value.Match.Index..], currentMatchedLine.Value.LineIndex, currentMatchedLine.Next?.Value.LineIndex);
                    break;
                case "GET":
                    yield return new GetCommand(currentMatchedLine.Value.Match,
                        currentMatchedLine.Next != null
                            ? source.Substring(currentMatchedLine.Value.Match.Index, currentMatchedLine.Next.Value.Match.Index - currentMatchedLine.Value.Match.Index)
                            : source[currentMatchedLine.Value.Match.Index..], currentMatchedLine.Value.LineIndex, currentMatchedLine.Next?.Value.LineIndex);
                    break;
            }

            currentMatchedLine = currentMatchedLine.Next;
        }
    }
}

public abstract class Command
{
    public int StartLineIndex { get; }
    public int? EndLineIndex { get; }
    public string? Body { get; }
    public string? Path { get; }

    protected static readonly Regex _parametersRegex = new Regex(
      "\\{\\{(?<name>[^:]+)(\\:(?<type>.+)){0,1}\\}\\}",
        RegexOptions.Multiline
        | RegexOptions.CultureInvariant
        | RegexOptions.Compiled
        );


    public Command(int startLineIndex, string? path = null, int? endLineIndex = null, string? body = null)
    {
        StartLineIndex = startLineIndex;
        EndLineIndex = endLineIndex;
        Path = path;
        Body = body;
    }

    public virtual IEnumerable<CommandParameter> GetParameters()
    {
        if (!string.IsNullOrWhiteSpace(Path))
        {
            foreach (var match in _parametersRegex.Matches(Path).Cast<Match>())
            {
                yield return new CommandParameter(match.Groups["name"].Value, CommandParameter.GetType(match.Groups["type"].Value));
            }
        }
        if (!string.IsNullOrWhiteSpace(Body))
        {
            foreach (var match in _parametersRegex.Matches(Body).Cast<Match>())
            {
                yield return new CommandParameter(match.Groups["name"].Value, CommandParameter.GetType(match.Groups["type"].Value));
            }
        }
    }
}

public class SetupCommand : Command
{
    public SetupCommand(Match commandMatch, string body, int startLineIndex, int? endLineIndex)
        : base(startLineIndex, commandMatch.Groups["path"].Value, endLineIndex, body)
    {
    }

    public override string ToString()
    {
        return $"CONNECT {Path}";
    }

}

public abstract class PathCommand : Command
{
    public PathCommand(Match commandMatch, string bodyString, int startLineIndex, int? endLineIndex)
        : base(startLineIndex, commandMatch.Groups["path"].Value, endLineIndex, bodyString)
    {
    }
}

public class GetCommand : PathCommand
{
    public GetCommand(Match commandMatch, string bodyString, int startLineIndex, int? endLineIndex)
        : base(commandMatch, bodyString, startLineIndex, endLineIndex)
    {
    }

    public override string ToString()
    {
        return $"GET {Path}";
    }
}


public enum CommandParameterType
{
    String,

    Password,

    Int,

    Double,

    DateTime,

    Date,

    TimeSpan,
}

public record CommandParameter(string Name, CommandParameterType Type, string? Description = null)
{ 
    public static CommandParameterType GetType(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return CommandParameterType.String;
        }

        return Enum.Parse<CommandParameterType>(type, true);
    }
};