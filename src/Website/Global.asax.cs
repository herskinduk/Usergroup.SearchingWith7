using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Website
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "SearchController", // Route name
                "search/{action}/{id}", // URL with parameters
                new { controller = "Search", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );


        }

        public void FormsAuthentication_OnAuthenticate(object sender, FormsAuthenticationEventArgs args)
        {
            string frameworkVersion = this.GetFrameworkVersion();
            if (!string.IsNullOrEmpty(frameworkVersion) && frameworkVersion.StartsWith("v4.", StringComparison.InvariantCultureIgnoreCase))
            {
                args.User = Sitecore.Context.User;
            }
        }

        string GetFrameworkVersion()
        {
            try
            {
                return System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion();
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Cannot get framework version", ex, this);
                return string.Empty;
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Use LocalDB for Entity Framework by default
            Database.DefaultConnectionFactory = new SqlConnectionFactory(@"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}