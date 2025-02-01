﻿using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public partial class VerifierModule
{
    [SlashCommand("verify", "Verifies your modded Switch ownership via the Switch Verifier Homebrew.")]
    public async Task<RuntimeResult> VerifyAsync(
        [Summary(description: "The token generated by the Switch Verifier homebrew, after being given your hash.")] 
        string token)
    {
        if (Context.User is not SocketGuildUser member) return None();
        
        await DeferAsync(true);

        var response = await Verifier.VerifyAsync(Context.User.Id, token);

        if (response.Result is ResultCode.Success)
            await ((SocketGuildUser)Context.User).AddRoleAsync(1334992661198930001);
        else
            await Verifier.SendVerificationErrorModlogMessageAsync(member, response);

        return response.Result switch
        {
            ResultCode.Success => Ok("Success! You can now get help."),
            ResultCode.InvalidInput => BadRequest("An input value was invalid."),
            ResultCode.InvalidTokenLength => BadRequest("The provided token didn't match the expected length."),
            ResultCode.TokenIsZeroes => BadRequest("Token is all zeroes."),
            ResultCode.ExpiredToken => BadRequest("The provided token has expired."),
            _ => BadRequest("Invalid token.")
        };
    }
}