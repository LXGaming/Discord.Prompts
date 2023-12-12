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

        var result = await prompt.ExecuteAsync(interaction).ConfigureAwait(false);
        if (result.Unregister) {
            await UnregisterAsync(interaction.Message.Id, true).ConfigureAwait(false);
        }

        return result;
    }

    public Task RegisterAsync(IUserMessage message, PromptBase prompt, TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
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
                    properties.Content = promptMessage.Content;
                    properties.Embeds = promptMessage.Embeds;
                    properties.Components = promptMessage.Components ?? new ComponentBuilder().Build();
                    properties.AllowedMentions = promptMessage.AllowedMentions;
                }).ConfigureAwait(false);
            }
        }).ContinueWith(_ => UnregisterAsync(messageId), CancellationToken.None);
    }

    public async Task<bool> UnregisterAsync(ulong key, bool stop = false) {
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

    public async Task<IUserMessage> FollowupAsync(
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null,
        bool isTTS = false, bool ephemeral = false, RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.FollowupAsync(
            page.Content,
            page.Embeds,
            isTTS,
            ephemeral,
            page.AllowedMentions,
            prompt.GetComponents(page),
            null,
            options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null) {
        var message = await interaction.FollowupAsync(
            text,
            embeds,
            isTTS,
            ephemeral,
            allowedMentions,
            prompt.Components,
            embed,
            options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null,
        RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = page.Content;
            properties.Embeds = page.Embeds;
            properties.Components = prompt.GetComponents(page);
            properties.AllowedMentions = page.AllowedMentions;
        }, options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        string? text = null, Embed[]? embeds = null, AllowedMentions? allowedMentions = null,
        Embed? embed = null, RequestOptions? options = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = text;
            properties.Embed = embed;
            properties.Embeds = embeds;
            properties.Components = prompt.Components;
            properties.AllowedMentions = allowedMentions;
        }, options).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(
        IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null,
        bool isTTS = false, bool ephemeral = false, RequestOptions? options = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        await interaction.RespondAsync(
            page.Content,
            page.Embeds,
            isTTS,
            ephemeral,
            page.AllowedMentions,
            prompt.GetComponents(page),
            null,
            options).ConfigureAwait(false);
        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(
        IDiscordInteraction interaction, PromptBase prompt, TimeSpan? timeout = null,
        string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false,
        AllowedMentions? allowedMentions = null, Embed? embed = null, RequestOptions? options = null) {
        await interaction.RespondAsync(
            text,
            embeds,
            isTTS,
            ephemeral,
            allowedMentions,
            prompt.Components,
            embed,
            options).ConfigureAwait(false);
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
            foreach (var key in _promptTasks.Keys) {
                await UnregisterAsync(key).ConfigureAwait(false);
            }
        }

        _disposed = true;
    }
}