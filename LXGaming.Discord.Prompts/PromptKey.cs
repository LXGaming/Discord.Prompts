namespace LXGaming.Discord.Prompts;

public sealed record PromptKey(
    ulong? GuildId,
    ulong ChannelId,
    ulong MessageId,
    ulong UserId,
    PromptBase Prompt,
    TimeSpan Timeout) {

    public override string ToString() {
        return MessageId.ToString();
    }
}