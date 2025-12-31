namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPrompt(
    IReadOnlyCollection<ulong> roleIds,
    IReadOnlyCollection<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage,
    IReadOnlyList<PromptMessage> pages)
    : PaginationPromptBase(roleIds, userIds, cancelMessage, expireMessage, invalidUserMessage) {

    public IReadOnlyList<PromptMessage> Pages { get; } = pages;
    public override int TotalPages => Pages.Count;

    public override Task<PromptMessage> GetPageAsync(int index) {
        return Task.FromResult(Pages[index]);
    }
}