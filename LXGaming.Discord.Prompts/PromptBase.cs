using System.Collections.Frozen;
using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBase(
    FrozenSet<ulong> roleIds,
    FrozenSet<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage) {

    public FrozenSet<ulong> RoleIds { get; } = roleIds;
    public FrozenSet<ulong> UserIds { get; } = userIds;
    public Func<PromptMessage>? CancelMessage { get; } = cancelMessage;
    public Func<PromptMessage>? ExpireMessage { get; } = expireMessage;
    public Func<PromptMessage>? InvalidUserMessage { get; } = invalidUserMessage;
    public abstract MessageComponent Components { get; }

    public abstract Task<PromptResult> ExecuteAsync(IComponentInteraction interaction);

    public virtual bool IsValidUser(IUser user) {
        if (user.IsBot || user.IsWebhook) {
            return false;
        }

        if (RoleIds.Count == 0 && UserIds.Count == 0) {
            return true;
        }

        if (UserIds.Contains(user.Id)) {
            return true;
        }

        if (user is IGuildUser guildUser) {
            return guildUser.RoleIds.Any(roleId => RoleIds.Contains(roleId));
        }

        return false;
    }
}