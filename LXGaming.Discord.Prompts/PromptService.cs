using System.Collections.Concurrent;
using Discord;
using LXGaming.Discord.Prompts.Utilities;
using Microsoft.Extensions.Logging;

namespace LXGaming.Discord.Prompts;

public class PromptService(
    IDiscordClient client,
    ILogger<PromptService> logger,
    PromptServiceOptions options) : IAsyncDisposable {

    private readonly ConcurrentDictionary<ulong, CancellablePrompt> _promptTasks = new();
    private bool _disposed;

    public async Task<PromptResult> ExecuteAsync(IComponentInteraction interaction) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_promptTasks.TryGetValue(interaction.Message.Id, out var existingPromptTask)) {
            return new PromptResult {
                Message = $"{interaction.Message.Id} is not registered",
                Status = PromptStatus.UnregisteredMessage
            };
        }

        PromptResult result;
        try {
            var prompt = existingPromptTask.Prompt;
            if (!prompt.IsValidUser(interaction.User)) {
                var invalidUserMessage = prompt.InvalidUserMessage?.Invoke();
                if (invalidUserMessage != null) {
                    await interaction.RespondAsync(invalidUserMessage.Attachments, invalidUserMessage.Content,
                        invalidUserMessage.Embeds, false, true, invalidUserMessage.AllowedMentions,
                        invalidUserMessage.Components).ConfigureAwait(false);
                }

                return new PromptResult {
                    Message = $"{interaction.User.Id} is not valid",
                    Status = PromptStatus.InvalidUser
                };
            }

            result = await prompt.ExecuteAsync(interaction).ConfigureAwait(false);
        } catch (Exception ex) {
            return new PromptResult {
                Exception = ex,
                Message = ex.Message,
                Status = PromptStatus.Exception
            };
        }

        if (result.Unregister) {
            await UnregisterAsync(interaction.Message.Id, false).ConfigureAwait(false);
        }

        return result;
    }

    public Task RegisterAsync(IUserMessage message, PromptBase prompt, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var channelId = message.Channel.Id;
        var messageId = message.Id;
        var delay = timeout ?? options.DefaultTimeout;

        if (_promptTasks.ContainsKey(messageId)) {
            throw new InvalidOperationException("Message is already registered");
        }

        logger.LogTrace("Registering prompt {Id} for expiration in {Delay}", messageId, delay);

        return _promptTasks.GetOrAdd(messageId, _ => new CancellablePrompt(async context => {
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                context.CancelToken,
                cancellationToken);
            try {
                await Task.Delay(delay, linkedSource.Token).ConfigureAwait(false);
            } catch (TaskCanceledException) {
                if (!context.StopToken.IsCancellationRequested) {
                    return;
                }
            }

            var promptMessage = linkedSource.IsCancellationRequested
                ? prompt.CancelMessage?.Invoke()
                : prompt.ExpireMessage?.Invoke();
            if (promptMessage == null) {
                return;
            }

            var channel = await client.GetChannelAsync(channelId).ConfigureAwait(false);
            if (channel is not IMessageChannel messageChannel) {
                logger.LogWarning("Channel {Id} not an {Type}", channelId, nameof(IMessageChannel));
                return;
            }

            if (promptMessage.Delete == true) {
                await messageChannel.DeleteMessageAsync(messageId).ConfigureAwait(false);
            } else {
                await messageChannel.ModifyMessageAsync(messageId, properties => {
                    properties.Content = DiscordUtils.CreateOptional(promptMessage.Content);
                    properties.Embeds = DiscordUtils.CreateOptional(promptMessage.Embeds);
                    properties.Components = DiscordUtils.CreateOptional(promptMessage.Components ?? MessageComponent.Empty);
                    properties.AllowedMentions = DiscordUtils.CreateOptional(promptMessage.AllowedMentions);
                    properties.Attachments = DiscordUtils.CreateOptional(promptMessage.Attachments);
                }).ConfigureAwait(false);
            }
        }, prompt)).StartAsync().ContinueWith(_ => UnregisterAsync(messageId, false), CancellationToken.None);
    }

    public async Task UnregisterAllAsync() {
        List<Exception>? exceptions = null;
        foreach (var pair in _promptTasks) {
            try {
                await UnregisterAsync(pair.Key).ConfigureAwait(false);
            } catch (Exception ex) {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        if (exceptions != null) {
            throw new AggregateException("Encountered an error while unregistering prompts", exceptions);
        }
    }

    public async Task<bool> UnregisterAsync(ulong key, bool stop = true) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_promptTasks.TryRemove(key, out var existingPromptTask)) {
            return false;
        }

        logger.LogTrace("Unregistering prompt {Id} ({Stop})", key, stop);

        try {
            if (stop) {
                await existingPromptTask.StopAsync().ConfigureAwait(false);
            }
        } finally {
            await existingPromptTask.DisposeAsync().ConfigureAwait(false);
        }

        return true;
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