using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Interfaces;

namespace Scraper.Services
{
    public class SearchAuctionsService : ISearchAuctionsService
    {
        private SearchLink _searchLink;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchAuctionsService> _logger;
        private readonly ISearchLinkService _searchLinkService;
        private readonly List<AdLink> _adLinks = new();
        private readonly SemaphoreSlim _semaphoreSlim = new(10, 10);
        private readonly List<string> _persistedQuerysList = new()
        {
            "68185c38c934469b1c93bedd9aec361086b91b10a4efbbae682ee05229899e13",
            "082392db93e737b5af878c4cdb40cdfeb76a9751bf2b9d8992c2b7c048dd3c61",
            "f6a92bbfc960d6112b6fe7092fc78a9f79b39b719d75ba62667c78e405c0a35b",
            "209ab0eefbf8afc152b9cdddaf3aad5d9bbb1dffa22ee2bcc670f1233fc07196"
        };
        private string PersistedQuery { get; set; }
        private int TotalPages { get; set; }
        private int TotalCount { get; set; }

        public SearchAuctionsService(IHttpClientFactory httpClientFactory, ILogger<SearchAuctionsService> logger, ISearchLinkService searchLinkService)
        {
            _httpClient = httpClientFactory.CreateClient("OtomotoHttpClient");
            _logger = logger;
            _searchLinkService = searchLinkService;
        }

        public async Task<List<AdLink>> Search(SearchLink searchLink)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _searchLink = searchLink;

            while (_persistedQuerysList.Any())
            {
                int index = new Random().Next(_persistedQuerysList.Count);
                PersistedQuery = _persistedQuerysList[index];
                _persistedQuerysList.RemoveAt(index);

                string firstLink = _searchLinkService.GenerateLinkToApi(searchLink.Link, 1, PersistedQuery);
            
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
                var links = Enumerable.Range(2, TotalPages - 1).Select(x =>
                    _searchLinkService.GenerateLinkToApi(searchLink.Link, x, PersistedQuery));
                var tasks = links.Select(GetAuctions);

                await Task.WhenAll(tasks);
            }

            stopwatch.Stop();

            if (_adLinks.Count > 0)
                _logger.LogInformation(
                    $"{DateTime.Now} - Znaleziono: {_adLinks.Count} z {TotalCount} rekordów, dla: {_searchLink.Link}, czas: {stopwatch.Elapsed}");

            if (_adLinks.Count == 0)
                _logger.LogWarning(
                    $"{DateTime.Now} - Znaleziono: {_adLinks.Count} z {TotalCount} rekordów, dla: {_searchLink.Link}, czas: {stopwatch.Elapsed}");

            return _adLinks;
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
                            Id = x.SelectToken("id")?.ToString(),
                            CategoryId = x.SelectToken("category.id")?.ToString(),
                            City = x.SelectToken("location.city.name")?.ToString(),
                            Region = x.SelectToken("location.region.name")?.ToString(),
                            Thumbnail = x.SelectToken("thumbnail.x1")?.ToString(),
                            Price = x.SelectToken("price.amount.units")?.ToString(),
                            Currency = x.SelectToken("price.amount.currencyCode")?.ToString(),
                            Make = x.SelectToken("parameters[?(@.key == 'make')].value")?.ToString(),
                            Model = x.SelectToken("parameters[?(@.key == 'model')].value")?.ToString(),
                            Gearbox = x.SelectToken("parameters[?(@.key == 'gearbox')].value")?.ToString(),
                            Year = x.SelectToken("parameters[?(@.key == 'year')].value")?.ToString(),
                            Mileage = x.SelectToken("parameters[?(@.key == 'mileage')].value")?.ToString(),
                            EngineCapacity = x.SelectToken("parameters[?(@.key == 'engine_capacity')].value")?.ToString(),
                            FuelType = x.SelectToken("parameters[?(@.key == 'fuel_type')].value")?.ToString()
                        }).ToList();
                        
                        // Check is correct ads
                        parsedAds = parsedAds.Where(x =>
                            long.TryParse(x.Id, out long parsedId) && parsedId > 0 &&
                            double.TryParse(x.Price, out double parsedPrice) && parsedPrice > 0 &&
                            !string.IsNullOrEmpty(x.Currency)).ToList();
                        
                        if (adsFromJson.Count() != parsedAds.Count())
                            return false;

                        if (TotalPages == 0)
                            GetAmountPages(json);

                        var adsToAdd = parsedAds.DistinctBy(x => x.Id).Select(x => 
                            new AdLink()
                            {
                                Id = long.Parse(x.Id),
                                CategoryId = int.TryParse(x.CategoryId, out int categoryId) ? categoryId : 0,
                                City = x.City,
                                Region = x.Region,
                                Thumbnail = x.Thumbnail,
                                Make = x.Make,
                                Model = x.Model,
                                Gearbox = x.Gearbox,
                                Year = int.TryParse(x.Year, out int year) ? year : 0,
                                Mileage = int.TryParse(x.Mileage, out int mileage) ? mileage : 0,
                                EngineCapacity = int.TryParse(x.EngineCapacity, out int engineCapacity) ? engineCapacity : 0,
                                FuelType = x.FuelType,
                                Prices = new List<AdPrice>()
                                {
                                    new AdPrice()
                                    {
                                        Price = double.Parse(x.Price),
                                        Currency = x.Currency
                                    }
                                }
                            }).ToList();

                        _adLinks.AddRange(adsToAdd);
                        return true;
                    }
                    
                    // Price = double.Parse(x.Price)
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
