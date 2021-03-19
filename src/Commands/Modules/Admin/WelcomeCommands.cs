﻿using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Core.Entities;
using Volte.Commands.Results;
using Volte.Services;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminModule
    {

        [Command("WelcomeChannel", "Wc")]
        [Description("Sets the channel used for welcoming new users for this guild.")]
        [Remarks("welcomechannel {Channel}")]
        [RequireGuildAdmin]
        public Task<ActionResult> WelcomeChannelAsync([Remainder] SocketTextChannel channel)
        {
            Context.GuildData.Configuration.Welcome.WelcomeChannel = channel.Id;
            Db.Save(Context.GuildData);
            return Ok($"Set this guild's welcome channel to {channel.Mention}.");
        }

        [Command("WelcomeMessage", "Wmsg")]
        [Description(
            "Sets or shows the welcome message used to welcome new users for this guild. Only in effect when the bot isn't using the welcome image generating API.")]
        [Remarks("welcomemessage [String]")]
        [RequireGuildAdmin]
        public Task<ActionResult> WelcomeMessageAsync([Remainder] string message = null)
        {
            if (message is null)
            {
                return Ok(new StringBuilder()
                    .AppendLine("The current welcome message for this guild is: ```")
                    .AppendLine(Context.GuildData.Configuration.Welcome.WelcomeMessage)
                    .Append("```")
                    .ToString());
            }

            Context.GuildData.Configuration.Welcome.WelcomeMessage = message;
            Db.Save(Context.GuildData);
            var welcomeChannel = Context.Guild.GetTextChannel(Context.GuildData.Configuration.Welcome.WelcomeChannel);
            var sendingTest = Context.GuildData.Configuration.Welcome.WelcomeChannel is 0 || welcomeChannel is null
                ? "Not sending a test message as you do not have a welcome channel set." +
                  "Set a welcome channel to fully complete the setup!"
                : $"Sending a test message to {welcomeChannel.Mention}.";

            return Ok(new StringBuilder()
                .AppendLine($"Set this guild's welcome message to {Format.Code(message)}")
                .AppendLine()
                .AppendLine($"{sendingTest}").ToString(),
                async _ => {
                    if (welcomeChannel != null)
                        await WelcomeService.JoinAsync(new UserJoinedEventArgs(Context.User));
                });
        }

        [Command("WelcomeColor", "WelcomeColour", "Wcl")]
        [Description("Sets the color used for welcome embeds for this guild.")]
        [Remarks("welcomecolor {Color}")]
        [RequireGuildAdmin]
        public Task<ActionResult> WelcomeColorAsync([Remainder] Color color)
        {
            Context.GuildData.Configuration.Welcome.WelcomeColor = color.RawValue;
            Db.Save(Context.GuildData);
            return Ok("Successfully set this guild's welcome message embed color!");
        }

        [Command("LeavingMessage", "Lmsg")]
        [Description("Sets or shows the leaving message used to say bye for this guild.")]
        [Remarks("leavingmessage [String]")]
        [RequireGuildAdmin]
        public Task<ActionResult> LeavingMessageAsync([Remainder] string message = null)
        {
            if (message is null)
            {
                return Ok(new StringBuilder()
                    .AppendLine("The current leaving message for this guild is ```")
                    .AppendLine(Context.GuildData.Configuration.Welcome.LeavingMessage)
                    .Append("```")
                    .ToString());
            }

            Context.GuildData.Configuration.Welcome.LeavingMessage = message;
            Db.Save(Context.GuildData);
            var welcomeChannel = Context.Guild.GetTextChannel(Context.GuildData.Configuration.Welcome.WelcomeChannel);
            var sendingTest = Context.GuildData.Configuration.Welcome.WelcomeChannel == 0 || welcomeChannel is null
                ? "Not sending a test message, as you do not have a welcome channel set. " +
                  "Set a welcome channel to fully complete the setup!"
                : $"Sending a test message to {welcomeChannel.Mention}.";

            return Ok(new StringBuilder()
                    .AppendLine($"Set this server's leaving message to {Format.Code(message)}")
                    .AppendLine()
                    .AppendLine($"{sendingTest}").ToString(),
                async _ =>
                {
                    if (welcomeChannel != null)
                        await WelcomeService.LeaveAsync(new UserLeftEventArgs(Context.User));
                });
        }

        [Command("WelcomeDmMessage", "Wdmm")]
        [Description("Sets the message to be (attempted to) sent to members upon joining.")]
        [Remarks("welcomedmmessage")]
        [RequireGuildAdmin]
        public Task<ActionResult> WelcomeDmMessageAsync(string message = null)
        {
            if (message is null)
                return Ok($"Unset the WelcomeDmMessage that was previously set to: {Format.Code(Context.GuildData.Configuration.Welcome.WelcomeDmMessage)}");

            Context.GuildData.Configuration.Welcome.WelcomeDmMessage = message;
            Db.Save(Context.GuildData);
            return Ok($"Set the WelcomeDmMessage to: {Format.Code(message)}");
        }
    }
}