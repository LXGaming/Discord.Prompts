using Discord;
using LXGaming.Common.Threading.Tasks;
using LXGaming.Discord.Prompts.Utilities;
using Microsoft.Extensions.Logging;

namespace LXGaming.Discord.Prompts;

public class PromptService : IAsyncDisposable {

    private readonly IDiscordClient _client;
    private readonly ILogger<PromptService> _logger;
    private readonly PromptServiceOptions _options;
    private readonly CancellableTaskCollection<PromptKey> _promptTasks;
    private bool _disposed;

    public PromptService(IDiscordClient client, ILogger<PromptService> logger, PromptServiceOptions options) {
        _client = client;
        _logger = logger;
        _options = options;
        _promptTasks = new CancellableTaskCollection<PromptKey>();

        _promptTasks.Added += (_, args) => {
            _logger.LogTrace("Registered prompt {Id} with timeout {Timeout}", args.Key.MessageId, args.Key.Timeout);
            return Task.CompletedTask;
        };
        _promptTasks.Removed += (_, args) => {
            _logger.LogTrace("Unregistered prompt {Id}", args.Key.MessageId);
            return Task.CompletedTask;
        };
        _promptTasks.UnhandledException += (_, args) => {
            _logger.LogError(args.Exception, "Encountered an error while handling prompt {Id}", args.Key.MessageId);
            return Task.CompletedTask;
        };
    }

    public async Task<PromptResult> ExecuteAsync(IComponentInteraction interaction) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = _promptTasks.FirstOrDefault(key => key.MessageId == interaction.Message.Id);
        if (key == null) {
            return new PromptResult {
                Message = $"{interaction.Message.Id} is not registered",
                Status = PromptStatus.UnregisteredMessage
            };
        }

        PromptResult result;
        try {
            var prompt = key.Prompt;
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
            await _promptTasks.RemoveAsync(key, false).ConfigureAwait(false);
        }

        return result;
    }

    public Task<bool> RegisterAsync(IUserMessage message, PromptBase prompt, TimeSpan? timeout = null) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var channel = message.Channel;
        var guildId = (message.Channel as IGuildChannel)?.GuildId;
        var channelId = message.Channel.Id;
        var messageId = message.Id;
        var userId = message.Author.Id;
        var delay = timeout ?? _options.DefaultTimeout;
        var key = new PromptKey(guildId, channelId, messageId, userId, prompt, delay);
        return _promptTasks.AddAsync(key, async context => {
            try {
                await Task.Delay(delay, context.CancelToken).ConfigureAwait(false);
            } catch (TaskCanceledException) {
                if (!context.StopToken.IsCancellationRequested) {
                    return;
                }
            }

            var promptMessage = context.CancelToken.IsCancellationRequested
                ? prompt.CancelMessage?.Invoke()
                : prompt.ExpireMessage?.Invoke();
            if (promptMessage == null) {
                return;
            }

            if (promptMessage.Delete == true) {
                await channel.DeleteMessageAsync(messageId).ConfigureAwait(false);
            } else {
                await channel.ModifyMessageAsync(messageId, properties => {
                    properties.Content = DiscordUtils.CreateOptional(promptMessage.Content);
                    properties.Embeds = DiscordUtils.CreateOptional(promptMessage.Embeds);
                    properties.Components = DiscordUtils.CreateOptional(promptMessage.Components ?? MessageComponent.Empty);
                    properties.AllowedMentions = DiscordUtils.CreateOptional(promptMessage.AllowedMentions);
                    properties.Attachments = DiscordUtils.CreateOptional(promptMessage.Attachments);
                }).ConfigureAwait(false);
            }
        });
    }

    public Task UnregisterAllAsync(bool stop = true) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _promptTasks.RemoveAllAsync(stop);
    }

    public Task UnregisterAllAsync(Predicate<PromptKey> match, bool stop = true) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _promptTasks.RemoveAllAsync(match, stop);
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore() {
        if (_disposed) {
            return;
        }

        _disposed = true;

        await _promptTasks.DisposeAsync().ConfigureAwait(false);
    }
}