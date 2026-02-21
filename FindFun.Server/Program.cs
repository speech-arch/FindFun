using FindFun.Server.Features.Parks;
using FindFun.Server.Features.Parks.Create;
using FindFun.Server.Infrastructure;
using FindFun.Server.Shared.File;
using FindFun.Server.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<FileUpLoad>();

builder.Services.AddScoped<CreateParkHandler>();
builder.Services.AddSingleton<GlobalExceptionHandler>();
builder.AddServiceDefaults()
    .AddConnectionStrings()
    .AddDatabase()
    .AddBlobStorage();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddValidation()
    .AddProblemDetails().
    AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapParks();
app.MapFallbackToFile("/index.html");
await app.InitializeDbAsync();

app.Run();
