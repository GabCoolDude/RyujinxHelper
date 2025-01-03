using JsonParser = System.Text.Json.JsonSerializer; // same class in namespace LiteDB
using System.Text.Json.Serialization;
using LiteDB;
using RyuBot.Commands.Text;

namespace RyuBot.Entities;

public sealed class Reminder
{
    public static Reminder CreateFrom(RyujinxBotContext ctx, DateTime end, string reminder) => new()
    {
        TargetTime = end,
        CreationTime = ctx.Now,
        CreatorId = ctx.User.Id,
        GuildId = ctx.Guild.Id,
        ChannelId = ctx.Channel.Id,
        MessageId = ctx.Message.Id,
        ReminderText = reminder
    };
        
    [BsonId, JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("target_timestamp")]
    public DateTime TargetTime { get; set; }
    [JsonPropertyName("creation_timestamp")]
    public DateTime CreationTime { get; set; }
    [JsonPropertyName("creator")]
    public ulong CreatorId { get; set; }
    [JsonPropertyName("guild")]
    public ulong GuildId { get; set; }
    [JsonPropertyName("channel")]
    public ulong ChannelId { get; set; }
    [JsonPropertyName("message")]
    public ulong MessageId { get; set; }
    [JsonPropertyName("reminder_for")]
    public string ReminderText { get; set; }

    public override string ToString()
        => JsonParser.Serialize(this, Config.JsonOptions);
}