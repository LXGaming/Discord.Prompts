using Discord;

namespace LXGaming.Discord.Prompts;

public sealed class PromptMessageBuilder {

    public AllowedMentions? AllowedMentions { get; set; }
    public List<FileAttachment>? Attachments { get; set; }
    public MessageComponent? Components { get; set; }
    public string? Content { get; set; }
    public bool? Delete { get; set; }
    public List<Embed>? Embeds { get; set; }

    public PromptMessage Build() {
        return new PromptMessage(AllowedMentions, Attachments, Components, Content, Delete, Embeds?.ToArray());
    }

    public PromptMessageBuilder WithAllowedMentions(AllowedMentions? allowedMentions) {
        AllowedMentions = allowedMentions;
        return this;
    }

    public PromptMessageBuilder WithAttachments(params FileAttachment[] attachments) {
        if (attachments.Length == 0) {
            return this;
        }

        Attachments ??= [];
        Attachments.AddRange(attachments);
        return this;
    }

    public PromptMessageBuilder WithComponents(MessageComponent? components) {
        Components = components;
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

        Embeds ??= [];
        Embeds.AddRange(embeds);
        return this;
    }
}