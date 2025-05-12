using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

namespace LegendsViewer.Frontend;

public static class WebAppStaticServer
{
    public const uint WebAppPort = 8081;
    public static readonly string WebAppUrl = $"http://localhost:{WebAppPort}";

    public static async Task RunAsync()
    {
        var options = new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "legends-viewer-frontend", "dist")
        };
        var builder = WebApplication.CreateBuilder(options);
        var app = builder.Build();
        app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = ["index.html"] });
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");

        try
        {
            await app.RunAsync($"http://*:{WebAppPort}");
        }
        catch (IOException exception) when (exception.InnerException is AddressInUseException)
        {
            Console.WriteLine($"Address already in use: {WebAppUrl}");
            Console.WriteLine("Skipping static server.");
        }
    }

    public static async Task OpenPageInBrowserAsync()
    {
        await Task.Delay(200);
        try
        {
            Process.Start(new ProcessStartInfo(WebAppUrl) { UseShellExecute = true });
        }
        catch
        {
            Console.WriteLine($"Failed to open URL: {WebAppUrl}");
        }
    }
}
