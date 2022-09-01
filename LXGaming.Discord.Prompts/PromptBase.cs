using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBase {

    public ulong[] RoleIds { get; }
    public ulong[] UserIds { get; }
    public PromptMessage? CancelMessage { get; }
    public PromptMessage? ExpireMessage { get; }
    public abstract MessageComponent Components { get; }

    protected PromptBase(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage) {
        RoleIds = roleIds;
        UserIds = userIds;
        CancelMessage = cancelMessage;
        ExpireMessage = expireMessage;
    }

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