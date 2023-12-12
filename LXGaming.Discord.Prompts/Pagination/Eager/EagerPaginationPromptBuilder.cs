using Discord;

namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPromptBuilder : PromptBuilderBase<EagerPaginationPromptBuilder, EagerPaginationPrompt> {

    public List<PromptMessage>? Pages { get; set; }

    public override EagerPaginationPrompt Build() {
        if (Pages == null) { throw new InvalidOperationException(nameof(Pages)); }
        if (Pages.Count <= 0) { throw new IndexOutOfRangeException(nameof(Pages)); }

        return new EagerPaginationPrompt(
            Roles?.Select(role => role.Id).ToArray() ?? Array.Empty<ulong>(),
            Users?.Select(user => user.Id).ToArray() ?? Array.Empty<ulong>(),
            CancelMessage,
            ExpireMessage,
            Pages.ToArray());
    }

    public EagerPaginationPromptBuilder WithPage(MessageComponent? components = null, string? content = null, params Embed[] embeds) {
        return WithPages(new PromptMessageBuilder()
            .WithComponents(components)
            .WithContent(content)
            .WithEmbeds(embeds)
            .Build());
    }

    public EagerPaginationPromptBuilder WithPages(params PromptMessage[] pages) {
        if (pages.Length == 0) {
            return this;
        }

        Pages ??= [];
        Pages.AddRange(pages);
        return this;
    }
}