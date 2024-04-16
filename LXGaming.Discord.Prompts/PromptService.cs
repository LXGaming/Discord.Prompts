using System.Collections.Concurrent;
using Discord;
using LXGaming.Discord.Prompts.Pagination;
using LXGaming.Discord.Prompts.Utilities;
using Microsoft.Extensions.Logging;

namespace LXGaming.Discord.Prompts;

public class PromptService(
    IDiscordClient client,
    ILogger<PromptService> logger,
    PromptServiceConfig config) : IAsyncDisposable {

    private readonly ConcurrentDictionary<ulong, CancellableTaskImpl> _promptTasks = new();
    private bool _disposed;

    public async Task<PromptResult> ExecuteAsync(IComponentInteraction interaction) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_promptTasks.TryGetValue(interaction.Message.Id, out var existingPromptTask)) {
            return new PromptResult {
                Message = $"{interaction.Message.Id} is not registered",
                Status = PromptStatus.UnregisteredMessage
            };
        }

        var prompt = existingPromptTask.Prompt;
        if (!prompt.IsValidUser(interaction.User)) {
            return new PromptResult {
                Message = $"{interaction.User.Id} is not valid",
                Status = PromptStatus.InvalidUser
            };
        }

        PromptResult result;
        try {
            result = await prompt.ExecuteAsync(interaction).ConfigureAwait(false);
        } catch (Exception ex) {
            return new PromptResult {
                Exception = ex,
                Message = ex.Message,
                Status = PromptStatus.Exception
            };
        }

        if (result.Unregister) {
            await UnregisterAsync(interaction.Message.Id, true).ConfigureAwait(false);
        }

        return result;
    }

    public Task RegisterAsync(IUserMessage message, PromptBase prompt, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_promptTasks.ContainsKey(message.Id)) {
            throw new InvalidOperationException("Message is already registered");
        }

        var delay = timeout ?? config.DefaultTimeout;

        logger.LogTrace("Registering prompt {Id} for expiration in {Delay}", message.Id, delay);

        var channelId = message.Channel.Id;
        var messageId = message.Id;
        var promptTask = _promptTasks.GetOrAdd(message.Id, _ => new CancellableTaskImpl(prompt));
        return promptTask.StartAsync(async () => {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                promptTask.CancellationToken,
                cancellationToken);
            try {
                await Task.Delay(delay, cancellationTokenSource.Token).ConfigureAwait(false);
            } catch (TaskCanceledException) {
                if (promptTask.Stopped) {
                    return;
                }
            }

            var channel = await client.GetChannelAsync(channelId).ConfigureAwait(false);
            if (channel is not IMessageChannel messageChannel) {
                logger.LogWarning("Channel {Id} not an {Type}", channelId, nameof(IMessageChannel));
                return;
            }

            var promptMessage = cancellationTokenSource.IsCancellationRequested
                ? promptTask.Prompt.CancelMessage
                : promptTask.Prompt.ExpireMessage;
            if (promptMessage == null) {
                return;
            }

            if (promptMessage.Delete ?? false) {
                await messageChannel.DeleteMessageAsync(messageId).ConfigureAwait(false);
            } else {
                await messageChannel.ModifyMessageAsync(messageId, properties => {
                    properties.Content = DiscordUtils.CreateOptional(promptMessage.Content);
                    properties.Embeds = DiscordUtils.CreateOptional(promptMessage.Embeds);
                    properties.Components = DiscordUtils.CreateOptional(promptMessage.Components ?? new ComponentBuilder().Build());
                    properties.AllowedMentions = DiscordUtils.CreateOptional(promptMessage.AllowedMentions);
                    properties.Attachments = DiscordUtils.CreateOptional(promptMessage.Attachments);
                }).ConfigureAwait(false);
            }
        }).ContinueWith(_ => UnregisterAsync(messageId), CancellationToken.None);
    }

    public async Task<bool> UnregisterAsync(ulong key, bool stop = false) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_promptTasks.TryRemove(key, out var existingPromptTask)) {
            return false;
        }

        logger.LogTrace("Unregistering prompt {Id} ({Stop})", key, stop);

        if (stop) {
            await existingPromptTask.StopAsync().ConfigureAwait(false);
        }

        await existingPromptTask.DisposeAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, PaginationPromptBase prompt,
        TimeSpan? timeout = null, bool isTTS = false, bool ephemeral = false, RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        IUserMessage message;
        if (page.Attachments != null) {
            message = await interaction.FollowupWithFilesAsync(page.Attachments, page.Content, page.Embeds, isTTS,
                ephemeral, page.AllowedMentions, prompt.GetComponents(page), null, options).ConfigureAwait(false);
        } else {
            message = await interaction.FollowupAsync(page.Content, page.Embeds, isTTS, ephemeral, page.AllowedMentions,
                prompt.GetComponents(page), null, options).ConfigureAwait(false);
        }

        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, PromptBase prompt,
        TimeSpan? timeout = null, IEnumerable<FileAttachment>? attachments = null, string? text = null,
        Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null,
        Embed? embed = null, RequestOptions? options = null) {
        IUserMessage message;
        if (attachments != null) {
            message = await interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral,
                allowedMentions, prompt.Components, embed, options).ConfigureAwait(false);
        } else {
            message = await interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions,
                prompt.Components, embed, options).ConfigureAwait(false);
        }

        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction,
        PaginationPromptBase prompt, TimeSpan? timeout = null, RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = DiscordUtils.CreateOptional(page.Content);
            properties.Embeds = DiscordUtils.CreateOptional(page.Embeds);
            properties.Components = DiscordUtils.CreateOptional(prompt.GetComponents(page));
            properties.AllowedMentions = DiscordUtils.CreateOptional(page.AllowedMentions);
            properties.Attachments = DiscordUtils.CreateOptional(page.Attachments);
        }, options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, PromptBase prompt,
        TimeSpan? timeout = null, IEnumerable<FileAttachment>? attachments = null, string? text = null,
        Embed[]? embeds = null, AllowedMentions? allowedMentions = null, Embed? embed = null,
        RequestOptions? options = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = DiscordUtils.CreateOptional(text);
            properties.Embed = DiscordUtils.CreateOptional(embed);
            properties.Embeds = DiscordUtils.CreateOptional(embeds);
            properties.Components = DiscordUtils.CreateOptional(prompt.Components);
            properties.AllowedMentions = DiscordUtils.CreateOptional(allowedMentions);
            properties.Attachments = DiscordUtils.CreateOptional(attachments);
        }, options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, PaginationPromptBase prompt,
        TimeSpan? timeout = null, bool isTTS = false,
        bool ephemeral = false, RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        if (page.Attachments != null) {
            await interaction.RespondWithFilesAsync(page.Attachments, page.Content, page.Embeds, isTTS, ephemeral,
                page.AllowedMentions, prompt.GetComponents(page), null, options).ConfigureAwait(false);
        } else {
            await interaction.RespondAsync(page.Content, page.Embeds, isTTS, ephemeral, page.AllowedMentions,
                prompt.GetComponents(page), null, options).ConfigureAwait(false);
        }

        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, PromptBase prompt,
        TimeSpan? timeout = null, IEnumerable<FileAttachment>? attachments = null, string? text = null,
        Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null,
        Embed? embed = null, RequestOptions? options = null) {
        if (attachments != null) {
            await interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions,
                prompt.Components, embed, options).ConfigureAwait(false);
        } else {
            await interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, prompt.Components, embed,
                options).ConfigureAwait(false);
        }

        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing) {
        if (_disposed) {
            return;
        }

        if (disposing) {
            foreach (var pair in _promptTasks) {
                await pair.Value.DisposeAsync().ConfigureAwait(false);
            }
        }

        _disposed = true;
    }
}