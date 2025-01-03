namespace RyuBot.Commands.Text;

public sealed class RequireGuildAdminAttribute : CheckAttribute
{
    public override ValueTask<CheckResult> CheckAsync(CommandContext context)
    {
        var ctx = context.Cast<RyujinxBotContext>();
        if (ctx.IsAdmin(ctx.User)) return CheckResult.Successful;
            
        return CheckResult.Failed("Insufficient permission.");
    }
}