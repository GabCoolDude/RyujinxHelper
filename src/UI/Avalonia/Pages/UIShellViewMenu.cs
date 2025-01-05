﻿using Avalonia.Controls.Notifications;
using Gommon;
using MenuFactory.Abstractions.Attributes;
using RyuBot.Helpers;
using RyuBot.Interactions;
// ReSharper disable UnusedMember.Global
// These members are never directly invoked.

namespace RyuBot.UI.Avalonia.Pages;

// ReSharper disable once InconsistentNaming
public class ShellViewMenu
{
    [Menu("Clear Commands", "Dev", Icon = "fa-solid fa-broom")]
    public static async Task ClearCommands()
    {
        var interactionService = RyujinxBot.Services.Get<RyujinxBotInteractionService>();
        if (interactionService is null || RyujinxBot.Client is null)
        {
            RyujinxBotApp.Notify("Not logged in", "State error", NotificationType.Error);
            return;
        }
        
#if DEBUG
        var removedCommands = await interactionService.ClearAllCommandsInGuildAsync(DiscordHelper.DevGuildId);
#else
        var removedCommands = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var guildId in Config.WhitelistGuilds.Keys)
        {
            removedCommands = Math.Max(await interactionService.ClearAllCommandsInGuildAsync(guildId), removedCommands);
        }
#endif

        RyujinxBotApp.Notify($"{removedCommands} removed", "Interaction commands cleared");
    }
}