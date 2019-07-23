using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Core.Data.Models;
using Volte.Core.Data.Models.EventArgs;
using Volte.Core.Data.Models.Results;

namespace Volte.Commands.Modules
{
    public partial class ModerationModule : VolteModule
    {
        [Command("Purge", "clear", "clean")]
        [Description("Purges the last x messages, or the last x messages by a given user.")]
        [Remarks("Usage: |prefix|purge {count} [targetAuthor]")]
        [RequireBotChannelPermission(ChannelPermission.ManageMessages)]
        [RequireGuildModerator]
        public async Task<VolteCommandResult> PurgeAsync(int count, SocketGuildUser targetAuthor = null)
        {
            //+1 to include the command invocation message, and actually delete the last x messages instead of x - 1.
            //lets you theoretically use 0 to delete only the invocation message, for testing or something.
            var messages = (await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync()).ToList();
            if (!(targetAuthor is null))
                await Context.Channel.DeleteMessagesAsync(messages.Where(x => x.Author.Id == targetAuthor.Id));
            else
                await Context.Channel.DeleteMessagesAsync(messages);

            //-1 to show that the correct amount of messages were deleted.
            var mCount = messages.Count() - 1;

            return Ok($"Successfully deleted **{mCount}** {"message".ToQuantity(mCount)}", m =>
            {
                _ = Executor.ExecuteAfterDelayAsync(3000, async () => await m.DeleteAsync());
                return ModLogService.OnModActionCompleteAsync(new ModActionEventArgs(Context, ModActionType.Purge,
                    count));
            });
        }
    }
}