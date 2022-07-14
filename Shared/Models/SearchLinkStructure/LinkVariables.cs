namespace Shared.Models.SearchLinkStructure
{
    public class LinkVariables
    {
        public string categoryId { get; set; }
        public string click2BuyExperimentId { get; set; } = "CARS-30865";
        public string click2BuyExperimentVariant { get; set; } = "b";
        public Filter[] filters { get; set; }
        public bool includeClick2Buy { get; set; } = true;
        public bool includePriceEvaluation { get; set; } = true;
        public bool includeRatings { get; set; } = false;
        public bool includeSortOptions { get; set; } = true;
        public int page { get; set; }
        public string[] parameters { get; set; } = new string[] { "make", "model", "gearbox", "offer_type", "year", "mileage", "engine_capacity", "fuel_type" };
        public string[] searchTerms { get; set; }
    }

    public class Filter
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}
