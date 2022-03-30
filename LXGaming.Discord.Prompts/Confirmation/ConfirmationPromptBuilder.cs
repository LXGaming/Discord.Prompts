using Discord;

namespace LXGaming.Discord.Prompts.Confirmation;

public class ConfirmationPromptBuilder : PromptBuilderBase<ConfirmationPromptBuilder, ConfirmationPrompt> {

    public MessageComponent? Components { get; set; }
    public Func<IComponentInteraction, bool, Task<bool>>? Action { get; set; }

    public override ConfirmationPrompt Build() {
        if (Action == null) { throw new InvalidOperationException(nameof(Action)); }

        Components ??= new ComponentBuilder()
            .WithButton("Yes", ConfirmationPrompt.TrueKey, ButtonStyle.Success)
            .WithButton("No", ConfirmationPrompt.FalseKey, ButtonStyle.Danger)
            .Build();

        return new ConfirmationPrompt(
            Roles?.Select(role => role.Id).ToArray() ?? Array.Empty<ulong>(),
            Users?.Select(user => user.Id).ToArray() ?? Array.Empty<ulong>(),
            CancelMessage,
            ExpireMessage,
            Components,
            Action);
    }

    public ConfirmationPromptBuilder WithComponents(MessageComponent? components) {
        Components = components;
        return this;
    }

    public ConfirmationPromptBuilder WithAction(Func<IComponentInteraction, bool, Task<bool>>? action) {
        Action = action;
        return this;
    }
}