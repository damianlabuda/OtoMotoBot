using Shared.Entities;

namespace Scraper.Interfaces;

public interface IAdLinksService
{
    Task GetAdLinksCount(SearchLink searchLink, User user);
}