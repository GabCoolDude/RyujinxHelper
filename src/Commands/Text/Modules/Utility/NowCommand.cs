using System.Text;
using System.Threading.Tasks;
using Discord;
using Gommon;
using Qmmands;
using Volte.Core.Helpers;

namespace Volte.Commands.Text.Modules
{
    public sealed partial class UtilityModule
    {
        [Command("Now")]
        [Description("Shows the current date and time.")]
        public Task<ActionResult> NowAsync()
            => Ok(new EmbedBuilder().WithTitle(Context.Now.ToDiscordTimestamp(TimestampType.LongDateTime)));
    }
}