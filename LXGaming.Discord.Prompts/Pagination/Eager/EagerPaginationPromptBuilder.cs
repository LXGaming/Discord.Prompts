using System.Collections.Immutable;
using Discord;
using LXGaming.Discord.Prompts.Utilities;

namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPromptBuilder : PromptBuilderBase<EagerPaginationPromptBuilder, EagerPaginationPrompt> {

    public IList<PromptMessage>? Pages { get; set; }

    public override EagerPaginationPrompt Build() {
        if (Pages == null) { throw new InvalidOperationException(nameof(Pages)); }
        if (Pages.Count <= 0) { throw new IndexOutOfRangeException(nameof(Pages)); }

        return new EagerPaginationPrompt(DiscordUtils.CreateImmutableHashSet(Roles),
            DiscordUtils.CreateImmutableHashSet(Users), CancelMessage, ExpireMessage, InvalidUserMessage,
            Pages.ToImmutableArray());
    }

    public EagerPaginationPromptBuilder WithPages(params PromptMessage[] pages) {
        return WithPages((IEnumerable<PromptMessage>) pages);
    }

    public EagerPaginationPromptBuilder WithPages(IEnumerable<PromptMessage> pages) {
        foreach (var page in pages) {
            WithPage(page);
        }

        return this;
    }

    public EagerPaginationPromptBuilder WithPage(AllowedMentions? allowedMentions = null,
        IEnumerable<FileAttachment>? attachments = null, MessageComponent? components = null, string? content = null,
        params Embed[]? embeds) {
        return WithPage(new PromptMessage(allowedMentions, attachments, components, content, false, embeds));
    }

    public EagerPaginationPromptBuilder WithPage(PromptMessage page) {
        Pages ??= new List<PromptMessage>();
        Pages.Add(page);
        return this;
    }
}