using LXGaming.Common.Threading.Tasks;

namespace LXGaming.Discord.Prompts.Utilities;

public class CancellablePrompt(PromptBase prompt) : CancellableTask {

    public PromptBase Prompt { get; } = prompt;
}