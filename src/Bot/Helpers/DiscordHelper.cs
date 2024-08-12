using Volte.Commands.Text.Modules;
using Volte.Interactions;

namespace Volte.Helpers;

public static class DiscordHelper
{
    public static ulong DevGuildId = 405806471578648588;
    
    public static string Zws => "\u200B";

    public static List<Emoji> GetPollEmojis()
        => [
            Emojis.One, Emojis.Two, Emojis.Three, Emojis.Four, Emojis.Five,
            Emojis.Six, Emojis.Seven, Emojis.Eight, Emojis.Nine
        ];

    public static RequestOptions CreateRequestOptions(Action<RequestOptions> initializer) 
        => new RequestOptions().Apply(initializer);


    /// <summary>
    ///     Checks if the current user is the user identified in the bot's config.
    /// </summary>
    /// <param name="user">The current user</param>
    /// <returns>True, if the current user is the bot's owner; false otherwise.</returns>
    public static bool IsBotOwner(this IUser user)
        => Config.Owner == user.Id;

    private static bool IsGuildOwner(this IGuildUser user)
        => user.Guild.OwnerId == user.Id || IsBotOwner(user);
    
    public static bool IsAdmin(this VolteContext ctx, SocketGuildUser user)
        => HasRole(user, ctx.GuildData.Configuration.Moderation.AdminRole) 
           || IsGuildOwner(user);

    public static bool IsModerator(this VolteContext ctx, SocketGuildUser user)
        => user.HasRole(ctx.GuildData.Configuration.Moderation.ModRole) 
           || ctx.IsAdmin(user) 
           || IsGuildOwner(user);

    public static bool HasRole(this SocketGuildUser user, ulong roleId)
        => user.Roles.Select(x => x.Id).Contains(roleId);
    
    public static Task WarnAsync(this SocketGuildUser member, VolteContext ctx, string reason)
        => ModerationModule.WarnAsync(ctx.User, ctx.GuildData, member,
            ctx.Services.GetRequiredService<DatabaseService>(), reason);

    public static async Task<bool> TrySendMessageAsync(this SocketGuildUser user, string text = null,
        bool isTts = false, Embed embed = null, RequestOptions options = null)
    {
        try
        {
            await user.SendMessageAsync(text, isTts, embed, options);
            return true;
        }
        catch (HttpException)
        {
            return false;
        }
    }

    public static SocketRole GetHighestRole(this SocketUser user, bool requireColor = true)
    {
        if (user is not SocketGuildUser sgu) return null;
        
        return sgu.Roles
            .Where(x => !requireColor || x.HasColor())
            .MaxBy(x => x.Position);
    }
    
    public static bool TryGetSpotifyStatus(this IUser user, out SpotifyGame spotify)
    {
        spotify = user.Activities.FirstOrDefault(x => x is SpotifyGame).Cast<SpotifyGame>();
        return spotify != null;
    }
        
    public static string ToMarkdownTimestamp(long unixSeconds, char timestampType)
        => $"<t:{unixSeconds}:{timestampType}>";
        
    public static string ToDiscordTimestamp(this DateTimeOffset dto, TimestampType type) =>
        ToMarkdownTimestamp(dto.ToUnixTimeSeconds(), (char)type);

    public static string ToDiscordTimestamp(this DateTime date, TimestampType type) => 
        new DateTimeOffset(date).ToDiscordTimestamp(type);

    public static async Task<bool> TrySendMessageAsync(this SocketTextChannel channel, string text = null,
        bool isTts = false, Embed embed = null, RequestOptions options = null)
    {
        try
        {
            await channel.SendMessageAsync(text, isTts, embed, options);
            return true;
        }
        catch (HttpException)
        {
            return false;
        }
    }

    public static string GetInviteUrl(this IDiscordClient client, bool withAdmin = true)
        => client.GetInviteUrl(withAdmin ? 8 : 402992246);
        
    public static string GetInviteUrl(this IDiscordClient client, long permissions)
        => $"https://discord.com/oauth2/authorize?client_id={client.CurrentUser.Id}&permissions={permissions}&scope=bot%20applications.commands";

    public static SocketUser GetOwner(this BaseSocketClient client)
        => client.GetUser(Config.Owner);

    public static SocketGuild GetPrimaryGuild(this BaseSocketClient client)
        => client.GetGuild(405806471578648588); // TODO: config option
    
