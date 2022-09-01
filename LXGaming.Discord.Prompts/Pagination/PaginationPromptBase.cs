using Discord;

namespace LXGaming.Discord.Prompts.Pagination;

public abstract class PaginationPromptBase : PromptBase {

    public override MessageComponent Components => new ComponentBuilder()
        .WithButton("First", "first", disabled: TotalPages < 3)
        .WithButton("Prev", "previous", disabled: TotalPages < 2)
        .WithButton($"Page {CurrentPage + 1} / {TotalPages}", "null", ButtonStyle.Secondary, disabled: true)
        .WithButton("Next", "next", disabled: TotalPages < 2)
        .WithButton("Last", "last", disabled: TotalPages < 3)
        .Build();
    public int CurrentPage { get; private set; }
    public abstract int TotalPages { get; }

    protected PaginationPromptBase(ulong[] roleIds, ulong[] userIds, PromptMessage? cancelMessage, PromptMessage? expireMessage)
        : base(roleIds, userIds, cancelMessage, expireMessage) {
    }

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

        await component.DeferAsync();

        var page = await GetPageAsync(CurrentPage);
        await component.ModifyOriginalResponseAsync(properties => {
            properties.Content = page.Content;
            properties.Embeds = page.Embeds;
            properties.Components = GetComponents(page);
            properties.AllowedMentions = page.AllowedMentions;
        });

        return new PromptResult {
            Status = PromptStatus.Success
        };
    }

    public MessageComponent GetComponents(PromptMessage page) {
        if (page.Components == null) {
            return Components;
        }

        var componentBuilder = new ComponentBuilder();
        Append(componentBuilder, page.Components);
        Append(componentBuilder, Components);
        return componentBuilder.Build();
    }

    private static void Append(ComponentBuilder componentBuilder, MessageComponent messageComponent) {
        foreach (var actionRowComponent in messageComponent.Components) {
            var actionRowBuilder = new ActionRowBuilder();
            actionRowBuilder.Components.AddRange(actionRowComponent.Components);
            componentBuilder.AddRow(actionRowBuilder);
        }
    }
}