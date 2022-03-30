using Discord;

namespace LXGaming.Discord.Prompts;

public sealed class PromptMessage {

    public AllowedMentions? AllowedMentions { get; }
    public string? Content { get; }
    public bool? Delete { get; }
    public Embed[]? Embeds { get; }

    public PromptMessage(AllowedMentions? allowedMentions, string? content, bool? delete, Embed[]? embeds) {
        AllowedMentions = allowedMentions;
        Content = content;
        Delete = delete;
        Embeds = embeds;
    }
}