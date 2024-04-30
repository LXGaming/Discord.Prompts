using System.Collections.Concurrent;

namespace LXGaming.Discord.Prompts.Pagination.Lazy;

public class LazyPaginationPrompt(
    IReadOnlyCollection<ulong> roleIds,
    IReadOnlyCollection<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage,
    Func<int, Task<PromptMessage>> action,
    bool cachePages,
    int totalPages) : PaginationPromptBase(roleIds, userIds, cancelMessage, expireMessage, invalidUserMessage) {

    public Func<int, Task<PromptMessage>> Action { get; } = action;

    public ConcurrentDictionary<int, PromptMessage>? CachedPages { get; } = cachePages
        ? new ConcurrentDictionary<int, PromptMessage>()
        : null;

    public override int TotalPages { get; } = totalPages;

    public override async Task<PromptMessage> GetPageAsync(int index) {
        if (CachedPages != null && CachedPages.TryGetValue(index, out var existingPage)) {
            return existingPage;
        }

        var page = await Action(index).ConfigureAwait(false);
        if (CachedPages != null) {
            CachedPages[index] = page;
        }

        return page;
    }
}