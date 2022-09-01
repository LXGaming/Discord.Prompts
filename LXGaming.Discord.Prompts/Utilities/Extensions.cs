using Discord;
using LXGaming.Discord.Prompts.Confirmation;
using LXGaming.Discord.Prompts.Custom;
using LXGaming.Discord.Prompts.Pagination;

namespace LXGaming.Discord.Prompts.Utilities;

public static class Extensions {

    public static Task<IUserMessage> FollowupAsync(this PromptService promptService, IInteractionContext context, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.FollowupAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService, IInteractionContext context, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.ModifyOriginalResponseAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> RespondAsync(this PromptService promptService, IInteractionContext context, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.RespondAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> FollowupAsync(this PromptService promptService, IInteractionContext context, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.FollowupAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService, IInteractionContext context, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.ModifyOriginalResponseAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> RespondAsync(this PromptService promptService, IInteractionContext context, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        return promptService.RespondAsync(context.Interaction, prompt, timeout, text, embed, allowedMentions);
    }

    public static Task<IUserMessage> FollowupAsync(this PromptService promptService, IInteractionContext context, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        return promptService.FollowupAsync(context.Interaction, prompt, timeout);
    }

    public static Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService, IInteractionContext context, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        return promptService.ModifyOriginalResponseAsync(context.Interaction, prompt, timeout);
    }

    public static Task<IUserMessage> RespondAsync(this PromptService promptService, IInteractionContext context, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        return promptService.RespondAsync(context.Interaction, prompt, timeout);
    }
}