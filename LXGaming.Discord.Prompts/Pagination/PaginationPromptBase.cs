using System.Collections.Frozen;
using Discord;
using LXGaming.Discord.Prompts.Utilities;

namespace LXGaming.Discord.Prompts.Pagination;

public abstract class PaginationPromptBase(
    FrozenSet<ulong> roleIds,
    FrozenSet<ulong> userIds,
    Func<PromptMessage>? cancelMessage,
    Func<PromptMessage>? expireMessage,
    Func<PromptMessage>? invalidUserMessage)
    : PromptBase(roleIds, userIds, cancelMessage, expireMessage, invalidUserMessage) {

    public override MessageComponent Components => new ComponentBuilderV2()
        .WithActionRow(new ActionRowBuilder()
            .WithButton("First", "first", disabled: TotalPages < 3)
            .WithButton("Prev", "previous", disabled: TotalPages < 2)
            .WithButton($"Page {CurrentPage + 1} / {TotalPages}", "null", ButtonStyle.Secondary, disabled: true)
            .WithButton("Next", "next", disabled: TotalPages < 2)
            .WithButton("Last", "last", disabled: TotalPages < 3))
        .Build();

    public int CurrentPage { get; private set; }
    public abstract int TotalPages { get; }

    public abstract Task<PromptMessage> GetPageAsync(int index);

    public override async Task<PromptResult> ExecuteAsync(IComponentInteraction component) {
        var id = component.Data.CustomId;
        if (string.Equals(id, "first")) {
            CurrentPage = 0;
        } else if (string.Equals(id, "previous")) {
            if (CurrentPage > 0) {
                CurrentPage -= 1;
            } else {
                CurrentPage = TotalPages - 1;
            }
        } else if (string.Equals(id, "next")) {
            if (CurrentPage < TotalPages - 1) {
                CurrentPage += 1;
            } else {
                CurrentPage = 0;
            }
        } else if (string.Equals(id, "last")) {
            CurrentPage = TotalPages - 1;
        } else {
            return new PromptResult {
                Message = $"{id} is not supported",
                Status = PromptStatus.UnsupportedComponent
            };
        }

        var deferTask = component.DeferAsync();
        var pageTask = GetPageAsync(CurrentPage);

        await Task.WhenAll(deferTask, pageTask).ConfigureAwait(false);

        var page = await pageTask.ConfigureAwait(false);
        await component.ModifyOriginalResponseAsync(properties => {
            properties.Content = DiscordUtils.CreateOptional(page.Content);
            properties.Embeds = DiscordUtils.CreateOptional(page.Embeds);
            properties.Components = DiscordUtils.CreateOptional(GetComponents(page));
            properties.AllowedMentions = DiscordUtils.CreateOptional(page.AllowedMentions);
            properties.Attachments = DiscordUtils.CreateOptional(page.Attachments);
        }).ConfigureAwait(false);

        return new PromptResult {
            Status = PromptStatus.Success
        };
    }

    public MessageComponent GetComponents(PromptMessage page) {
        if (page.Components == null) {
            return Components;
        }

        var componentBuilder = new ComponentBuilderV2();
        DiscordUtils.AddComponents(componentBuilder, page.Components);
        DiscordUtils.AddComponents(componentBuilder, Components);
        return componentBuilder.Build();
    }
}