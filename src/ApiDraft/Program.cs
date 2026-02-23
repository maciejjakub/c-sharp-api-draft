using Azure.Identity;
using Azure.Core;
using ApiDraft.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<TokenCredential, DefaultAzureCredential>();
builder.Services.AddSingleton<IConfidentialClientAccessTokenProvider, AzureProvider>();

// builder.Services.AddHttpClient("externalservice", client =>
// {
//     client.BaseAddress = new Uri("https://api.externalservice.com/");
//     client.DefaultRequestHeaders.Add("Accept", "application/json");
// });

builder.Services.AddHttpClient("externalservice", client => 
{
    client.BaseAddress = new Uri("https://api.myapp.com/");
})
.AddHttpMessageHandler(sp => 
{
    var provider = sp.GetRequiredService<IConfidentialClientAccessTokenProvider>();
    return new TokenAuthorizationHandler(provider, "api://external-service/.default");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapGet("/sync", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("externalservice");

    var response = await client.GetAsync("https://api.externalservice.com/external-service/api/devices");

    Console.WriteLine("Sync endpoint");

    var content = await response.Content.ReadAsStringAsync();
    return Results.Ok(content);
});

app.MapGet("/sync/{id}", (string id) => 
{
    Console.WriteLine("syncbyid");
    return Results.Ok($"Syncing data for item: {id}");
});

app.Run();