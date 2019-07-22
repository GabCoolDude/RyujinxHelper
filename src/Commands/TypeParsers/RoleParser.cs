using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Gommon;
using Qmmands;

namespace Volte.Commands.TypeParsers
{
    public sealed class RoleParser<TRole> : TypeParser<TRole> where TRole : IRole
    {
        public override Task<TypeParserResult<TRole>> ParseAsync(
            Parameter param,
            string value,
            ICommandContext context,
            IServiceProvider provider)
        {
            var ctx = (VolteContext) context;
            TRole role = default;
            if (ulong.TryParse(value, out var id) || MentionUtils.TryParseRole(value, out id))
                role = ctx.Guild.GetRole(id).Cast<TRole>();

            if (role is null)
            {
                var match = ctx.Guild.Roles.Where(x => x.Name.EqualsIgnoreCase(value)).ToList();
                if (match.Count > 1)
                    return Task.FromResult(TypeParserResult<TRole>.Unsuccessful(
                        "Multiple roles found. Try mentioning the role or using its ID.")
                    );

                role = match.FirstOrDefault().Cast<TRole>();
            }

            return role is null
                ? Task.FromResult(TypeParserResult<TRole>.Unsuccessful($"Role `{value}` not found."))
                : Task.FromResult(TypeParserResult<TRole>.Successful(role));
        }
    }
}