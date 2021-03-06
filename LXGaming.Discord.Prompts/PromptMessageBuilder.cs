using Discord;

namespace LXGaming.Discord.Prompts;

public sealed class PromptMessageBuilder {

    public AllowedMentions? AllowedMentions { get; set; }
    public string? Content { get; set; }
    public bool? Delete { get; set; }
    public List<Embed>? Embeds { get; set; }

    public PromptMessage Build() {
        return new PromptMessage(AllowedMentions, Content, Delete, Embeds?.ToArray());
    }

    public PromptMessageBuilder WithAllowedMentions(AllowedMentions? allowedMentions) {
        AllowedMentions = allowedMentions;
        return this;
    }

    public PromptMessageBuilder WithContent(string? content) {
        Content = content;
        return this;
    }

    public PromptMessageBuilder WithDelete(bool? delete) {
        Delete = delete;
        return this;
    }

    public PromptMessageBuilder WithEmbeds(params Embed[] embeds) {
        if (embeds.Length == 0) {
            return this;
        }

        Embeds ??= new List<Embed>();
        Embeds.AddRange(embeds);
        return this;
    }
}