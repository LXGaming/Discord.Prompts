using Discord;
using LXGaming.Discord.Prompts.Pagination;

namespace LXGaming.Discord.Prompts.Utilities;

public static class Extensions {

    public static Task<IUserMessage> FollowupAsync(this PromptService promptService, IInteractionContext context,
        PaginationPromptBase prompt, TimeSpan? timeout = null, bool isTTS = false, bool ephemeral = false,
        RequestOptions? options = null, PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        return promptService.FollowupAsync(context.Interaction, prompt, timeout, isTTS, ephemeral, options, poll,
            flags);
    }

    public static Task<IUserMessage> FollowupAsync(this PromptService promptService, IInteractionContext context,
        PromptBase prompt, TimeSpan? timeout = null, IEnumerable<FileAttachment>? attachments = null,
        string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null,
        PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        return promptService.FollowupAsync(context.Interaction, prompt, timeout, attachments, text, embeds, isTTS,
            ephemeral, allowedMentions, embed, options, poll, flags);
    }

    public static Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService,
        IInteractionContext context, PaginationPromptBase prompt, TimeSpan? timeout = null,
        RequestOptions? options = null) {
        return promptService.ModifyOriginalResponseAsync(context.Interaction, prompt, timeout, options);
    }

    public static Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService,
        IInteractionContext context, PromptBase prompt, TimeSpan? timeout = null,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null) {
        return promptService.ModifyOriginalResponseAsync(context.Interaction, prompt, timeout, attachments, text,
            embeds, allowedMentions, embed, options);
    }

    public static Task<IUserMessage> RespondAsync(this PromptService promptService, IInteractionContext context,
        PaginationPromptBase prompt, TimeSpan? timeout = null, bool isTTS = false, bool ephemeral = false,
        RequestOptions? options = null, PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        return promptService.RespondAsync(context.Interaction, prompt, timeout, isTTS, ephemeral, options, poll, flags);
    }

    public static Task<IUserMessage> RespondAsync(this PromptService promptService, IInteractionContext context,
        PromptBase prompt, TimeSpan? timeout = null, IEnumerable<FileAttachment>? attachments = null,
        string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null,
        PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        return promptService.RespondAsync(context.Interaction, prompt, timeout, attachments, text, embeds, isTTS,
            ephemeral, allowedMentions, embed, options, poll, flags);
    }
}