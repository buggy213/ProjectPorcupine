using UnityEngine;
using System.Collections;

public struct TradeItemRequest
{
    public string ItemType { get; set; }
    
    public RequestUrgency Urgency { get; set; }

    public TradeItemRequest(string itemType, RequestUrgency urgency)
    {
        ItemType = itemType;
        Urgency = urgency;
    }
}

public enum RequestUrgency
{
    NONE,
    LOW,
    MEDIUM,
    HIGH,
    URGENT
}
