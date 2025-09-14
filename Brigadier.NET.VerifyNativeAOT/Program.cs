using System;
using System.IO;
using System.Linq;
using System.Text;
using Brigadier.NET;
using Brigadier.NET.Builder;
using Brigadier.NET.Exceptions;
using Brigadier.NET.ArgumentTypes;
using static Brigadier.NET.Arguments;
using Spectre.Console;

AnsiConsole.MarkupLine("[green]Brigadier.NET VerifyNativeAOT - Filesystem Browser (Brigadier commands)[/]");
AnsiConsole.MarkupLine("Type commands like: [yellow]ls All .[/], [yellow]cd ..[/], [yellow]readfile file.txt[/]. Type [yellow]help[/] or [yellow]exit[/].");

var source = new ConsoleSource();
var dispatcher = BuildDispatcher();

while (true)
{
    var input = AnsiConsole.Ask<string>("[blue]cmd[/]:");
    if (string.IsNullOrWhiteSpace(input))
        continue;
    if (string.Equals(input.Trim(), "exit", StringComparison.OrdinalIgnoreCase))
        break;

    try
    {
        dispatcher.Execute(input, source);
    }
    catch (CommandSyntaxException ex)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
        if (ex.Input != null && ex.Cursor >= 0)
        {
            // Escape input to avoid markup parsing errors
            AnsiConsole.MarkupLine(MarkupEscaped(ex.Input));
            AnsiConsole.MarkupLine(new string(' ', ex.Cursor) + "^");
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]Error: {MarkupEscaped(ex.Message)}[/]");
    }
}

AnsiConsole.MarkupLine("[green]Goodbye![/]");

// -----------------------------
// Build Brigadier dispatcher
// -----------------------------
CommandDispatcher<ConsoleSource> BuildDispatcher()
{
    var d = new CommandDispatcher<ConsoleSource>();

    // ls <type> [path]
    d.Register(ctx => ctx
        .Literal("ls")
        .Then(ctx.Argument("type", new EnumArgumentType<EntryType>())
            .Then(ctx.Argument("path", String())
                .Executes(c =>
                {
                    var type = c.GetArgument<EntryType>("type");
                    var path = GetString(c, "path");
                    var resolved = ResolvePath(path, c.Source.CurrentDirectory);
                    ListDirectory(resolved, type);
                    return 1;
                }))
            .Executes(c =>
            {
                var type = c.GetArgument<EntryType>("type");
                ListDirectory(c.Source.CurrentDirectory, type);
                return 1;
            })));

    // cd <path>
    d.Register(ctx => ctx
        .Literal("cd")
        .Then(ctx.Argument("path", GreedyString())
            .Executes(c =>
            {
                var path = GetString(c, "path");
                var resolved = ResolvePath(path, c.Source.CurrentDirectory);
                if (!Directory.Exists(resolved))
                {
                    AnsiConsole.MarkupLine("[yellow]Directory not found.[/]");
                    return 0;
                }
                c.Source.CurrentDirectory = resolved;
                // Show cwd with escaped content to avoid markup errors
                AnsiConsole.MarkupLine($"[green]cwd[/]: [grey]{MarkupEscaped(c.Source.CurrentDirectory)}[/]");
                return 1;
            })));
    // readfile <path>
    d.Register(ctx => ctx
        .Literal("readfile")
        .Then(ctx.Argument("path", GreedyString())
            .Executes(c =>
            {
                var path = GetString(c, "path");
                var resolved = ResolvePath(path, c.Source.CurrentDirectory);
                if (!File.Exists(resolved))
                {
                    AnsiConsole.MarkupLine("[yellow]File not found.[/]");
                    return 0;
                }
                ShowFilePreview(resolved);
                return 1;
            })));
    // help
    d.Register(ctx => ctx
        .Literal("help")
        .Executes(c =>
        {
            var usages = d.GetAllUsage(d.Root, c.Source, true);
            AnsiConsole.MarkupLine("[green]Available commands:[/]");
            foreach (var u in usages.Distinct().OrderBy(s => s))
                AnsiConsole.MarkupLine("  " + u);
            AnsiConsole.MarkupLine("Special: exit");
            return usages.Length;
        }));
    // default: allow plain 'ls' to list current dir (no type) -> show All
    d.Register(ctx => ctx
        .Literal("ls")
        .Executes(c =>
        {
            ListDirectory(c.Source.CurrentDirectory, EntryType.All);
            return 1;
        }));

    return d;
}

// -----------------------------
// Helpers
// -----------------------------
static string ResolvePath(string input, string cwd)
{
    if (string.IsNullOrWhiteSpace(input))
        return cwd;

    if (input.StartsWith("~"))
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        input = Path.Combine(home, input[1..]);
    }

    return Path.GetFullPath(Path.IsPathRooted(input) ? input : Path.Combine(cwd, input));
}

static void ListDirectory(string path, EntryType type)
{
    try
    {
        var dirs = Directory.EnumerateDirectories(path).Select(x => new FileSystemEntry(x, true));
        var files = Directory.EnumerateFiles(path).Select(x => new FileSystemEntry(x, false));

        var entries = type switch
        {
            EntryType.Directory => dirs,
            EntryType.File => files,
            _ => dirs.Concat(files)
        };

        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Type");
        table.AddColumn("Size");
        table.AddColumn("Modified");

        foreach (var e in entries.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
        {
            table.AddRow(
                MarkupEscaped(e.Name),
                e.IsDirectory ? "[blue]Directory[/]" : "[green]File[/]",
                e.IsDirectory ? "-" : e.Size.ToString(),
                e.Modified.ToString("yyyy-MM-dd HH:mm:ss")
            );
        }

        AnsiConsole.Write(table);
    }
    catch (UnauthorizedAccessException)
    {
        AnsiConsole.MarkupLine("[red]Access denied.[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error: {MarkupEscaped(ex.Message)}[/]");
    }
}

static void ShowFilePreview(string file)
{
    try
    {
        var lines = File.ReadLines(file).Take(200).ToList();
        AnsiConsole.MarkupLine($"[green]Preview of[/] [grey]{MarkupEscaped(file)}[/] ([yellow]{lines.Count} lines[/]):");
        var panel = new Panel(string.Join('\n', lines)) { Border = BoxBorder.Rounded };
        AnsiConsole.Write(panel);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error reading file: {MarkupEscaped(ex.Message)}[/]");
    }
}

static string MarkupEscaped(string text)
{
    if (text == null) return string.Empty;
    return text.Replace("[", "[[").Replace("]", "]]" );
}

record FileSystemEntry(string Path, bool IsDirectory)
{
    public string Name => System.IO.Path.GetFileName(Path) ?? Path;
    public long Size => IsDirectory ? 0 : new FileInfo(Path).Length;
    public DateTime Modified => IsDirectory ? Directory.GetLastWriteTimeUtc(Path) : File.GetLastWriteTimeUtc(Path);
}

public class ConsoleSource
{
    public string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();
}

public enum EntryType
{
    All,
    Directory,
    File
}