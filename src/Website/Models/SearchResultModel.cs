using Sitecore.ContentSearch.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class SearchResultModel<T>
    {
        public IEnumerable<SearchHit<T>> Hits { get; set; }
        public FacetResults Facets { get; set; }
        public int TotalSearchResults { get; set; }
    }
}