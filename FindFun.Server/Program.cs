using FindFun.Server.Features.Parks;
using FindFun.Server.Features.Parks.Create;
using FindFun.Server.Features.Parks.Get;
using FindFun.Server.Infrastructure;
using FindFun.Server.Shared.File;
using FindFun.Server.Shared.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<FileUpLoad>()
    .AddScoped<CreateParkHandler>()
    .AddScoped<GetParkHandler>()
    .AddScoped<GetParksHandler>();

builder.Services.AddSingleton<GlobalExceptionHandler>();
builder.AddServiceDefaults()
    .AddConnectionStrings()
    .AddDatabase()
    .AddBlobStorage();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddValidation()
    .AddProblemDetails().
    AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.UseDefaultFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapParks();
app.MapFallbackToFile("/index.html");
await app.InitializeDbAsync();

app.Run();
