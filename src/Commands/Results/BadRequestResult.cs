namespace Volte.Commands;

public class BadRequestResult : ActionResult
{
    public BadRequestResult(string reason) 
        => Reason = reason;

    public string Reason { get; }

    public override bool IsSuccessful => false;

    public override async ValueTask<Gommon.Optional<ResultCompletionData>> ExecuteResultAsync(VolteContext ctx)
    {
        var e = ctx.CreateEmbedBuilder()
            .WithTitle("No can do, partner.")
            .WithDescription(Reason)
            .WithCurrentTimestamp();

        return new ResultCompletionData(ctx.GuildData.Configuration.ReplyInline
            ? await e.ReplyToAsync(ctx.Message)
            : await e.SendToAsync(ctx.Channel));
    }
}