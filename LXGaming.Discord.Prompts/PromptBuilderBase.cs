using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPromptBuilder : PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPrompt : PromptBase {

    public ISet<IRole>? Roles { get; set; }
    public ISet<IUser>? Users { get; set; }
    public PromptMessage? CancelMessage { get; set; } = new PromptMessageBuilder()
        .WithEmbeds(new EmbedBuilder()
            .WithColor(Color.Red)
            .WithFooter("Cancelled")
            .Build())
        .Build();
    public PromptMessage? ExpireMessage { get; set; } = new PromptMessageBuilder()
        .WithEmbeds(new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithFooter("Expired")
            .Build())
        .Build();

    public abstract TPrompt Build();

    public TPromptBuilder WithRoles(params IRole[] roles) {
        if (roles.Length == 0) {
            return (TPromptBuilder) this;
        }

        Roles ??= new HashSet<IRole>();
        foreach (var role in roles) {
            Roles.Add(role);
        }

        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithUsers(params IUser[] users) {
        if (users.Length == 0) {
            return (TPromptBuilder) this;
        }

        Users ??= new HashSet<IUser>();
        foreach (var user in users) {
            Users.Add(user);
        }

        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithCancelMessage(PromptMessage? cancelMessage) {
        CancelMessage = cancelMessage;
        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithExpireMessage(PromptMessage? expireMessage) {
        ExpireMessage = expireMessage;
        return (TPromptBuilder) this;
    }
}