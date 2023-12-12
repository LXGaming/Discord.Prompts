﻿using Discord;

namespace LXGaming.Discord.Prompts.Custom;

public class CustomPrompt : PromptBase {

    public override MessageComponent Components { get; }
    public Func<IComponentInteraction, Task<bool>> Action { get; }

    public CustomPrompt(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage,
        MessageComponent components, Func<IComponentInteraction, Task<bool>> action) : base(roleIds, userIds, cancelMessage, expireMessage) {
        Components = components;
        Action = action;
    }

    public override async Task<PromptResult> ExecuteAsync(IComponentInteraction component) {
        var result = await Action(component).ConfigureAwait(false);
        return new PromptResult {
            Status = PromptStatus.Success,
            Unregister = result
        };
    }
}