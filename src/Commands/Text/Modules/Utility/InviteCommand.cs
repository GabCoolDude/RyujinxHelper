﻿namespace Volte.Commands.Text.Modules;

public sealed partial class UtilityModule
{
    [Command("Invite")]
    [Description("Get an invite to use Volte in your own guild.")]
    public Task<ActionResult> InviteAsync()
        => Ok(new StringBuilder()
            .AppendLine(
                $"Do you like Volte? If you do, that's awesome! If not then I'm sorry (please tell me what you don't like {Format.Url("here", "https://forms.gle/CJ9XtKmKf2Q2mQwb7")}!) :( ")
            .AppendLine()
            .AppendLine(Format.Url("GitHub", "https://github.com/Ultz/Volte"))
            .AppendLine(Format.Url("Invite Me", Context.Client.GetInviteUrl()))
            .AppendLine(Format.Url("Support Server", "https://discord.gg/H8bcFr2"))
            .AppendLine()
            .AppendLine("And again, thanks for using me!")
            .ToString());
}