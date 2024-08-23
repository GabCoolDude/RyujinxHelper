﻿using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Discord;
using Gommon;
using Volte.Entities;
using Volte.Helpers;
using Volte.UI.Helpers;

namespace Volte.UI.Avalonia.Pages;

public partial class LogsViewModel : ObservableObject
{
    private const byte MaxLogsInMemory = 200;

    private readonly object _logSync = new();
    
    public required LogsView? View { get; init; }

    [ObservableProperty] 
    private ObservableCollection<VolteLog> _logs = [];

    [ObservableProperty] 
    private VolteLog? _selected;
    
    public LogsViewModel() => Logger.Event += Receive;

    ~LogsViewModel() => Logger.Event -= Receive;

    public static void UnregisterHandler()
    {
        Logger.Event -= PageManager.Shared.GetViewModel<LogsViewModel>().Receive;
    }

    private void Receive(VolteLogEventArgs eventArgs)
    {
        if (eventArgs is { Source: LogSource.Sentry, Severity: LogSeverity.Debug }) 
            return; //sentry debug messages are huge and break the log view entirely.

        lock (_logSync)
        {
            if (!(eventArgs.Message.IsNullOrEmpty() || eventArgs.Message.IsNullOrWhitespace()))
            {
                if (Logs.Count >= MaxLogsInMemory)
                    Logs.WithIndex()
                        .OrderByDescending(x => x.Value.Date)
                        .TakeLast(10)
                        .ForEach(toRemove => Logs.RemoveAt(toRemove.Index));
                
                Logs.Add(new VolteLog(eventArgs));
                Lambda.Try(() => Dispatcher.UIThread.Invoke(() => View?.Viewer?.ScrollToEnd()));
            }

            if (eventArgs.Error is not { } err) return;
        
            VolteApp.NotifyError(err);
            err.SentryCapture(scope => 
                scope.AddBreadcrumb("This exception might not have been thrown, and may not be important; it is merely being logged.")
            );
        }
    }
}