namespace LXGaming.Discord.Prompts;

public class PromptResult {

    public Exception? Exception { get; init; }

    public string? Message { get; init; }

    public required PromptStatus Status { get; init; }

    public bool Unregister { get; init; }
}