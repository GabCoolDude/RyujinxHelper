using LiteDB;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace Volte.Services;

public sealed class DatabaseService : VolteService, IDisposable
{
    public static readonly LiteDatabase Database =
        new($"filename={FilePath.Data / "Volte.db"};upgrade=true;connection=direct");

    private readonly DiscordSocketClient _client;

    private readonly ILiteCollection<GuildData> _guildData;
    private readonly ILiteCollection<Reminder> _reminderData;
    private readonly ILiteCollection<StarboardDbEntry> _starboardData;

    public DatabaseService(DiscordSocketClient discordShardedClient)
    {
        _client = discordShardedClient;
        _guildData = Database.GetCollection<GuildData>("guilds");
        _reminderData = Database.GetCollection<Reminder>("reminders");
        _starboardData = Database.GetCollection<StarboardDbEntry>("starboard");
        _starboardData.EnsureIndex("composite_id",
            $"$.{nameof(StarboardDbEntry.GuildId)} + '_' + $.{nameof(StarboardDbEntry.Key)}");
    }

    public GuildData GetData(IGuild guild) => GetData(guild.Id);

    public ValueTask<GuildData> GetDataAsync(ulong id) => new(GetData(id));

    public GuildData GetData(ulong id)
    {
        return _guildData.ValueLock(() =>
        {
            var conf = _guildData.FindOne(g => g.Id == id);
            if (conf != null) return conf;
            var newConf = GuildData.CreateFrom(_client.GetGuild(id));
            _guildData.Insert(newConf);
            return newConf;
        });
    }

    public GuildData this[ulong guildId] => GetData(guildId);
    public GuildData this[IGuild guild] => GetData(guild);
    public HashSet<Reminder> this[IUser user] => GetReminders(user);
    
    public HashSet<Reminder> GetReminders(IUser user, IGuild guild = null) =>
        GetReminders(user.Id, guild?.Id ?? 0).ToHashSet();

    public HashSet<Reminder> GetReminders(ulong creator, ulong guild = 0)
        => GetAllReminders().Where(r => r.CreatorId == creator && (guild is 0 || r.GuildId == guild)).ToHashSet();

    public bool TryDeleteReminder(Reminder reminder) =>
        _reminderData.ValueLock(() => _reminderData.Delete(reminder.Id));

    public HashSet<Reminder> GetAllReminders() => _reminderData.ValueLock(() => _reminderData.FindAll().ToHashSet());

    public void CreateReminder(Reminder reminder) => _reminderData.ValueLock(() => _reminderData.Insert(reminder));

    public void Modify(ulong guildId, DataEditor modifier)
    {
        _guildData.LockedRef(coll =>
        {
            var data = GetData(guildId);
            modifier(data);
            Save(data);
        });
    }

    public void Save(GuildData newConfig)
    {
        _guildData.LockedRef(coll =>
        {
            coll.EnsureIndex(s => s.Id, true);
            coll.Update(newConfig);
        });
    }

    private StarboardDbEntry GetStargazersInternal(ulong guildId, ulong messageId)
        => _reminderData.ValueLock(() => _starboardData.FindOne(g => g.GuildId == guildId && g.Key == messageId));

    public StarboardEntry GetStargazers(ulong guildId, ulong messageId)
        => GetStargazersInternal(guildId, messageId)?.Value;


    public bool TryGetStargazers(ulong guildId, ulong messageId, [NotNullWhen(true)] out StarboardEntry entry)
    {
        entry = GetStargazersInternal(guildId, messageId)?.Value;
        return entry != null;
    }

    public void UpdateStargazers(StarboardEntry entry)
    {
        _starboardData.LockedRef(coll =>
        {
            coll.Upsert($"{entry.GuildId}_{entry.StarboardMessageId}", new StarboardDbEntry
            {
                GuildId = entry.GuildId,
                Key = entry.StarboardMessageId,
                Value = entry
            });

            coll.Upsert($"{entry.GuildId}_{entry.StarredMessageId}", new StarboardDbEntry
            {
                GuildId = entry.GuildId,
                Key = entry.StarredMessageId,
                Value = entry
            });
        });
    }

    public void RemoveStargazers(StarboardEntry entry)
    {
        _starboardData.LockedRef(coll =>
        {
            coll.Delete($"{entry.GuildId}_{entry.StarboardMessageId}");
            coll.Delete($"{entry.GuildId}_{entry.StarredMessageId}");
        });
    }

    public void Dispose()
        => Database.Dispose();
}