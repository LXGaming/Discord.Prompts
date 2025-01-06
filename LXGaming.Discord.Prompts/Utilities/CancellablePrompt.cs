using LXGaming.Common.Threading.Tasks;

namespace LXGaming.Discord.Prompts.Utilities;

public class CancellablePrompt(Func<CancellableTaskContext, Task> func, PromptBase prompt) : CancellableTask(func) {

    public PromptBase Prompt { get; } = prompt;
}