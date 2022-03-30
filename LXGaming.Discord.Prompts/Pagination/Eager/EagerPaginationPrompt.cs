namespace LXGaming.Discord.Prompts.Pagination.Eager;

public class EagerPaginationPrompt : PaginationPromptBase {

    public PromptMessage[] Pages { get; }
    public override int TotalPages => Pages.Length;

    public EagerPaginationPrompt(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage,
        PromptMessage[] pages) : base(roleIds, userIds, cancelMessage, expireMessage) {
        Pages = pages;
    }

    public override Task<PromptMessage> GetPageAsync(int index) {
        return Task.FromResult(Pages[index]);
    }
}