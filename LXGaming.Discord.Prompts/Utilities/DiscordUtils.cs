using System.Collections.Immutable;
using Discord;

namespace LXGaming.Discord.Prompts.Utilities;

public static class DiscordUtils {

    public static void AddComponents(IComponentContainer componentContainer, INestedComponent nestedComponent) {
        foreach (var component in nestedComponent.Components) {
            componentContainer.AddComponent(component.ToBuilder());
        }
    }

    public static ImmutableHashSet<ulong> CreateImmutableHashSet(IEnumerable<IEntity<ulong>>? entities) {
        return entities?.Select(entity => entity.Id).ToImmutableHashSet() ?? ImmutableHashSet<ulong>.Empty;
    }

    public static Optional<T> CreateOptional<T>(T? value) {
        return value != null ? new Optional<T>(value) : Optional<T>.Unspecified;
    }
}