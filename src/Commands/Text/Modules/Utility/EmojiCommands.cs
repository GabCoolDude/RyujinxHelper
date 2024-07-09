﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Volte.Core.Helpers;

namespace Volte.Commands.Text.Modules
{
    public sealed partial class UtilityModule
    {
        [Command("BigEmoji", "HugeEmoji", "BigEmote", "HugeEmote")]
        [Description("Shows the image URL for a given emoji or emote.")]
        public Task<ActionResult> BigEmojiAsync(
            [Description("The emote you want to see large. Has to be a custom emote.")]
            Emote emote)
            => Ok(GenerateEmbed(emote));

        [Command("Emotes")]
        [Description("Shows pages for every emote in this guild.")]
        public Task<ActionResult> EmotesAsync()
        {
            var embeds = Context.Guild.Emotes.Select(GenerateEmbed).ToList();
            if (embeds.Any()) 
                return embeds.Count is 1 ? Ok(embeds.First()) : Ok(embeds);    
            
            return BadRequest("This guild doesn't have any emotes.");
        }

        private EmbedBuilder GenerateEmbed(Emote emote)
            => Context.CreateEmbedBuilder(Format.Url("Direct Link", emote.Url))
                    .AddField("Created", emote.CreatedAt.ToDiscordTimestamp(TimestampType.Relative), true)
                    .AddField("Animated?", emote.Animated ? "Yes" : "No")
                    .WithImageUrl(emote.Url)
                    .WithAuthor($":{emote.Name}:", emote.Url);
    }
}