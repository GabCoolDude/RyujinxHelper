﻿using System.Threading.Tasks;
using Discord.WebSocket;
using Volte.Commands.Text;

namespace Volte.Interactive
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public ValueTask<bool> JudgeAsync(VolteContext sourceContext, SocketReaction parameter) 
            => new ValueTask<bool>(parameter.UserId == sourceContext.User.Id);
    }
}
