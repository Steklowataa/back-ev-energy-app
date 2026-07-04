var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<EvEnergyApi.Services.CarbonIntensityService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173",
            "https://front-ev-energy-app.onrender.com"
            )
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
                ? "Błąd połączenia z zewnętrznym API. Spróbuj ponownie później."
                : "Wystąpił nieoczekiwany błąd serwera.";

            await context.Response.WriteAsJsonAsync(new { error = message });
        }
    });
});


app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();