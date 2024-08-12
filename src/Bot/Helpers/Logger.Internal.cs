﻿using Colorful;
using Sentry.Extensibility;

using Color = System.Drawing.Color;
using Optional = Gommon.Optional;

namespace Volte.Helpers;

public static partial class Logger
{
    public class SentryTranslator : IDiagnosticLogger
    {
        public bool IsEnabled(SentryLevel logLevel) 
            => Version.IsDevelopment || logLevel is not SentryLevel.Debug;
            
        public void Log(SentryLevel logLevel, string message, Exception exception = null, params object[] args)
            => LogSync.Lock(() =>
            {
                var (color, value) = VerifySentryLevel(logLevel);
                Append($"{value}:".P(), color);
                Append("[SENTRY]".P(), Color.Chartreuse);

                if (!message.IsNullOrWhitespace())
                    Append(message.Format(args), Color.White);

                if (exception != null)
                {
                    var toWrite = $"{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}";
                    Append(toWrite, Color.IndianRed);
                }
                    
                Console.Write(Environment.NewLine);
            });
    }

    private static readonly string[] VolteAscii =
        new Figlet().ToAscii("Volte").ConcreteValue.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
    static Logger() => FilePath.Logs.Create();
        
    private static readonly object LogSync = new();
    
    internal static void PrintHeader()
    {
        if (VolteBot.AvaloniaIsAttached) return;
        
        Info(LogSource.Volte, MessageService.Separator.Trim());
        VolteAscii.ForEach(ln => Info(LogSource.Volte, ln));
        Info(LogSource.Volte, MessageService.Separator.Trim());
    }

    private const string Side = "----------------------------------------------------------";
    private static bool _logFileNoticePrinted;

    internal static void LogFileRestartNotice()
    {
        if (_logFileNoticePrinted || !(Config.EnabledFeatures?.LogToFile ?? false)) return;
            
        GetLogFilePath(DateTime.Now).AppendAllText($"{Side}RESTARTING{Side}\n");
            
        _logFileNoticePrinted = true;
    }
    
    private static void Log(LogSeverity s, LogSource from, string message, Exception e, InvocationInfo caller = default)
    {
        if (s is LogSeverity.Debug && !Config.DebugEnabled)
            return;
        
        LogEventHandler.Call(new VolteLogEventArgs
        {
            Severity = s,
            Source = from,
            Message = message,
            Error = e,
            Invocation = caller
        });
    }
    
    private static void Execute(LogSeverity s, LogSource src, string message, Exception e, InvocationInfo caller)
    {
        var content = new StringBuilder();

        if (IsDebugLoggingEnabled && caller.IsInitialized)
        {
            var debugInfo = Optional.None<string>();
            switch (caller.Type)
            {
                case { Full: true }:
                    debugInfo =
                        $"{caller.GetSourceFileName()}:{caller.LineInFile}->{caller.CallerName}";
                    break;
                case { CallerOnly: true }:
                    debugInfo = caller.CallerName;
                    break;
                
                case { FileLoc: true }:
                    debugInfo = $"{caller.GetSourceFileName()}:{caller.LineInFile}";
                    break;
            }

            debugInfo.IfPresent(debugInfoContent =>
            {
                Append(debugInfoContent, Color.Aquamarine, ref content);
                Append(" |>  ", Color.Goldenrod, ref content);
            });
        }
        
        var (color, value) = VerifySeverity(s);
        Append($"{value}:".P(), color);
        var dt = DateTime.Now.ToLocalTime();
        content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {value} -> ");

        (color, value) = VerifySource(src);
        Append($"[{value}]".P(), color);
        content.Append(string.Intern($"{value} -> "));

        if (!message.IsNullOrWhitespace())
            Append(message, Color.White, ref content);

        if (e != null)
        {
            e.SentryCapture(scope => 
                scope.AddBreadcrumb("This exception might not have been thrown, and may not be important; it is merely being logged.")
            );
            
            Append(errorString(), Color.IndianRed, ref content);

            string errorString()
                => Environment.NewLine + (e.Message.IsNullOrEmpty() ? "No message provided" : e.Message) +
                   Environment.NewLine + e.StackTrace;
        }

        if (Environment.NewLine != content[^1].ToString())
        {
            Console.Write(Environment.NewLine);
            content.AppendLine();
        }
            
        if (Config.EnabledFeatures?.LogToFile ?? false)
            GetLogFilePath(DateTime.Now).AppendAllText(content.ToString());
    }

    public static FilePath GetLogFilePath(DateTime date) 
        => new FilePath("logs") / string.Intern($"{date.Year}-{date.Month}-{date.Day}.log");

    private static void Append(string m, Color c)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
    }

    private static void Append(string m, Color c, ref StringBuilder sb)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
        sb?.Append(m);
    }

    private static (Color Color, string Source) VerifySource(LogSource source) =>
        source switch
        {
            LogSource.Discord => (Color.RoyalBlue, "DISCORD"),
            LogSource.Gateway => (Color.RoyalBlue, "DISCORD"),
            LogSource.Volte => (Color.LawnGreen, "CORE"),
            LogSource.Service => (Color.Gold, "SERVICE"),
            LogSource.Module => (Color.LimeGreen, "MODULE"),
            LogSource.Rest => (Color.Red, "REST"),
            LogSource.Unknown => (Color.Fuchsia, "UNKNOWN"),
            LogSource.Sentry => (Color.Chartreuse, "SENTRY"),
            LogSource.UI => (Color.Teal, "UI"),
            _ => throw new InvalidOperationException($"The specified LogSource {source} is invalid.")
        };
        
    private static (Color Color, string Level) VerifySentryLevel(SentryLevel level) =>
        level switch
        {
            SentryLevel.Debug => (Color.RoyalBlue, "DEBUG"),
            SentryLevel.Info => (Color.RoyalBlue, "INFO"),
            SentryLevel.Warning => (Color.LawnGreen, "WARN"),
            SentryLevel.Error => (Color.Gold, "ERROR"),
            SentryLevel.Fatal => (Color.LimeGreen, "FATAL"),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };


    private static (Color Color, string Level) VerifySeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => (Color.Maroon, "CRITICAL"),
            LogSeverity.Error => (Color.DarkRed, "ERROR"),
            LogSeverity.Warning => (Color.Yellow, "WARN"),
            LogSeverity.Info => (Color.SpringGreen, "INFO"),
            LogSeverity.Verbose => (Color.Pink, "VERBOSE"),
            LogSeverity.Debug => (Color.SandyBrown, "DEBUG"),
            _ => throw new InvalidOperationException($"The specified LogSeverity ({severity}) is invalid.")
        };

    public static string P(this string input, int padding = 10) => string.Intern(input.PadRight(padding));
}