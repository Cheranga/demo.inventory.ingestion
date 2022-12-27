using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

public static class HttpExtensions
{
    private static readonly JsonSerializerSettings SerializerSettings =
        new() { Error = (_, args) => args.ErrorContext.Handled = true };

    public static async Task<TModel> ToModelAsync<TModel>(this HttpRequest request)
    {
        // TODO: return better type
        var content = await request.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content)) return default;

        return JsonConvert.DeserializeObject<TModel>(content, SerializerSettings);
    }
}