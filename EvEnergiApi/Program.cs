var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<EvEnergyApi.Services.CarbonIntensityService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            var message = ex is HttpRequestException
                ? "Error connecting to the external API. Please try again later."
                : "An unexpected server error occurred.";

            await context.Response.WriteAsJsonAsync(new { error = message });
        }
    });
});

app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();