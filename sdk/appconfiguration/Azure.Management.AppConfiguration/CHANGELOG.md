# Release History

## 1.0.0-preview.1

This package follows the [Azure SDK Design Guidelines for .NET](https://azure.github.io/azure-sdk/dotnet_introduction.html) which provide a number of core capabilities that are shared amongst all Azure SDKs, including the intuitive Azure Identity library, an HTTP Pipeline with custom policies, error-handling, distributed tracing, and much more.

This is a Public Preview version, so expect incompatible changes in subsequent releases as we improve the product. To provide feedback, please submit an issue in our [Azure SDK for .NET GitHub repo](https://github.com/Azure/azure-sdk-for-net/issues).

### General New Features

    - Support MSAL.NET, Azure.Identity is out of box for supporting MSAL.NET
    - Support [OpenTelemetry](https://opentelemetry.io/) for distributed tracing
    - HTTP pipeline with custom policies
    - Support uniform telemetry across all languages

> NOTE: For more information about unified authentication, please refer to [Azure Identity documentation for .NET](https://docs.microsoft.com//dotnet/api/overview/azure/identity-readme?view=azure-dotnet)

### Samples

The package name is `Azure.Management.AppConfiguration`

Example: Create an configuration store

```csharp
using Azure.Identity;
using Azure.Management.AppConfiguration;
using Azure.Management.AppConfiguration.Models;

var appConfigurationManagementClient = new AppConfigurationManagementClient(subscriptionId, new DefaultAzureCredential());
var configurationStoresClient = eventHubsManagementClient.GetConfigurationStoresClient();

var configurationCreateResult = await configurationStoresClient.StartCreateAsync(
    resourceGroup,
    storeName,
    new ConfigurationStore("westus", new Sku("Standard")));
await configurationCreateResult.WaitForCompletionAsync();
```
