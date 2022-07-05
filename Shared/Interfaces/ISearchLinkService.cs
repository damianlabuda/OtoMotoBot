namespace Shared.Interfaces;

public interface ISearchLinkService
{
    string GenerateLinkToApi(string searchLink, int page, string persistedQuery);
}