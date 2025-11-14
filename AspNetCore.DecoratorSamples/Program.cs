using AspNetCore.DecoratorSamples;
using AspNetCore.DecoratorSamples.Decorators;
using AspNetCore.DecoratorSamples.Interface;
using AspNetCore.DecoratorSamples.Service;

using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("Invoices", c =>
{
    c.Timeout = Timeout.InfiniteTimeSpan;
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
    MaxConnectionsPerServer = 8
});


// Base service via factory so you can pass baseUrl and token provider
builder.Services.AddTransient<IInvoicesClient>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Invoices");
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Erp:BaseUrl"] ?? "";
    //var tokenProvider = sp.GetRequiredService<ITokenProvider>();

    ///return new InvoicesClient(http, baseUrl, () => tokenProvider.GetToken());
    
    return new InvoicesClient(http, baseUrl, () => "place your token here");
});

// Decorators with Scrutor
builder.Services.Decorate<IInvoicesClient, LoggingInvoicesClientDecorator>();
builder.Services.Decorate<IInvoicesClient, RetryInvoicesClientDecorator>();

//Native DI without Scrutor
//builder.Services.AddHttpClient("Invoices", c =>
//{
//    c.Timeout = Timeout.InfiniteTimeSpan;
//    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
//});

//builder.Services.AddTransient<IInvoicesClient>(sp =>
//{
//    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Invoices");
//    var cfg = sp.GetRequiredService<IConfiguration>();
//    var baseUrl = cfg["Erp:BaseUrl"] ?? "";
//    var tokenProvider = sp.GetRequiredService<ITokenProvider>();

//    IInvoicesClient svc = new InvoicesClient(http, baseUrl, () => tokenProvider.GetToken());
//    svc = new LoggingInvoicesClientDecorator(svc, sp.GetRequiredService<ILogger<LoggingInvoicesClientDecorator>>());
//    svc = new RetryInvoicesClientDecorator(svc);
//    return svc;
//});


builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddHttpClient("Invoices", c => { /* defaults above */ })
    .AddHttpMessageHandler<LoggingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.Run();

