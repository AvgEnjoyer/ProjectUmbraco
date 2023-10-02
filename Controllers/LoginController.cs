using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ProjectUmbraco.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using StackExchange.Profiling.Internal;

namespace ProjectUmbraco.Controllers
{
    public class LoginController : SurfaceController
    {
        public LoginController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
        }
        private string ParseJson(string jsonResponse)
        {

            // Parse the outer JSON
            var outerObject = JsonConvert.DeserializeObject<JObject>(jsonResponse);

            // Extract the inner JSON string
            string innerJsonString = outerObject["return"].ToString();

            // Parse the inner JSON
            return JsonConvert.DeserializeObject<JObject>(innerJsonString).ToString();


        }
        [HttpPost]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> SubmitAsync(LoginViewModel model)
        {
            // Retrieve login and password from the form model
            string login = model.Username;
            string password = model.Password;

            // Create a proxy to the SOAP web service
            var client = new ServiceReference1.ICUTechClient(); // Replace with your service client name
            var contentService = Services.ConsentService;
            try
            {
                // Call the SOAP web service's Login method
                var response = await client.LoginAsync(login, password, "");
                var sR = response.ToJson();
                if (sR.Contains("-1"))
                {
                    ViewBag.ErrorMessage = "Login failed. Please check your credentials.";
                }
                else
                {
                    ViewBag.SuccessMessage = ParseJson(sR);
                }
                client.Close();
            }
            catch (Exception ex)
            {
                client.Abort();
                ViewBag.ErrorMessage = "An error occurred while processing your request.";
            }

            return CurrentUmbracoPage();
        }

    }
}
