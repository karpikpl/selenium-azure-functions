using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;

namespace ApiHeadless.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "PlayWright")]
    public async Task<FileContentResult> Get()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://playwright.dev/dotnet");
        var imageBytes = await page.ScreenshotAsync();

        return new FileContentResult(imageBytes, "image/jpeg");
    }
}
