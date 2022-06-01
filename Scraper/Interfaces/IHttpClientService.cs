namespace Scraper.Interfaces;

public interface IHttpClientService
{
    Task<HttpResponseMessage> Get(string url);
}