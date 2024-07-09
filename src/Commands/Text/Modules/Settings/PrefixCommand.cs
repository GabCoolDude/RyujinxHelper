﻿using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace Volte.Commands.Text.Modules
{
    public sealed partial class SettingsModule
    {
        [Command("Prefix")]
        [Description("Sets the command prefix for this guild.")]
        public Task<ActionResult> PrefixAsync([Remainder] string newPrefix)
        {
            Context.Modify(data => data.Configuration.CommandPrefix = newPrefix);
            return Ok($"Set this guild's prefix to {Format.Bold(newPrefix)}.");
        }
    }
}