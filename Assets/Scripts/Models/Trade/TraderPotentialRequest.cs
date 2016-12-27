using System;
using UnityEngine;

public struct TraderPotentialRequest
{
    private float chanceToFulfill;

    public string Request { get; set; }

    public string Category { get; set; }

    public float ChanceToFulfill
    {
        get
        {
            return chanceToFulfill;
        }
        set
        {
            chanceToFulfill = Mathf.Clamp(value, 0f, 1f);
        }
    }
}
