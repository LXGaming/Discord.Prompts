using Discord;

namespace LXGaming.Discord.Prompts;

public sealed class PromptMessage(
    AllowedMentions? allowedMentions,
    MessageComponent? components,
    string? content,
    bool? delete,
    Embed[]? embeds) {

    public AllowedMentions? AllowedMentions { get; } = allowedMentions;
    public MessageComponent? Components { get; } = components;
    public string? Content { get; } = content;
    public bool? Delete { get; } = delete;
    public Embed[]? Embeds { get; } = embeds;
}