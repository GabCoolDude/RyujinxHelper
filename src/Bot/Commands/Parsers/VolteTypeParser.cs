﻿namespace RyuBot.Commands.Text;

public abstract class VolteTypeParser<T> : TypeParser<T>
{
    public abstract ValueTask<TypeParserResult<T>> ParseAsync(string value, RyujinxBotContext ctx);

    public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter _, string value,
        CommandContext context) => ParseAsync(value, context.Cast<RyujinxBotContext>());
}