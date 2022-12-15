using Newtonsoft.Json;

namespace TestIngestionConsole;

public record SendInventoryEventRequest(string CorrelationId, string FileName);

public interface IInventoryService
{
    Task<bool> SendInventoryEvent(SendInventoryEventRequest request);
}

public class InventoryService : IInventoryService
{
    private readonly HttpClient _client;

    public InventoryService(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> SendInventoryEvent(SendInventoryEventRequest request)
    {
        var requestContent = JsonConvert.SerializeObject(request);
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post,
            new Uri("https://inventoryingdev.azurewebsites.net/api/inventory"))
        {
            Content = new StringContent(requestContent)
        });

        return response.IsSuccessStatusCode;
    }
}