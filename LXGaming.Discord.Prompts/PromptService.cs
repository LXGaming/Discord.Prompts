using System.Collections.Concurrent;
using Discord;
using LXGaming.Discord.Prompts.Confirmation;
using LXGaming.Discord.Prompts.Custom;
using LXGaming.Discord.Prompts.Pagination;
using LXGaming.Discord.Prompts.Utilities;
using Microsoft.Extensions.Logging;

namespace LXGaming.Discord.Prompts;

public class PromptService : IAsyncDisposable {

    private readonly IDiscordClient _client;
    private readonly ILogger<PromptService> _logger;
    private readonly PromptServiceConfig _config;
    private readonly ConcurrentDictionary<ulong, CancellableTaskImpl> _promptTasks;
    private bool _disposed;

    public PromptService(IDiscordClient client, ILogger<PromptService> logger, PromptServiceConfig config) {
        _client = client;
        _logger = logger;
        _config = config;
        _promptTasks = new ConcurrentDictionary<ulong, CancellableTaskImpl>();
    }

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

        var delay = timeout ?? _config.DefaultTimeout;

        _logger.LogTrace("Registering prompt {Id} for expiration in {Delay}", message.Id, delay);

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

            var channel = await _client.GetChannelAsync(channelId).ConfigureAwait(false);
            if (channel is not IMessageChannel messageChannel) {
                _logger.LogWarning("Channel {Id} not an {Type}", channelId, nameof(IMessageChannel));
                return;
            }

            var promptMessage = cancellationTokenSource.IsCancellationRequested ? promptTask.Prompt.CancelMessage : promptTask.Prompt.ExpireMessage;
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

        _logger.LogTrace("Unregistering prompt {Id} ({Stop})", key, stop);

        if (stop) {
            await existingPromptTask.StopAsync().ConfigureAwait(false);
        }

        await existingPromptTask.DisposeAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.FollowupAsync(
            text,
            embed: embed,
            allowedMentions: allowedMentions,
            components: prompt.Components).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = text;
            properties.Embed = embed;
            properties.AllowedMentions = allowedMentions;
            properties.Components = prompt.Components;
        }).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        await interaction.RespondAsync(
            text,
            embed: embed,
            allowedMentions: allowedMentions,
            components: prompt.Components).ConfigureAwait(false);
        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.FollowupAsync(
            text,
            embed: embed,
            allowedMentions: allowedMentions,
            components: prompt.Components).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = text;
            properties.Embed = embed;
            properties.AllowedMentions = allowedMentions;
            properties.Components = prompt.Components;
        }).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan? timeout = null,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        await interaction.RespondAsync(
            text,
            embed: embed,
            allowedMentions: allowedMentions,
            components: prompt.Components).ConfigureAwait(false);
        var message = await interaction.GetOriginalResponseAsync().ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.FollowupAsync(
            page.Content,
            page.Embeds,
            false,
            false,
            page.AllowedMentions,
            prompt.GetComponents(page)).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = page.Content;
            properties.Embeds = page.Embeds;
            properties.AllowedMentions = page.AllowedMentions;
            properties.Components = prompt.GetComponents(page);
        }).ConfigureAwait(false);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan? timeout = null) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage).ConfigureAwait(false);
        await interaction.RespondAsync(
            page.Content,
            page.Embeds,
            false,
            false,
            page.AllowedMentions,
            prompt.GetComponents(page)).ConfigureAwait(false);
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