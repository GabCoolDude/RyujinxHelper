﻿using System.ComponentModel;
using RyuBot.Commands.Text;

namespace RyuBot.Interactive;

public class EnsureFromUserCriterion : ICriterion<IMessage>
{
    private readonly ulong _id;

    public EnsureFromUserCriterion(IUser user)
        => _id = user.Id;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public EnsureFromUserCriterion(ulong id)
        => _id = id;

    public ValueTask<bool> JudgeAsync(VolteContext sourceContext, IMessage parameter) 
        => new(_id == parameter.Author.Id);

}