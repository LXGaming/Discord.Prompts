using Discord;

namespace LXGaming.Discord.Prompts;

public abstract class PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPromptBuilder : PromptBuilderBase<TPromptBuilder, TPrompt>
    where TPrompt : PromptBase {

    public ISet<IRole>? Roles { get; set; }
    public ISet<IUser>? Users { get; set; }

    public Func<PromptMessage>? CancelMessage { get; set; } = () => new PromptMessageBuilder()
        .WithEmbed(new EmbedBuilder()
            .WithColor(Color.Red)
            .WithFooter("Cancelled")
            .Build())
        .Build();

    public Func<PromptMessage>? ExpireMessage { get; set; } = () => new PromptMessageBuilder()
        .WithEmbed(new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithFooter("Expired")
            .Build())
        .Build();

    public Func<PromptMessage>? InvalidUserMessage { get; set; } = () => new PromptMessageBuilder()
        .WithContent("You do not have permission to interact with this prompt")
        .Build();

    public abstract TPrompt Build();

    public TPromptBuilder WithRoles(params IRole[] roles) {
        return WithRoles((IEnumerable<IRole>) roles);
    }

    public TPromptBuilder WithRoles(IEnumerable<IRole> roles) {
        foreach (var role in roles) {
            WithRole(role);
        }

        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithRole(IRole role) {
        Roles ??= new HashSet<IRole>();
        Roles.Add(role);
        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithUsers(params IUser[] users) {
        return WithUsers((IEnumerable<IUser>) users);
    }

    public TPromptBuilder WithUsers(IEnumerable<IUser> users) {
        foreach (var user in users) {
            WithUser(user);
        }

        return (TPromptBuilder) this;
    }

    public TPromptBuilder WithUser(IUser user) {
        Users ??= new HashSet<IUser>();
        Users.Add(user);
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

    public TPromptBuilder WithInvalidUserMessage(AllowedMentions? allowedMentions = null,
        IEnumerable<FileAttachment>? attachments = null, MessageComponent? components = null, string? content = null,
        params Embed[]? embeds) {
        return WithInvalidUserMessage(new PromptMessage(allowedMentions, attachments, components, content, false, embeds));
    }

    public TPromptBuilder WithInvalidUserMessage(PromptMessage invalidUserMessage) {
        return WithInvalidUserMessage(() => invalidUserMessage);
    }

    public TPromptBuilder WithInvalidUserMessage(Func<PromptMessage>? invalidUserMessage) {
        InvalidUserMessage = invalidUserMessage;
        return (TPromptBuilder) this;
    }
}