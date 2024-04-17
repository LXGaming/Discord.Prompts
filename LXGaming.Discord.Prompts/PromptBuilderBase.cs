using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPromptBuilder : PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPrompt : PromptBase {

    public ISet<IRole>? Roles { get; set; }
    public ISet<IUser>? Users { get; set; }

    public Func<PromptMessage>? CancelMessage { get; set; } = () => new PromptMessageBuilder()
        .WithEmbeds(new EmbedBuilder()
            .WithColor(Color.Red)
            .WithFooter("Cancelled")
            .Build())
        .Build();

    public Func<PromptMessage>? ExpireMessage { get; set; } = () => new PromptMessageBuilder()
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

    public TPromptBuilder WithCancelMessage(AllowedMentions? allowedMentions = null,
        IEnumerable<FileAttachment>? attachments = null, MessageComponent? components = null, string? content = null,
        params Embed[]? embeds) {
        return WithCancelMessage(new PromptMessage(allowedMentions, attachments, components, content, false, embeds));
    }

    public TPromptBuilder WithCancelMessage(PromptMessage cancelMessage) {
        return WithCancelMessage(() => cancelMessage);
    }

    public TPromptBuilder WithCancelMessage(Func<PromptMessage>? cancelMessage) {
        CancelMessage = cancelMessage;
        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithExpireMessage(AllowedMentions? allowedMentions = null,
        IEnumerable<FileAttachment>? attachments = null, MessageComponent? components = null, string? content = null,
        params Embed[]? embeds) {
        return WithExpireMessage(new PromptMessage(allowedMentions, attachments, components, content, false, embeds));
    }

    public TPromptBuilder WithExpireMessage(PromptMessage expireMessage) {
        return WithExpireMessage(() => expireMessage);
    }

    public TPromptBuilder WithExpireMessage(Func<PromptMessage>? expireMessage) {
        ExpireMessage = expireMessage;
        return (TPromptBuilder) this;
    }
}