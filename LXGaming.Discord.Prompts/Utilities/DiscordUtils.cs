using Discord;

namespace LXGaming.Discord.Prompts.Utilities;

public static class DiscordUtils {

    public static Optional<T> CreateOptional<T>(T? value) {
        return value != null ? new Optional<T>(value) : Optional<T>.Unspecified;
    }
}