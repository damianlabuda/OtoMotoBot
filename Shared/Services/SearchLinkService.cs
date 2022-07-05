using Shared.Models.SearchLinkStructure;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Shared.Interfaces;

namespace Shared.Services
{
    public class SearchLinkService : ISearchLinkService
    {
        private readonly Dictionary<string, string> _categoriesIdDict = new()
        {
            {"osobowe", "29"},
            {"motocykle-i-quady", "65"},
            {"dostawcze", "73"},
        };

        public string GenerateLinkToApi(string searchLink, int page, string persistedQuery)
        {
            if (searchLink.Contains("page="))
                return String.Empty;

            if (!Uri.IsWellFormedUriString(searchLink, UriKind.Absolute))
                return String.Empty;

            Uri baseUri = new Uri(searchLink);

            if (baseUri.Host != "www.otomoto.pl")
                return String.Empty;

            string[] searchTerms = baseUri.Segments
                .Select(x => x.Replace("/", "")).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (!searchTerms.Any())
                return String.Empty;

            if (!_categoriesIdDict.ContainsKey(searchTerms[0]))
                return String.Empty;

            var linkQueries = HttpUtility.ParseQueryString(baseUri.Query);
            Filter[] filtersFromUserLink = linkQueries.AllKeys
                .Where(x => Regex.IsMatch(x, "(?<=search\\[).*?(?=])"))
                .Select(x => new Filter()
                    {name = Regex.Match(x, "(?<=search\\[).*?(?=])").Value, value = linkQueries[x]}).ToArray();

            if (!((searchTerms.Length + filtersFromUserLink.Length) >= 3))
                return String.Empty;

            LinkExtensions extensions = new();
            LinkVariables variables = new();

            Filter[] sortDesc =
            {
                new() {name = "order", value = "created_at_first:desc"}
            };
            var filters = filtersFromUserLink.Concat(sortDesc).ToArray();

            variables.categoryId = _categoriesIdDict[searchTerms[0]];
            variables.filters = filters;
            variables.searchTerms = searchTerms;
            variables.page = page;
            extensions.persistedQuery.sha256Hash = persistedQuery;
            
            string jsonVariables = JsonConvert.SerializeObject(variables);
            string jsonExtensions = JsonConvert.SerializeObject(extensions);
            
            return $"https://www.otomoto.pl/graphql?operationName=listingScreen" +
                   $"&variables={HttpUtility.UrlEncode(jsonVariables)}" +
                   $"&extensions={HttpUtility.UrlEncode(jsonExtensions)}";
        }
    }
}