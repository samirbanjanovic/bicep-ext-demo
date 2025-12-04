using Microsoft.AspNetCore.Builder;
using Bicep.Local.Extension.Host.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Bicep.Local.Extension.Types.Attributes;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Host.Handlers;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder();

builder.AddBicepExtensionHost(args);
var ext = builder.Services
                 .AddBicepExtension(
                    name: "ExtensionDemo",
                    version: "1.0.0",
                    isSingleton: true,
                    typeAssembly: typeof(Program).Assembly);

ext.WithResourceHandler<EchoHandler>();

var app = builder.Build();
app.MapBicepExtension().Run();


public class EchoHandler
    : GenericResourceHandler
{
    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => Task.FromResult(GetResponse(request));

    protected override Task<ResourceResponse> Delete(ReferenceRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ResourceResponse
        {
            Type = request.Type,
            ApiVersion = request.ApiVersion,
            Properties = new JsonObject(),
            Identifiers = request.Identifiers
        });
    }

    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => Task.FromResult(GetResponse(request));


    protected override JsonObject GetIdentifiers(JsonObject properties)
        => properties;
}