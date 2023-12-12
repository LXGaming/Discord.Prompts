using Discord;

namespace LXGaming.Discord.Prompts.Custom;

public class CustomPrompt(
    ulong[] roleIds,
    ulong[] userIds,
    PromptMessage? cancelMessage,
    PromptMessage? expireMessage,
    MessageComponent components,
    Func<IComponentInteraction, Task<bool>> action) : PromptBase(roleIds, userIds, cancelMessage, expireMessage) {

    public override MessageComponent Components { get; } = components;
    public Func<IComponentInteraction, Task<bool>> Action { get; } = action;

    public override async Task<PromptResult> ExecuteAsync(IComponentInteraction component) {
        var result = await Action(component).ConfigureAwait(false);
        return new PromptResult {
            Status = PromptStatus.Success,
            Unregister = result
        };
    }
}