﻿namespace Volte.Core.Helpers;

public static class EmbedHelpers
{
    public static EmbedBuilder WithSuccessColor(this EmbedBuilder e) => e.WithColor(Config.SuccessColor);

    public static EmbedBuilder WithErrorColor(this EmbedBuilder e) => e.WithColor(Config.ErrorColor);

    public static EmbedBuilder WithRelevantColor(this EmbedBuilder e, SocketGuildUser user) =>
        e.WithColor(user.GetHighestRole()?.Color ?? new Color(Config.SuccessColor));
    
    public static EmbedBuilder WithDescription(this EmbedBuilder e, Action<StringBuilder> description) => 
        e.WithDescription(String(description));

    public static EmbedBuilder AppendDescription(this EmbedBuilder e, string toAppend) =>
        e.WithDescription((e.Description ?? string.Empty) + toAppend);
    
    public static EmbedBuilder AppendDescriptionLine(this EmbedBuilder e, string toAppend = "") =>
        e.AppendDescription($"{toAppend}\n");

    public static EmbedBuilder AddField(this EmbedBuilder e, object name, Action<StringBuilder> description,
        bool inline = false) => 
        e.AddField(name.ToString(), String(description), inline);

    /// <summary>
    ///     Removes the author and sets the color to the config-provided <see cref="Config"/>.<see cref="Config.SuccessColor"/>,
    /// however it only removes it if <see cref="ModerationOptions.ShowResponsibleModerator"/> on the provided <paramref name="data"/> is <see langword="false"/>
    /// </summary>
    /// <param name="e">The current <see cref="EmbedBuilder"/>.</param>
    /// <param name="data">The <see cref="GuildData"/> to apply settings for.</param>
    /// <returns>The possibly-modified <see cref="EmbedBuilder"/></returns>
    public static EmbedBuilder ApplyConfig(this EmbedBuilder e, GuildData data) => e.Apply(eb =>
    {
        if (data.Configuration.Moderation.ShowResponsibleModerator) return;
        
        eb.WithAuthor(author: null);
        eb.WithSuccessColor();
    });
    
    public static Embed Embed(Action<EmbedBuilder> initializer) => new EmbedBuilder().Apply(initializer).Build();
}