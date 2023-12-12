namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPrompt(
    ulong[] roleIds,
    ulong[] userIds,
    PromptMessage? cancelMessage,
    PromptMessage? expireMessage,
    PromptMessage[] pages)
    : PaginationPromptBase(roleIds, userIds, cancelMessage, expireMessage) {

    public PromptMessage[] Pages { get; } = pages;
    public override int TotalPages => Pages.Length;

    public override Task<PromptMessage> GetPageAsync(int index) {
        return Task.FromResult(Pages[index]);
    }
}