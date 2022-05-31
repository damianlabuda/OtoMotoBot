using Shared.Entities;

namespace Scraper.Interfaces;

public interface ISearchAuctionsService
{
    Task<List<AdLink>> Search(SearchLink searchLink);
}