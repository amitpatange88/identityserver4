using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Weathermvc.Models;
using Weathermvc.Services;

namespace Weathermvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITokenService tokenService,ILogger<HomeController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Weather()
        {
            var data = new List<WeatherData>();

            using (var client = new HttpClient())
            {
                var result = client
                    .GetAsync(requestUri: "https://localhost:5445/weatherforecast")
                    .Result;

                if (result.IsSuccessStatusCode)
                {
                    var model = result.Content.ReadAsStringAsync().Result;

                    data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

                    return View(data);
                }
                else
                {
                    throw new Exception(message: "401 Unauthorized. Unable to get content.");
                }
            }
        }

        public async Task<IActionResult> AWeather()
        {
            var data = new List<WeatherData>();

            using (var client = new HttpClient())
            {
                var tokenResponse = await _tokenService.GetToken(scope: "weatherapi.read");

                //Extenstion method of identity model.
                client.SetBearerToken(tokenResponse.AccessToken);

                var result = client
                    .GetAsync(requestUri: "https://localhost:5445/weatherforecast")
                    .Result;

                if (result.IsSuccessStatusCode)
                {
                    var model = result.Content.ReadAsStringAsync().Result;

                    data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

                    return View(data);
                }
                else
                {
                    throw new Exception(message: "401 Unauthorized. Unable to get content.");
                }
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
