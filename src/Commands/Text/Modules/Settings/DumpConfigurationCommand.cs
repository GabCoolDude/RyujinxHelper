/*namespace Volte.Commands.Modules;

public partial class SettingsModule
{
    [Command("Dump", "Dc")]
    [Description("Dumps this guild's configuration to paste.greemdev.net in JSON format for getting support.")]
    public async Task<ActionResult> DumpConfigurationAsync() 
        => Ok($"{await HttpHelper.PostToGreemPasteAsync(Context.GuildData.ToString(), Context.Services, "json")}");
}*/