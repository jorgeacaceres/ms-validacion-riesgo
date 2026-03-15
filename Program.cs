using ms_validacion_riesgo;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    builder.Host.UseSerilog();
    builder.Services.AddCap(x =>
    {
        x.UsePostgreSql(builder.Configuration.GetConnectionString("DefaultConnection"));
        x.UseKafka(builder.Configuration.GetValue<string>("Kafka:BootstrapServers"));
    });
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Configuration"));
    builder.Services.AddTransient<RiskEvaluationListener>();
    var app = builder.Build();
    app.MapGet("/health", () => "MS-Riesgo is running");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fallo en inicio del microservicio");
}
finally
{
    Log.CloseAndFlush();
}