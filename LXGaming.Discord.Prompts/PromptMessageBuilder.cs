using System.Collections.Immutable;
using Discord;

namespace LXGaming.Discord.Prompts;

public sealed class PromptMessageBuilder {

    public AllowedMentions? AllowedMentions { get; set; }
    public IList<FileAttachment>? Attachments { get; set; }
    public MessageComponent? Components { get; set; }
    public string? Content { get; set; }
    public bool? Delete { get; set; }
    public IList<Embed>? Embeds { get; set; }

    public PromptMessage Build() {
        return new PromptMessage(AllowedMentions, Attachments?.ToImmutableList(), Components, Content, Delete,
            Embeds?.ToArray());
    }

    public PromptMessageBuilder WithAllowedMentions(AllowedMentions? allowedMentions) {
        AllowedMentions = allowedMentions;
        return this;
    }

    public PromptMessageBuilder WithAttachments(params FileAttachment[] attachments) {
        return WithAttachments((IEnumerable<FileAttachment>) attachments);
    }

    public PromptMessageBuilder WithAttachments(IEnumerable<FileAttachment> attachments) {
        foreach (var attachment in attachments) {
            WithAttachment(attachment);
        }

        return this;
    }

    public PromptMessageBuilder WithAttachment(FileAttachment attachment) {
        Attachments ??= new List<FileAttachment>();
        Attachments.Add(attachment);
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
        return WithEmbeds((IEnumerable<Embed>) embeds);
    }

    public PromptMessageBuilder WithEmbeds(IEnumerable<Embed> embeds) {
        foreach (var embed in embeds) {
            WithEmbed(embed);
        }

        return this;
    }

    public PromptMessageBuilder WithEmbed(Embed embed) {
        Embeds ??= new List<Embed>();
        Embeds.Add(embed);
        return this;
    }
}