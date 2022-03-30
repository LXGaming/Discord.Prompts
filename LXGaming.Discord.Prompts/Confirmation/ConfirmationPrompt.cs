using Discord;

namespace LXGaming.Discord.Prompts.Confirmation;

public class ConfirmationPrompt : PromptBase {

    public const string FalseKey = "false";
    public const string TrueKey = "true";

    public override MessageComponent Components { get; }
    public Func<IComponentInteraction, bool, Task<bool>> Action { get; }

    public ConfirmationPrompt(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage,
        MessageComponent components, Func<IComponentInteraction, bool, Task<bool>> action) : base(roleIds, userIds, cancelMessage, expireMessage) {
        Components = components;
        Action = action;
    }

    public override async Task<bool> ExecuteAsync(IComponentInteraction component) {
        var id = component.Data.CustomId;
        bool value;
        if (string.Equals(id, FalseKey)) {
            value = false;
        } else if (string.Equals(id, TrueKey)) {
            value = true;
        } else {
            throw new InvalidOperationException($"{id} is not supported");
        }

        return await Action(component, value);
    }
}