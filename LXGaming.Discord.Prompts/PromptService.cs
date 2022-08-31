using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using LXGaming.Discord.Prompts.Confirmation;
using LXGaming.Discord.Prompts.Custom;
using LXGaming.Discord.Prompts.Pagination;
using LXGaming.Discord.Prompts.Utilities;
using Microsoft.Extensions.Logging;

namespace LXGaming.Discord.Prompts;

public class PromptService : IAsyncDisposable {

    private readonly BaseSocketClient _client;
    private readonly ILogger<PromptService> _logger;
    private readonly PromptServiceConfig _config;
    private readonly ConcurrentDictionary<ulong, CancellableTask> _promptTasks;
    private bool _disposed;

    public PromptService(BaseSocketClient client, ILogger<PromptService> logger, PromptServiceConfig config) {
        _client = client;
        _logger = logger;
        _config = config;
        _promptTasks = new ConcurrentDictionary<ulong, CancellableTask>();
    }

    public async Task<bool> ExecuteAsync(IComponentInteraction interaction) {
        if (!_promptTasks.TryGetValue(interaction.Message.Id, out var existingPromptTask)) {
            return false;
        }

        var prompt = existingPromptTask.Prompt;
        if (!prompt.IsValidUser(interaction.User)) {
            return true;
        }

        if (await prompt.ExecuteAsync(interaction)) {
            await UnregisterAsync(interaction.Message.Id, true);
        }

        return true;
    }

    public Task RegisterAsync(IUserMessage message, PromptBase prompt, TimeSpan? timeout = null, CancellationToken cancellationToken = default) {
        if (_promptTasks.ContainsKey(message.Id)) {
            throw new InvalidOperationException("Message is already registered");
        }

        var delay = timeout ?? _config.DefaultTimeout;

        _logger.LogTrace("Registering prompt {Id} for expiration in {Delay}", message.Id, delay);

        var channelId = message.Channel.Id;
        var messageId = message.Id;
        var promptTask = _promptTasks.GetOrAdd(message.Id, _ => new CancellableTask(prompt));
        return promptTask.StartAsync(async () => {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                promptTask.CancellationToken,
                cancellationToken);
            try {
                await Task.Delay(delay, cancellationTokenSource.Token);
            } catch (TaskCanceledException) {
                if (promptTask.Stopped) {
                    return;
                }
            }

            if (_client.GetChannel(channelId) is not IMessageChannel channel) {
                _logger.LogWarning("Channel {Id} not an {Type}", channelId, nameof(IMessageChannel));
                return;
            }

            var promptMessage = cancellationTokenSource.IsCancellationRequested ? promptTask.Prompt.CancelMessage : promptTask.Prompt.ExpireMessage;
            if (promptMessage == null) {
                return;
            }

            if (promptMessage.Delete ?? false) {
                await channel.DeleteMessageAsync(messageId);
            } else {
                await channel.ModifyMessageAsync(messageId, properties => {
                    properties.Content = promptMessage.Content;
                    properties.Embeds = promptMessage.Embeds;
                    properties.Components = new ComponentBuilder().Build();
                    properties.AllowedMentions = promptMessage.AllowedMentions;
                });
            }
        }).ContinueWith(_ => UnregisterAsync(messageId), CancellationToken.None);
    }

    public async Task<bool> UnregisterAsync(ulong key, bool stop = false) {
        if (!_promptTasks.TryRemove(key, out var existingPromptTask)) {
            return false;
        }

        _logger.LogTrace("Unregistering prompt {Id} ({Stop})", key, stop);

        if (stop) {
            await existingPromptTask.StopAsync();
        }

        await existingPromptTask.DisposeAsync();
        return true;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.FollowupAsync(text, embed: embed, allowedMentions: allowedMentions, components: prompt.Components);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = text;
            properties.Embed = embed;
            properties.AllowedMentions = allowedMentions;
            properties.Components = prompt.Components;
        });
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, CustomPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        await interaction.RespondAsync(text, embed: embed, allowedMentions: allowedMentions, components: prompt.Components);
        var message = await interaction.GetOriginalResponseAsync();
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.FollowupAsync(text, embed: embed, allowedMentions: allowedMentions, components: prompt.Components);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = text;
            properties.Embed = embed;
            properties.AllowedMentions = allowedMentions;
            properties.Components = prompt.Components;
        });
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, ConfirmationPrompt prompt, TimeSpan timeout,
        string? text = null, Embed? embed = null, AllowedMentions? allowedMentions = null) {
        await interaction.RespondAsync(text, embed: embed, allowedMentions: allowedMentions, components: prompt.Components);
        var message = await interaction.GetOriginalResponseAsync();
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> FollowupAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan timeout) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage);
        var message = await interaction.FollowupAsync(
            page.Content,
            page.Embeds,
            false,
            false,
            page.AllowedMentions,
            prompt.Components);
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> ModifyOriginalResponseAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan timeout) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage);
        var message = await interaction.ModifyOriginalResponseAsync(properties => {
            properties.Content = page.Content;
            properties.Embeds = page.Embeds;
            properties.AllowedMentions = page.AllowedMentions;
            properties.Components = prompt.Components;
        });
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async Task<IUserMessage> RespondAsync(IDiscordInteraction interaction, PaginationPromptBase prompt, TimeSpan timeout) {
        var page = await prompt.GetPageAsync(prompt.CurrentPage);
        await interaction.RespondAsync(
            page.Content,
            page.Embeds,
            false,
            false,
            page.AllowedMentions,
            prompt.Components);
        var message = await interaction.GetOriginalResponseAsync();
        _ = RegisterAsync(message, prompt, timeout);
        return message;
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing) {
        if (_disposed) {
            return;
        }

        if (disposing) {
            foreach (var key in _promptTasks.Keys) {
                await UnregisterAsync(key);
            }
        }

        _disposed = true;
    }
}