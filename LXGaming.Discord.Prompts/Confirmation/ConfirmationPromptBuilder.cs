using Discord;
using LXGaming.Discord.Prompts.Utilities;

namespace LXGaming.Discord.Prompts.Confirmation;

public class ConfirmationPromptBuilder : PromptBuilderBase<ConfirmationPromptBuilder, ConfirmationPrompt> {

    public MessageComponent? Components { get; set; }
    public Func<IComponentInteraction, bool, Task<bool>>? Action { get; set; }

    public override ConfirmationPrompt Build() {
        if (Action == null) { throw new InvalidOperationException(nameof(Action)); }

        Components ??= new ComponentBuilderV2()
            .WithActionRow(new ActionRowBuilder()
                .WithButton("Yes", ConfirmationPrompt.TrueKey, ButtonStyle.Success)
                .WithButton("No", ConfirmationPrompt.FalseKey, ButtonStyle.Danger))
            .Build();

        return new ConfirmationPrompt(DiscordUtils.CreateImmutableHashSet(Roles),
            DiscordUtils.CreateImmutableHashSet(Users), CancelMessage, ExpireMessage, InvalidUserMessage,
            Components, Action);
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