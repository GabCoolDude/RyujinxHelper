﻿namespace Volte.Services;

public sealed class WelcomeService : VolteService
{
    private readonly DatabaseService _db;

    public WelcomeService(DiscordSocketClient client, DatabaseService databaseService)
    {
        _db = databaseService;
        client.UserJoined += user => JoinAsync(new UserJoinedEventArgs(user));
        client.UserLeft += (guild, user) => LeaveAsync(new UserLeftEventArgs(guild, user));
    }
    
    public async Task JoinAsync(UserJoinedEventArgs args)
    {
        if (!Config.EnabledFeatures.Welcome) return;
        
        var data = _db.GetData(args.Guild);
        
        if (!data.Configuration.Welcome.WelcomeDmMessage.IsNullOrEmpty())
            await args.User.TrySendMessageAsync(data.Configuration.Welcome.FormatDmMessage(args.User));

        if (data.Configuration.Welcome.WelcomeMessage.IsNullOrEmpty())
            return; //we don't want to send an empty join message


        Debug(LogSource.Volte,
            "User joined a guild, let's check to see if we should send a welcome embed.");
        var welcomeMessage = data.Configuration.Welcome.FormatWelcomeMessage(args.User);
        var c = args.Guild.GetTextChannel(data.Configuration.Welcome.WelcomeChannel);

        if (c is not null)
        {
            await new EmbedBuilder()
                .WithColor(data.Configuration.Welcome.WelcomeColor)
                .WithDescription(welcomeMessage)
                .WithThumbnailUrl(args.User.GetEffectiveAvatarUrl())
                .WithCurrentTimestamp()
                .SendToAsync(c);

            Debug(LogSource.Volte, $"Sent a welcome embed to #{c.Name}.");
        } else
            Debug(LogSource.Volte,
            "WelcomeChannel config value resulted in an invalid/nonexistent channel; aborting.");
    }

    public async Task LeaveAsync(UserLeftEventArgs args)
    {
        if (!Config.EnabledFeatures.Welcome) return;
        
        var data = _db.GetData(args.Guild);
        if (data.Configuration.Welcome.LeavingMessage.IsNullOrEmpty()) return;
        Debug(LogSource.Volte,
            "User left a guild, let's check to see if we should send a leaving embed.");
        var c = args.Guild.GetTextChannel(data.Configuration.Welcome.WelcomeChannel);
        if (c is not null)
        {
            await new EmbedBuilder()
                .WithColor(data.Configuration.Welcome.WelcomeColor)
                .WithDescription(data.Configuration.Welcome.FormatLeavingMessage(args.Guild, args.User))
                .WithThumbnailUrl(args.User.GetEffectiveAvatarUrl())
                .WithCurrentTimestamp()
                .SendToAsync(c);
            Debug(LogSource.Volte, $"Sent a leaving embed to #{c.Name}.");
        } else
            Debug(LogSource.Volte,
                "WelcomeChannel config value resulted in an invalid/nonexistent channel; aborting.");
    }
}