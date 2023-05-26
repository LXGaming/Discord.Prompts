using LXGaming.Common.Threading.Tasks;

namespace LXGaming.Discord.Prompts.Utilities;

public class CancellableTaskImpl : CancellableTask {

    public PromptBase Prompt { get; }

    public CancellableTaskImpl(PromptBase prompt) {
        Prompt = prompt;
    }
}