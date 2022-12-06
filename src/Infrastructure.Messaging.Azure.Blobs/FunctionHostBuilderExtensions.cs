﻿using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;

namespace Infrastructure.Messaging.Azure.Queues;

public static class FunctionHostBuilderExtensions
{
    public static bool IsNonProd(this IFunctionsHostBuilder builder)
    {
        var environmentName = builder.GetContext().EnvironmentName;
        return !(
            string.Equals("staging", environmentName, StringComparison.OrdinalIgnoreCase)
            || string.Equals("production", environmentName, StringComparison.OrdinalIgnoreCase)
        );
    }
    
    public static void RegisterBlobServiceClient(
        this IFunctionsHostBuilder builder,
        string account,
        string name
    )
    {
        builder.Services.AddAzureClients(clientBuilder =>
        {
            var queueBuilder = clientBuilder.AddBlobServiceClient(account).WithName(name);

            if (!builder.IsNonProd())
            {
                queueBuilder.WithCredential(new ManagedIdentityCredential());
            }
        });
    }
}