using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtoMoto.Scraper.Entities;
using OtoMoto.Scraper.Entities.LinkStructure;
using OtoMoto.Scraper.HttpClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OtoMoto.Scraper
{
    public class OtomotoSearchAuctions
    {
        private readonly SearchLink _searchLink;

        private readonly IOtoMotoHttpClient _otoMotoHttpClient;

        private readonly List<AdLink> _adLinks = new List<AdLink>();

        private readonly LinkExtensions _extensions = new LinkExtensions();

        private readonly LinkVariables _variables = new LinkVariables();

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10, 10);
        private int _totalPages { get; set; } = 0;
        private int _totalCount { get; set; } = 0;

        public OtomotoSearchAuctions(SearchLink searchLink, IOtoMotoHttpClient otoMotoHttpClient)
        {
            _searchLink = searchLink;
            _otoMotoHttpClient = otoMotoHttpClient;
        }
        public async Task<List<AdLink>> Search()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string firstLink = GenerateLinkToApi(1);

            if (!string.IsNullOrEmpty(firstLink))
            {
                await GetAuctions(firstLink);

                if (_totalPages > 1)
                {
                    var links = Enumerable.Range(2, _totalPages - 1).Select(GenerateLinkToApi);
                    var tasks = links.Select(GetAuctions);

                    await Task.WhenAll(tasks);
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Znaleziono: {_adLinks.Count} z {_totalCount} rekordów, dla: {_searchLink.Link}, czas: {stopwatch.Elapsed}");

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
                Filter[] filters = linkQueries.AllKeys
                    .Where(x => Regex.IsMatch(x, "(?<=search\\[).*?(?=])"))
                    .Select(x => new Filter() { name = Regex.Match(x, "(?<=search\\[).*?(?=])").Value, value = linkQueries[x] }).ToArray();

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
            catch(Exception) { }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<string> GetHttpRequest(string link)
        {
            try
            {
                var result = await _otoMotoHttpClient.Get(link);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return string.Empty;
                }

                return result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
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
                    IEnumerable<JToken> pricyProducts = json.SelectTokens("data.advertSearch.edges[*].node");

                    if (pricyProducts.Any())
                    {
                        var a = pricyProducts.Select(x => new { Link = x.SelectToken("url").ToString(), Price = x.SelectToken("price.amount.units").ToString() });

                        a = a.Where(x =>
                            !string.IsNullOrEmpty(x.Link) && double.TryParse(x.Price, out double parsedPrice) && parsedPrice > 0);

                        if (pricyProducts.Count() != a.Count())
                        {
                            return false;
                        }

                        if (_totalPages == 0)
                        {
                            GetAmountPages(json);
                        }

                        var b = a.Distinct().Select(x => new AdLink()
                            {Link = x.Link, Price = double.Parse(x.Price), SearchLinkId = _searchLink.Id}).Where(x => x.Link != null);

                        _adLinks.AddRange(b);
                        return true;
                    }
                }
                catch (Exception)
                {
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

            if ((int)totalCount > _totalCount)
            {
                _totalCount = (int)totalCount;
            }

            if (!double.TryParse(json.SelectToken("data.advertSearch.pageInfo.pageSize").ToString(), out double pageSize) && pageSize > 0)
            {
                return;
            }

            int totalPages = (int)Math.Ceiling(totalCount / pageSize);

            if (totalPages > _totalPages)
            {
                _totalPages = totalPages;
            }
        }
    }
}