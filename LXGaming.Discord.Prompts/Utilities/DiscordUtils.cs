using Discord;

namespace LXGaming.Discord.Prompts.Utilities;

public static class DiscordUtils {

    public static void AddRow(ComponentBuilder componentBuilder, MessageComponent messageComponent) {
        foreach (var actionRowComponent in messageComponent.Components) {
            var actionRowBuilder = new ActionRowBuilder();
            actionRowBuilder.Components.AddRange(actionRowComponent.Components);
            componentBuilder.AddRow(actionRowBuilder);
        }
    }

    public static Optional<T> CreateOptional<T>(T? value) {
        return value != null ? new Optional<T>(value) : Optional<T>.Unspecified;
    }
}