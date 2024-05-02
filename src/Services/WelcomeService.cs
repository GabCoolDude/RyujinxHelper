﻿using Volte.Core;

namespace Volte.Services;

public sealed class WelcomeService(DatabaseService _db) : IVolteService
{
    public async Task JoinAsync(UserJoinedEventArgs args)
    {
        var data = _db.GetData(args.Guild);

        if (data.Configuration.Welcome.WelcomeMessage.IsNullOrEmpty())
            return; //we don't want to send an empty join message
        if (!data.Configuration.Welcome.WelcomeDmMessage.IsNullOrEmpty())
            _ = await args.User.TrySendMessageAsync(data.Configuration.Welcome.FormatDmMessage(args.User));

        Logger.Debug(LogSource.Volte,
            "User joined a guild, let's check to see if we should send a welcome embed.");
        var welcomeMessage = data.Configuration.Welcome.FormatWelcomeMessage(args.User);
        var c = args.Guild.GetTextChannel(data.Configuration.Welcome.WelcomeChannel);

        if (c != null)
        {
            await new EmbedBuilder()
                .WithColor(data.Configuration.Welcome.WelcomeColor)
                .WithDescription(welcomeMessage)
                .WithThumbnailUrl(args.User.GetEffectiveAvatarUrl())
                .WithCurrentTimestamp()
                .SendToAsync(c);

            Logger.Debug(LogSource.Volte, $"Sent a welcome embed to #{c.Name}.");
            return;
        }

        Logger.Debug(LogSource.Volte,
            "WelcomeChannel config value resulted in an invalid/nonexistent channel; aborting.");
    }

    public async Task LeaveAsync(UserLeftEventArgs args)
    {
        var data = _db.GetData(args.Guild);
        if (data.Configuration.Welcome.LeavingMessage.IsNullOrEmpty()) return;
        Logger.Debug(LogSource.Volte,
            "User left a guild, let's check to see if we should send a leaving embed.");
        var c = args.Guild.GetTextChannel(data.Configuration.Welcome.WelcomeChannel);
        if (c is null)
            Logger.Debug(LogSource.Volte,
                "WelcomeChannel config value resulted in an invalid/nonexistent channel; aborting.");
        else
        {
            await new EmbedBuilder()
                .WithColor(data.Configuration.Welcome.WelcomeColor)
                .WithDescription(data.Configuration.Welcome.FormatLeavingMessage(args.Guild, args.User))
                .WithThumbnailUrl(args.User.GetEffectiveAvatarUrl())
                .WithCurrentTimestamp()
                .SendToAsync(c);
            Logger.Debug(LogSource.Volte, $"Sent a leaving embed to #{c.Name}.");
        }
    }
}