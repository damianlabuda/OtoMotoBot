using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models.SearchLinkStructure;

namespace Scraper.Services
{
    public class SearchAuctionsService : ISearchAuctionsService
    {
        private SearchLink _searchLink;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchAuctionsService> _logger;
        private readonly List<AdLink> _adLinks = new();
        private readonly LinkExtensions _extensions = new();
        private readonly LinkVariables _variables = new();
        private readonly SemaphoreSlim _semaphoreSlim = new(10, 10);
        private readonly List<string> _persistedQuerysList = new()
        {
            "68185c38c934469b1c93bedd9aec361086b91b10a4efbbae682ee05229899e13",
            "082392db93e737b5af878c4cdb40cdfeb76a9751bf2b9d8992c2b7c048dd3c61"
        };
        private int TotalPages { get; set; } = 0;
        private int TotalCount { get; set; } = 0;

        public SearchAuctionsService(IHttpClientFactory httpClientFactory, ILogger<SearchAuctionsService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("OtomotoHttpClient");
            _logger = logger;
        }

        public async Task<List<AdLink>> Search(SearchLink searchLink)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            _searchLink = searchLink;
            
            while (_persistedQuerysList.Any())
            {
                int index = new Random().Next(_persistedQuerysList.Count);
                _extensions.persistedQuery.sha256Hash = _persistedQuerysList[index];
                _persistedQuerysList.RemoveAt(index);

                string firstLink = GenerateLinkToApi(1);

                if (string.IsNullOrEmpty(firstLink))
                    break;

                await GetAuctions(firstLink);

                if (TotalCount > 0)
                    break;
            }

            if (_searchLink.SearchCount != 0 && _searchLink.LastUpdateDateTime != null && _searchLink.SearchCount % 10 != 0) 
                DetermineHowManyPagesCheck();
            
            if (TotalPages > 1)
            {
                var links = Enumerable.Range(2, TotalPages - 1).Select(GenerateLinkToApi);
                var tasks = links.Select(GetAuctions);

                await Task.WhenAll(tasks);
            }

            stopwatch.Stop();

            if (_adLinks.Count > 0)
                _logger.LogInformation($"{DateTime.Now} - Znaleziono: {_adLinks.Count} z {TotalCount} rekordów, dla: {_searchLink.Link}, czas: {stopwatch.Elapsed}");

            if (_adLinks.Count == 0)
                _logger.LogWarning($"{DateTime.Now} - Znaleziono: {_adLinks.Count} z {TotalCount} rekordów, dla: {_searchLink.Link}, czas: {stopwatch.Elapsed}");

            return _adLinks;
        }
        
        private string GenerateLinkToApi(int page)
        {
            if (page == 1)
            {
                if (!Uri.IsWellFormedUriString(_searchLink.Link, UriKind.Absolute))
                {
                    return string.Empty;
                }

                Uri baseUri = new Uri(_searchLink.Link);

                if (baseUri.Host != "www.otomoto.pl")
                {
                    return string.Empty;
                }

                string[] searchTerms = baseUri.Segments
                    .Select(x => x.Replace("/", "")).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                var linkQueries = HttpUtility.ParseQueryString(baseUri.Query);
                Filter[] filtersFromUserLink = linkQueries.AllKeys
                    .Where(x => Regex.IsMatch(x, "(?<=search\\[).*?(?=])"))
                    .Select(x => new Filter() { name = Regex.Match(x, "(?<=search\\[).*?(?=])").Value, value = linkQueries[x] }).ToArray();

                Filter[] sortDesc =
                {
                    new() {name = "order", value = "created_at_first:desc"}
                };
                
                var filters = filtersFromUserLink.Concat(sortDesc).ToArray();

                _variables.filters = filters;
                _variables.searchTerms = searchTerms;
            }

            _variables.page = page;

            string jsonVariables = JsonConvert.SerializeObject(_variables);
            string jsonExtensions = JsonConvert.SerializeObject(_extensions);

            return $"https://www.otomoto.pl/graphql?operationName=listingScreen" +
                           $"&variables={HttpUtility.UrlEncode(jsonVariables)}" +
                           $"&extensions={HttpUtility.UrlEncode(jsonExtensions)}";
        }

        private async Task GetAuctions(string link)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    string httpResponse = await GetHttpRequest(link);

                    if (!string.IsNullOrEmpty(httpResponse))
                    {
                        if (GetAuctionsFromJsonApiString(httpResponse))
                        {
                            break;
                        }
                    }

                    await Task.Delay(2000);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<string> GetHttpRequest(string link)
        {
            try
            {
                var result = await _httpClient.GetAsync(link);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return string.Empty;
                }

                return result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return string.Empty;
            }
        }

        private bool GetAuctionsFromJsonApiString(string httpResponse)
        {
            if (!string.IsNullOrEmpty(httpResponse))
            {
                try
                {
                    JObject json = JObject.Parse(httpResponse);

                    IEnumerable<JToken> adsFromJson = json.SelectTokens("data.advertSearch.edges[*].node");
                    if (adsFromJson.Any())
                    {
                        var parsedAds = adsFromJson.Select(x => new
                        {
                            Id = x.SelectToken("id").ToString(), Price = x.SelectToken("price.amount.units").ToString()
                        }).ToList();

                        // Check is correct ads
                        parsedAds = parsedAds.Where(x =>
                            long.TryParse(x.Id, out long parsedId) && parsedId > 0 &&
                            double.TryParse(x.Price, out double parsedPrice) && parsedPrice > 0).ToList();
                        
                        if (adsFromJson.Count() != parsedAds.Count())
                            return false;

                        if (TotalPages == 0)
                            GetAmountPages(json);

                        var adsToAdd = parsedAds.DistinctBy(x => x.Id).Select(x => new AdLink()
                        { Id = long.Parse(x.Id), Price = double.Parse(x.Price) }).ToList();

                        _adLinks.AddRange(adsToAdd);
                        return true;
                    }

                    //var errorMessage = json.SelectToken("errors[0].message");
                    //if (errorMessage != null && errorMessage.ToString() == "PersistedQueryNotFound")
                    //{
                    //    _extensions.persistedQuery.sha256Hash = "082392db93e737b5af878c4cdb40cdfeb76a9751bf2b9d8992c2b7c048dd3c61";
                    //}
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return false;
                }
            }

            return false;
        }

        private void GetAmountPages(JObject json)
        {
            if (!double.TryParse(json.SelectToken("data.advertSearch.totalCount").ToString(), out double totalCount) && totalCount > 0)
            {
                return;
            }

            if ((int)totalCount > TotalCount)
            {
                TotalCount = (int)totalCount;
            }

            if (!double.TryParse(json.SelectToken("data.advertSearch.pageInfo.pageSize").ToString(), out double pageSize) && pageSize > 0)
            {
                return;
            }

            int totalPages = (int)Math.Ceiling(totalCount / pageSize);

            if (totalPages > TotalPages)
            {
                TotalPages = totalPages;
            }
        }
        
        private void DetermineHowManyPagesCheck()
        {
            var lastTimeUtcCheck = _searchLink.LastUpdateDateTime;
            var timeNowUtc = DateTime.UtcNow;

            if (timeNowUtc.AddHours(-1) < lastTimeUtcCheck)
                TotalPages = (int)Math.Ceiling((double)TotalPages / (double)100 * (double)10); 
        }
    }
}
