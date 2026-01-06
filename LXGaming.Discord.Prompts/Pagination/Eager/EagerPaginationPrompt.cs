using System.Collections.Frozen;
using System.Collections.Immutable;

namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPrompt(
    FrozenSet<ulong> roleIds,
    FrozenSet<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage,
    ImmutableArray<PromptMessage> pages)
    : PaginationPromptBase(roleIds, userIds, cancelMessage, expireMessage, invalidUserMessage) {

    public ImmutableArray<PromptMessage> Pages { get; } = pages;
    public override int TotalPages => Pages.Length;

    public override Task<PromptMessage> GetPageAsync(int index) {
        return Task.FromResult(Pages[index]);
    }
}