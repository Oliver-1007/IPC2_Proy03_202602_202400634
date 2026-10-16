using SatWeb.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "http://localhost:5029/");
    c.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton<PdfService>();
builder.WebHost.UseUrls("http://localhost:5200");

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();