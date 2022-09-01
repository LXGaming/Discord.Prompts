namespace LXGaming.Discord.Prompts;

public class PromptResult {

    public string? Message { get; init; }

    public PromptStatus Status { get; init; }

    public bool Unregister { get; init; }
}