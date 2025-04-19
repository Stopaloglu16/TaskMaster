﻿using Microsoft.EntityFrameworkCore;

namespace Application.Common.Models;


public class PagingResponse<T>
{
   
    public PagingResponse(IReadOnlyCollection<T> items)
    {
        Items = items;
    }

    public IReadOnlyCollection<T> Items { get; }

    public int PageSize { get; set; }
    public int PageNumber { get; set; } //Current page number
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public int TotalCount { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;


    public static async Task<PagingResponse<T>> CreateAsync(IQueryable<T> source, PagingParameters pagingParameters)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize).Take(pagingParameters.PageSize).ToListAsync();

        return new PagingResponse<T>(items) { PageNumber = pagingParameters.PageNumber, PageSize = pagingParameters.PageSize, TotalCount = count };
    }
}