    public static async Task RegisterVolteEventHandlers(this DiscordSocketClient client, ServiceProvider provider)
    {
        Listen(client);
        
        CalledCommandsInfo.StartPersistence(provider, saveEvery: 2.Minutes());
        
        client.MessageReceived += async socketMessage =>
        {
            Info(LogSource.Volte, socketMessage.Content);
            
            if (socketMessage.ShouldHandle(out var msg))
            {
                if (msg.Channel is IDMChannel dm)
                    await dm.SendMessageAsync("Currently, I do not support commands via DM.");
                else
                    await provider.Get<MessageService>().HandleMessageAsync(new MessageReceivedEventArgs(socketMessage, provider));
            }
        };

        client.Ready += async () =>
        {
            var guilds = client.Guilds.Count;
            var users = client.Guilds.SelectMany(x => x.Users).DistinctBy(x => x.Id).Count();
            var channels = client.Guilds.SelectMany(x => x.Channels).DistinctBy(x => x.Id).Count();

            PrintHeader();
            Info(LogSource.Volte, $"Currently running Volte V{Version.InformationVersion}.");
            Info(LogSource.Volte, "Use this URL to invite me to your guilds:");
            Info(LogSource.Volte, $"{client.GetInviteUrl()}");
            Info(LogSource.Volte, $"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
            Info(LogSource.Volte, $"Default command prefix is: \"{Config.CommandPrefix}\"");
            Info(LogSource.Volte, "Connected to:");
            Info(LogSource.Volte, $"     {"guild".ToQuantity(guilds)}");
            Info(LogSource.Volte, $"     {"user".ToQuantity(users)}");
            Info(LogSource.Volte, $"     {"channel".ToQuantity(channels)}");

            var (type, name, streamer) = Config.ParseActivity();

            if (streamer is null && type != ActivityType.CustomStatus)
            {
                await client.SetGameAsync(name, null, type);
                Info(LogSource.Volte, $"Set {client.CurrentUser.Username}'s game to \"{Config.Game}\".");
            }
            else if (type != ActivityType.CustomStatus)
            {
                await client.SetGameAsync(name, Config.FormattedStreamUrl, type);
                Info(LogSource.Volte,
                    $"Set {client.CurrentUser.Username}'s activity to \"{type}: {name}\", at Twitch user {Config.Streamer}.");
            }

            ExecuteBackgroundAsync(async () =>
            {
                foreach (var g in client.Guilds)
                {
                    if (Config.BlacklistedOwners.Contains(g.OwnerId))
                        await g.LeaveAsync().Then(async () => Warn(LogSource.Volte,
                            $"Left guild \"{g.Name}\" owned by blacklisted owner {await client.Rest.GetUserAsync(g.OwnerId)}."));
                    else provider.Get<DatabaseService>().GetData(g); //ensuring all guilds have data available to prevent exceptions later on 
                }
            });
            
            await provider.Get<VolteInteractionService>().InitAsync();
        };
    }

    public static Task<IUserMessage> SendToAsync(this EmbedBuilder e, IMessageChannel c) =>
        c.SendMessageAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

    public static Task<IUserMessage> SendToAsync(this Embed e, IMessageChannel c) =>
        c.SendMessageAsync(embed: e, allowedMentions: AllowedMentions.None);

    public static Task<IUserMessage> ReplyToAsync(this EmbedBuilder e, IUserMessage msg) =>
        msg.ReplyAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

    public static Task<IUserMessage> ReplyToAsync(this Embed e, IUserMessage msg) =>
        msg.ReplyAsync(embed: e, allowedMentions: AllowedMentions.None);


    // ReSharper disable twice UnusedMethodReturnValue.Global
    public static async Task<IUserMessage> SendToAsync(this EmbedBuilder e, IGuildUser u) =>
        await (await u.CreateDMChannelAsync()).SendMessageAsync(embed: e.Build());

    public static async Task<IUserMessage> SendToAsync(this Embed e, IGuildUser u) =>
        await (await u.CreateDMChannelAsync()).SendMessageAsync(embed: e);

    public static Emoji ToEmoji(this string str) => new(str);

    public static bool ShouldHandle(this SocketMessage message, out SocketUserMessage userMessage)
    {
        if (message is SocketUserMessage msg && !msg.Author.IsBot)
        {
            userMessage = msg;
            return true;
        }

        userMessage = null;
        return false;
    }

    public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RequestOptions options = null)
    {
        try
        {
            if (deletable is null) return false;
            await deletable.DeleteAsync(options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Task<bool> TryDeleteAsync(this IDeletable deletable, string reason)
        => deletable.TryDeleteAsync(CreateRequestOptions(opts => opts.AuditLogReason = reason));

    public static string GetEffectiveUsername(this IGuildUser user) =>
        user.Nickname ?? user.Username;

    public static string GetEffectiveAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto,
        ushort size = 128)
        => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

    public static bool HasAttachments(this IMessage message)
        => message.Attachments.Count != 0;

    public static bool HasColor(this IRole role)
        => role.Color.RawValue != 0;

    public static EmbedBuilder WithDescription(this EmbedBuilder e, StringBuilder sb)
        => e.WithDescription(sb.ToString());
}

public enum TimestampType : sbyte
{
    ShortTime = (sbyte)'t',
    LongTime = (sbyte)'T',
    ShortDate = (sbyte)'d',
    LongDate = (sbyte)'D',
    ShortDateTime = (sbyte)'f',
    LongDateTime = (sbyte)'F',
    Relative = (sbyte)'R'
}