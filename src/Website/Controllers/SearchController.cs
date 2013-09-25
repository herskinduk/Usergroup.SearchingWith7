using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AnatomyOfSearch()
        {
            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                // Get an implementation of IQueryable from the index provider of the search context
                var queryable = context.GetQueryable<SearchResultItem>();

                // Result is still an IQueryable (note that IQueryable implements IEnumerable)
                var result = queryable.Where(i => i.Name == "folder");

                // Only now the query will be executed
                return View(result.ToList());
            }
        }

        public ActionResult Paths()
        {
            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>();

                // Note the chaining of Where methods. This is interpreted as an AND operation
                var result = queryable
                    .Where(i => i.Name.Contains("standard"))
                    .Where(i => i.Paths.Contains(Sitecore.ItemIDs.SystemRoot));

                return View(result.ToList());
            }
        }

        public ActionResult PagedResult(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("PagedResult", new { id = 0 }); 
            }

            var searchResultModel = new Models.SearchResultModel<SearchResultItem>();

            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>();

                // Event though we apply Paging - we still get a TotalSearchResults 
                // for the search result without the limit
                var result = queryable
                    .Where(i => (i.Name.Contains("Media") || i.Name.Contains("FDA")))
                    .Page(id.Value, 10)
                    .GetResults();

                searchResultModel.Hits = result.Hits.ToList();
                searchResultModel.TotalSearchResults = result.TotalSearchResults;
                searchResultModel.Facets = result.Facets;

                return View(searchResultModel);
            }
        }


        public ActionResult QueryAndFilter()
        {
            var searchResultModel = new Models.SearchResultModel<SearchResultItem>();

            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>();

                // No query or filter restriction for "folder" in name 
                var result = queryable
                    .Where(i => (i.Name.Contains("Media") || i.Name.Contains("FDA")))
                    .GetResults();

                // Filtered to only show items with "folder" in the name
                //var result = queryable
                //    .Where(i => (i.Name.Contains("Media") || i.Name.Contains("FDA")))
                //    .Filter(i => i.Name.Contains("Folder"))
                //    .GetResults();


                // Query restriction to only show items - This will impact ranking!
                //var result = queryable
                //    .Where(i => (i.Name.Contains("Media") || i.Name.Contains("FDA")) && i.Name.Contains("Folder"))
                //    .GetResults(); 

                searchResultModel.Hits = result.Hits.ToList();
                searchResultModel.TotalSearchResults = result.TotalSearchResults;
                searchResultModel.Facets = result.Facets;

                return View(searchResultModel);
            }
        }

        public ActionResult Facets()
        {
            var searchResultModel = new Models.SearchResultModel<SearchResultItem>();

            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>()
                    .Where(i => (i.Name.Contains("Media") || i.Name.Contains("FDA")))
                    .FacetOn(i => i.Language)
                    .FacetOn(i => i.TemplateName);

                var facets = GetFacetsFromQueryString(Request.QueryString);
                foreach (var facet in facets)
                {
                    queryable = queryable.Filter(i => i[facet.Key].Equals(facet.Value));
                }
                ViewBag.Facets = facets;


                var result = queryable.GetResults();

                searchResultModel.Hits = result.Hits.ToList();
                searchResultModel.TotalSearchResults = result.TotalSearchResults;
                searchResultModel.Facets = result.Facets;

                return View(searchResultModel);
            }
        }

        #region Facet action helper methods
        private IEnumerable<KeyValuePair<string, string>> GetFacetsFromQueryString(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            foreach (var key in nameValueCollection.AllKeys)
            {
                if (key.StartsWith("facet"))
                {
                    yield return new KeyValuePair<string, string>(key.Substring(5), nameValueCollection[key.ToString()]);
                }
            }
        }

        public static IHtmlString BuildFacetUrl(string url, IEnumerable<KeyValuePair<string, string>> facets)
        {
            foreach (var facet in facets)
            {
                url += url.Contains("?") ? "&" : "?";
                url += "facet" + facet.Key + "=" + facet.Value;
            }
            return new HtmlString(url);
        } 
        #endregion

        public ActionResult Regex(string id)
        {
            var searchResultModel = new Models.SearchResultModel<SearchResultItem>();
            ViewBag.Regex = id;

            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            if (id != null)
            {
                using (var context = index.CreateSearchContext())
                {
                    var queryable = context.GetQueryable<SearchResultItem>();

                    var result = queryable
                        .Where(i => i.Name.Matches(id))
                        .GetResults();

                    searchResultModel.Hits = result.Hits.ToList();
                    searchResultModel.TotalSearchResults = result.TotalSearchResults;
                    searchResultModel.Facets = result.Facets;

                    return View(searchResultModel);
                }
            }
            return new EmptyResult();
        }

        public ActionResult Fuzzy(string id)
        {
            var searchResultModel = new Models.SearchResultModel<SearchResultItem>();

            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            if (id != null)
            {
                using (var context = index.CreateSearchContext())
                {
                    var queryable = context.GetQueryable<SearchResultItem>();

                    var result = queryable
                        .Where(i => i.Name.Like(id, 0.5f))
                        .GetResults();

                    searchResultModel.Hits = result.Hits.ToList();
                    searchResultModel.TotalSearchResults = result.TotalSearchResults;
                    searchResultModel.Facets = result.Facets;

                    return View(searchResultModel);
                }
            }
            return new EmptyResult();
        }

        public ActionResult Dragons()
        {
            var index = ContentSearchManager.GetIndex("sitecore_master_index");

            using (var context = index.CreateSearchContext())
            {
                var queryable = context.GetQueryable<SearchResultItem>();
                Expression<Func<SearchResultItem, bool>> expression = (i => i.Name.Contains("standard"));
                Func<SearchResultItem, bool> func = (i => i.Name.Contains("standard"));

                var timer1 = new Sitecore.Diagnostics.HighResTimer();
                timer1.Start();
                var result1 = queryable
                    .Where(i => i.Name.Contains("standard"))
                    .ToList();
                timer1.Stop();

                var timer2 = new Sitecore.Diagnostics.HighResTimer();
                timer2.Start();
                var result2 = queryable
                    .Where(func)
                    .ToList();
                timer2.Stop();



                ViewBag.Timer1 = timer1.ElapsedTimeSpan.Milliseconds;
                ViewBag.Timer2 = timer2.ElapsedTimeSpan.Milliseconds;

                ViewBag.Result1 = result1;
                ViewBag.Result2 = result2;


                return View(result2);
            }
        }
    }
}
