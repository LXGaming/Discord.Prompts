using Discord;
using LXGaming.Discord.Prompts.Pagination;

namespace LXGaming.Discord.Prompts.Utilities;

public static class Extensions {

    public static async Task<IUserMessage> FollowupAsync(this PromptService promptService,
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null, bool isTTS = false,
        bool ephemeral = false, RequestOptions? options = null, PollProperties? poll = null,
        MessageFlags flags = MessageFlags.None) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.FollowupAsync(page.Attachments, page.Content, page.Embeds, isTTS, ephemeral,
            page.AllowedMentions, prompt.GetComponents(page), null, options, poll, flags).ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    public static async Task<IUserMessage> FollowupAsync(this PromptService promptService,
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, Embed? embed = null,
        RequestOptions? options = null, PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        var message = await interaction.FollowupAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions,
            prompt.Components, embed, options, poll, flags).ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    internal static Task<IUserMessage> FollowupAsync(this IDiscordInteraction interaction,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null,
        MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null,
        PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        if (attachments != null) {
            return interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions,
                components, embed, options, poll, flags);
        }

        return interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options,
            poll, flags);
    }

    public static async Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService,
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null,
        RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = DiscordUtils.CreateOptional(page.Content);
            properties.Embeds = DiscordUtils.CreateOptional(page.Embeds);
            properties.Components = DiscordUtils.CreateOptional(prompt.GetComponents(page));
            properties.AllowedMentions = DiscordUtils.CreateOptional(page.AllowedMentions);
            properties.Attachments = DiscordUtils.CreateOptional(page.Attachments);
        }, options).ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    public static async Task<IUserMessage> ModifyOriginalResponseAsync(this PromptService promptService,
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = DiscordUtils.CreateOptional(text);
            properties.Embed = DiscordUtils.CreateOptional(embed);
            properties.Embeds = DiscordUtils.CreateOptional(embeds);
            properties.Components = DiscordUtils.CreateOptional(prompt.Components);
            properties.AllowedMentions = DiscordUtils.CreateOptional(allowedMentions);
            properties.Attachments = DiscordUtils.CreateOptional(attachments);
        }, options).ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    public static async Task<IUserMessage> RespondAsync(this PromptService promptService,
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null, bool isTTS = false,
        bool ephemeral = false, RequestOptions? options = null, PollProperties? poll = null,
        MessageFlags flags = MessageFlags.None) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        await interaction.RespondAsync(page.Attachments, page.Content, page.Embeds, isTTS, ephemeral,
            page.AllowedMentions, prompt.GetComponents(page), null, options, poll, flags).ConfigureAwait(false);
        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    public static async Task<IUserMessage> RespondAsync(this PromptService promptService,
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, Embed? embed = null,
        RequestOptions? options = null, PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        await interaction.RespondAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, prompt.Components,
            embed, options, poll, flags).ConfigureAwait(false);
        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        await promptService.RegisterAsync(message, prompt, timeout).ConfigureAwait(false);
        return message;
    }

    internal static Task RespondAsync(this IDiscordInteraction interaction,
        IEnumerable<FileAttachment>? attachments = null, string? text = null, Embed[]? embeds = null,
        bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null,
        MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null,
        PollProperties? poll = null, MessageFlags flags = MessageFlags.None) {
        if (attachments != null) {
            return interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions,
                components, embed, options, poll, flags);
        }

        return interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options,
            poll, flags);
    }
}