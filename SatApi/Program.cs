using SatApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<RepositorioDatos>();
builder.Services.AddSingleton<ServicioRtu>();
builder.Services.AddSingleton<ServicioAutorizaciones>();
builder.Services.AddSingleton<ServicioConsultas>();
builder.WebHost.UseUrls("http://localhost:5029");

var app = builder.Build();
app.MapControllers();
app.Run();
