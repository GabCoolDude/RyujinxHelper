﻿using RyuBot.Interactions;

namespace RyuBot.Commands.Modules;

public partial class CompatibilityModule : RyujinxBotSlashCommandModule
{
    public CompatibilityCsvService Compatibility { get; set; }
}