﻿using Discord.Interactions;

namespace RyuBot.Interactions.Results;

public class NoneResult() : RuntimeResult(null, string.Empty)
{
    public static implicit operator Task<NoneResult>(NoneResult input) 
        => Task.FromResult(input);
}