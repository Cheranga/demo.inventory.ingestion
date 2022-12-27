namespace Demo.Inventory.Ingestion.Domain;

public interface ITrackable
{
    public string CorrelationId { get; set; }
}