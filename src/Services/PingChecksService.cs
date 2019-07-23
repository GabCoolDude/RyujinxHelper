using System.Threading.Tasks;
using Gommon;
using Volte.Core.Data.Models.EventArgs;

namespace Volte.Services
{
    [Service("PingChecks", "The main Service used for checking if any given message contains mass mentions.")]
    public sealed class PingChecksService
    {
        public async Task CheckMessageAsync(MessageReceivedEventArgs args)
        {
            if (args.Data.Configuration.Moderation.MassPingChecks &&
                !args.Context.User.IsAdmin(args.Context.ServiceProvider))
            {
                var content = args.Message.Content;
                if (content.ContainsIgnoreCase("@everyone") ||
                    content.ContainsIgnoreCase("@here") ||
                    args.Message.MentionedUsers.Count > 10)
                {
                    await args.Message.DeleteAsync();
                }
            }
        }
    }
}