namespace RyuBot.Commands.Text.Modules;

public partial class SettingsModule
{
    [Command("AccountAgeChecking", "Aac")]
    [Description(
        "Enables or disables the automatic account age warnings when a user who joins has a very young account (created within the last month).")]
    public Task<ActionResult> AccountAgeCheckingAsync(bool enabled)
    {
        Context.Modify(data => data.Configuration.Moderation.CheckAccountAge = enabled);
        return Ok(enabled ? "Account age detection has been enabled." : "Account age detection has been disabled.");
    }
}