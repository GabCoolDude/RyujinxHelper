﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using RyuBot;
using Version = RyuBot.Version;

namespace Gommon;

public static partial class Extensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection coll) =>
        coll.AddSingleton(new HttpClient
            {
                Timeout = 10.Seconds()
            })
            .AddSingleton(new CommandService(new CommandServiceConfiguration
            {
                IgnoresExtraArguments = true,
                StringComparison = StringComparison.OrdinalIgnoreCase,
                DefaultRunMode = RunMode.Sequential,
                SeparatorRequirement = SeparatorRequirement.SeparatorOrWhitespace,
                Separator = " ",
                NullableNouns = null
            }))
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Config.DebugEnabled || Version.IsDevelopment 
                    ? LogSeverity.Debug 
                    : LogSeverity.Verbose,
                GatewayIntents = Intents,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 10000,
                MessageCacheSize = 50
            }))
            .Apply(_ =>
            {
                if (!Config.SentryDsn.IsNullOrEmpty())
                    coll.AddSingleton(SentrySdk.Init(opts =>
                    {
                        opts.Dsn = Config.SentryDsn;
                        opts.Debug = IsDebugLoggingEnabled;
                        opts.DiagnosticLogger = new SentryTranslator();
                    }));
                
                //get all the classes that inherit BotService, and aren't abstract; add them to the service provider
                var l = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(IsEligibleService)
                    .Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Info(LogSource.Volte, $"Injected services [{l.Select(static x => x.Name.ReplaceIgnoreCase("Service", "")).JoinToString(", ")}] into the provider.");
            });

    private const GatewayIntents Intents
        = GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
           GatewayIntents.GuildMessages | GatewayIntents.GuildPresences | GatewayIntents.MessageContent;

    private static bool IsEligibleService(Type type) => type.Inherits<BotService>() && !type.IsAbstract;
}