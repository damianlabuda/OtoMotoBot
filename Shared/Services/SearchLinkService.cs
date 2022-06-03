using Shared.Models.SearchLinkStructure;
using System.Text.RegularExpressions;
using System.Web;
using Shared.Interfaces;

namespace Shared.Services
{
    public class SearchLinkService : ISearchLinkService
    {
        public bool Check(string searchLink)
        {
            if (searchLink.Contains("page="))
                return false;
            
            if (!Uri.IsWellFormedUriString(searchLink, UriKind.Absolute))
                return false;

            Uri baseUri = new Uri(searchLink);

            if (baseUri.Host != "www.otomoto.pl")
                return false;

            string[] searchTerms = baseUri.Segments
                .Select(x => x.Replace("/", "")).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var linkQueries = HttpUtility.ParseQueryString(baseUri.Query);
            Filter[] filters = linkQueries.AllKeys
                .Where(x => Regex.IsMatch(x, "(?<=search\\[).*?(?=])"))
                .Select(x => new Filter() { name = Regex.Match(x, "(?<=search\\[).*?(?=])").Value, value = linkQueries[x] }).ToArray();

            if ((searchTerms.Length + filters.Length) >= 3)
                return true;

            return false;
        }
    }
}
