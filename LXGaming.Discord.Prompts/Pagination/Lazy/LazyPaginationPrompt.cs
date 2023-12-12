using System.Collections.Concurrent;

namespace LXGaming.Discord.Prompts.Pagination.Lazy;

public class LazyPaginationPrompt : PaginationPromptBase {

    public Func<int, Task<PromptMessage>> Action { get; }
    public override int TotalPages { get; }

    private readonly ConcurrentDictionary<int, PromptMessage> _cachedPages;

    public LazyPaginationPrompt(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage,
        Func<int, Task<PromptMessage>> action, int totalPages) : base(roleIds, userIds, cancelMessage, expireMessage) {
        Action = action;
        TotalPages = totalPages;
        _cachedPages = new ConcurrentDictionary<int, PromptMessage>();
    }

    public override async Task<PromptMessage> GetPageAsync(int index) {
        if (_cachedPages.TryGetValue(index, out var existingPage)) {
            return existingPage;
        }

        var page = await Action(index).ConfigureAwait(false);
        _cachedPages[index] = page;
        return page;
    }
}