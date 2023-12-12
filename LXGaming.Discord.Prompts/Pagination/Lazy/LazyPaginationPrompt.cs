using System.Collections.Concurrent;

namespace LXGaming.Discord.Prompts.Pagination.Lazy;

public class LazyPaginationPrompt(
    ulong[] roleIds,
    ulong[] userIds,
    PromptMessage? cancelMessage,
    PromptMessage? expireMessage,
    Func<int, Task<PromptMessage>> action,
    int totalPages) : PaginationPromptBase(roleIds, userIds, cancelMessage, expireMessage) {

    public Func<int, Task<PromptMessage>> Action { get; } = action;
    public override int TotalPages { get; } = totalPages;

    private readonly ConcurrentDictionary<int, PromptMessage> _cachedPages = new();

    public override async Task<PromptMessage> GetPageAsync(int index) {
        if (_cachedPages.TryGetValue(index, out var existingPage)) {
            return existingPage;
        }

        var page = await Action(index).ConfigureAwait(false);
        _cachedPages[index] = page;
        return page;
    }
}