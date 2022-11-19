namespace UnlimitSoft.WebApi.EventBus.EventBus;


public class EventBusOptions
{
    public string Endpoint { get; set; } = null!;
    public string QueueOrTopic { get; set; } = null!;
    public string Queue1 { get; set; } = null!;
    public string Queue2 { get; set; } = null!;
}
