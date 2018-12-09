﻿using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class ValuesController : Controller
    {
        //Set as static readonly field to avoid TCP resource exhaustion.
        //See https://docs.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/
        private static readonly HttpClient client;

        static ValuesController()
        {
            client = new HttpClient();
        }

        // GET: Values
        public async Task<ActionResult> Index()
        {
            var authContext = new AuthenticationContext(AuthHelper.Authority);
            var clientCredential = new ClientCredential(AuthHelper.ClientId, AuthHelper.GetKey());

            var result = await authContext.AcquireTokenAsync(AuthHelper.CustomServiceResourceId, clientCredential);

            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            var response = await client.GetAsync(AuthHelper.CustomServiceBaseAddress + "/api/Values");
            var values = await response.Content.ReadAsStringAsync();
            ViewBag.Values = values;
            return View();
        }
    }
}