using Discord;
using LXGaming.Discord.Prompts.Utilities;

namespace LXGaming.Discord.Prompts.Custom;

public class CustomPromptBuilder : PromptBuilderBase<CustomPromptBuilder, CustomPrompt> {

    public MessageComponent? Components { get; set; }
    public Func<IComponentInteraction, Task<bool>>? Action { get; set; }

    public override CustomPrompt Build() {
        if (Components == null) { throw new InvalidOperationException(nameof(Components)); }
        if (Action == null) { throw new InvalidOperationException(nameof(Action)); }

        return new CustomPrompt(DiscordUtils.CreateImmutableHashSet(Roles),
            DiscordUtils.CreateImmutableHashSet(Users), CancelMessage, ExpireMessage, InvalidUserMessage, Components, Action);
    }

    public CustomPromptBuilder WithComponents(MessageComponent? components) {
        Components = components;
        return this;
    }

    public CustomPromptBuilder WithAction(Func<IComponentInteraction, Task<bool>>? action) {
        Action = action;
        return this;
    }
}