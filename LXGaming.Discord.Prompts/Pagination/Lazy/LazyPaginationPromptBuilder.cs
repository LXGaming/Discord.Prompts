﻿using LXGaming.Discord.Prompts.Utilities;

namespace LXGaming.Discord.Prompts.Pagination.Lazy;

public class LazyPaginationPromptBuilder : PromptBuilderBase<LazyPaginationPromptBuilder, LazyPaginationPrompt> {

    public Func<int, Task<PromptMessage>>? Action { get; set; }
    public bool CachePages { get; set; } = true;
    public int TotalPages { get; set; }

    public override LazyPaginationPrompt Build() {
        if (Action == null) { throw new InvalidOperationException(nameof(Action)); }
        if (TotalPages <= 0) { throw new IndexOutOfRangeException(nameof(TotalPages)); }

        return new LazyPaginationPrompt(DiscordUtils.CreateImmutableHashSet(Roles),
            DiscordUtils.CreateImmutableHashSet(Users), CancelMessage, ExpireMessage, InvalidUserMessage, Action,
            CachePages, TotalPages);
    }

    public LazyPaginationPromptBuilder WithAction(Func<int, Task<PromptMessage>>? action) {
        Action = action;
        return this;
    }

    public LazyPaginationPromptBuilder WithCachePages(bool cachePages) {
        CachePages = cachePages;
        return this;
    }

    public LazyPaginationPromptBuilder WithTotalPages(int totalPages) {
        TotalPages = totalPages;
        return this;
    }
}