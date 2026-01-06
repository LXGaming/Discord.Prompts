using System.Collections.Frozen;
using Discord;

namespace LXGaming.Discord.Prompts.Confirmation;

public class ConfirmationPrompt(
    FrozenSet<ulong> roleIds,
    FrozenSet<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage,
    MessageComponent components,
    Func<IComponentInteraction, bool, Task<bool>> action)
    : PromptBase(roleIds, userIds, cancelMessage, expireMessage, invalidUserMessage) {

    public const string FalseKey = "false";
    public const string TrueKey = "true";

    public override MessageComponent Components { get; } = components;
    public Func<IComponentInteraction, bool, Task<bool>> Action { get; } = action;

    public override async Task<PromptResult> ExecuteAsync(IComponentInteraction component) {
        var id = component.Data.CustomId;
        bool value;
        if (string.Equals(id, FalseKey)) {
            value = false;
        } else if (string.Equals(id, TrueKey)) {
            value = true;
        } else {
            return new PromptResult {
                Message = $"{id} is not supported",
                Status = PromptStatus.UnsupportedComponent
            };
        }

        var result = await Action(component, value).ConfigureAwait(false);
        return new PromptResult {
            Status = PromptStatus.Success,
            Unregister = result
        };
    }
}