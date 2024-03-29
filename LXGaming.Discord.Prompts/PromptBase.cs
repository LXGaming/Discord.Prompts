﻿using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBase(
    ulong[] roleIds,
    ulong[] userIds,
    PromptMessage? cancelMessage,
    PromptMessage? expireMessage) {

    public ulong[] RoleIds { get; } = roleIds;
    public ulong[] UserIds { get; } = userIds;
    public PromptMessage? CancelMessage { get; } = cancelMessage;
    public PromptMessage? ExpireMessage { get; } = expireMessage;
    public abstract MessageComponent Components { get; }

    public abstract Task<PromptResult> ExecuteAsync(IComponentInteraction interaction);

    public bool IsValidUser(IUser user) {
        if (user.IsBot || user.IsWebhook) {
            return false;
        }

        if (RoleIds.Length == 0 && UserIds.Length == 0) {
            return true;
        }

        if (UserIds.Contains(user.Id)) {
            return true;
        }

        return user is IGuildUser guildUser && RoleIds.Any(roleId => guildUser.RoleIds.Contains(roleId));
    }
}