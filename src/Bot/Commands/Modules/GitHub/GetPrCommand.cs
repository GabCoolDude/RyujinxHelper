﻿using System.Text.RegularExpressions;
using Discord.Interactions;
using RyuBot.Interactions;

namespace Volte.Interactions.Commands.Modules;

public partial class GitHubModule
{
    private static readonly Regex PrBuildPattern = PrBuildRegex();

    [SlashCommand("pull-request", "Show the title & build downloads of a Pull Request on the Ryujinx GitHub.")]
    public async Task<RuntimeResult> PrAsync(
        [Summary("pr_number", "The Pull Request to display.")]
        int prNumber,
        [Summary("ignore_builds", "Don't show the build downloads on the reply.")]
        bool ignoreBuilds = false)
    {
        

        var pr = await GitHub.GetPullRequestAsync(Context, prNumber);

        if (pr is null)
            return BadRequest($"Pull Request {prNumber} not found.");

        if (Context.Guild?.Id is 1291765437100720243)
            ignoreBuilds = true;
        
        if (!ignoreBuilds)
            await DeferAsync();
        
        //ryu-mirror PR builds aren't the best; users in that server should benefit from the speedup of doing one less REST request.

        var comments = !ignoreBuilds ? await GitHub.GetCommentsForIssueAsync(Context, prNumber) : [];
        var buildComment = comments.FirstOrDefault(x => x.User.Login == "github-actions[bot]");
        var builds = new Dictionary<string, string>();

        foreach (var line in buildComment?.Body?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? [])
        {
            if (PrBuildPattern.IsMatch(line, out var match))
            {
                builds.Add(FormatRid(match.Groups["RuntimeIdentifier"].Value), match.Groups["DownloadUrl"].Value);
            }
        }

        return Ok(CreateReplyBuilder()
            .WithButtons(Buttons.Link(pr.HtmlUrl, "Open on GitHub"))
            .WithEmbed(embed =>
            {
                embed.WithAuthor(pr.User.Name, pr.User.AvatarUrl, pr.HtmlUrl);
                embed.WithTitle($"[{pr.Number}] {pr.Title}".Truncate(EmbedBuilder.MaxTitleLength));
                embed.AddField("Labels", pr.FormatLabels());

                if (builds.Count > 0)
                {
                    var buildsText = String(sb =>
                    {
                        if (builds.Count > 0)
                            sb.AppendLine().AppendLine()
                                .AppendLine("## Downloads: ")
                                .AppendLine(
                                    "*You must have an account on GitHub and be logged in in order to download these.*")
                                .AppendLine().AppendLine();

                        foreach (var (rid, downloadUrl) in builds)
                        {
                            sb.AppendLine(Format.Bold(Format.Url($"{rid} download", downloadUrl)));
                        }
                    });

                    var prBody = pr.Body.Truncate(EmbedBuilder.MaxDescriptionLength - buildsText.Length);

                    embed.WithDescription($"{prBody}{buildsText}");
                }
                else
                {
                    embed.WithDescription(pr.Body.Truncate(EmbedBuilder.MaxDescriptionLength));
                }

                embed.WithColor(GetColorBasedOnIssueState(pr));
                embed.WithFooter(FormatIssueState(pr));
            }));
    }

    private static string FormatRid(string inputRid) => inputRid.ToLower() switch
    {
        "linux_arm64" => "Linux ARM64",
        "linux_x64" => "Linux x64",
        "win_x64" => "Windows x64",
        "macos_universal" => "macOS Universal",
        _ => inputRid
    };

    [GeneratedRegex(@"\* \[ryujinx-Release-1\.2\.0\+\w{7}-(?<RuntimeIdentifier>\w+)\]\((?<DownloadUrl>.+)\)")]
    private static partial Regex PrBuildRegex();
}