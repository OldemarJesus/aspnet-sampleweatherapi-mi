using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SampleWeatherPortal.Models;

namespace SampleWeatherPortal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        // Load weather data from SampleWeatherApi
        var client = _httpClientFactory.CreateClient("SampleWeatherApi");
        var response = client.GetAsync("api/Sample").GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to load weather data from SampleWeatherApi.");
            ViewBag.WeatherData = $"Error loading weather data. Status code: {response.StatusCode}";
            return View();
        }

        var weatherData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        ViewBag.WeatherData = weatherData;
        return View();
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
