// See https://aka.ms/new-console-template for more information
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnlimitSoft.Mediator;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IMediator>(p => new ServiceProviderMediator(p));
builder.Services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
builder.Services.AddScoped<IRequestHandlerPipeline<TestRequest, string>, TestRequestHandlerTimerCounter>();

builder.Services.AddHostedService<DemoHostedService>();


using var host = builder.Build();
await host.RunAsync();