using UnityEngine;

public enum WireColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public static class WireColorUtility
{
    public static Color toColor(WireColor wireColor)
    {
        return wireColor switch
        {
            WireColor.Red => Color.red,
            WireColor.Blue => Color.blue,
            WireColor.Green => Color.green,
            WireColor.Yellow => Color.yellow,
            _ => Color.white
        };
    }
}

public class WireConnection : Interactable
{
    [Header("Visual References")]
    [SerializeField] private Transform plugVisual;
    [SerializeField] private Transform looseEndResetPoint;
    [SerializeField] private LineRenderer wireLine;
    [SerializeField] private Renderer plugRenderer;

    private WireTask wireTask;
    private WireSocket connectedSocket;

    public WireColor CurrentColor { get; private set; }
    public bool IsConnected { get; private set; }

    private void Awake()
    {
        wireTask = GetComponentInParent<WireTask>();

        if (plugVisual == null)
        {
            plugVisual = transform;
        }
    }

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (wireTask != null && !wireTask.HasRequiredTool(equippedTool))
        {
            return $"Requires {wireTask.RequiredToolDisplayName}";
        }

        if (wireTask == null || !wireTask.IsTaskActive || IsConnected)
        {
            return string.Empty;
        }

        if (wireTask.IsHoldingWire(this))
        {
            return $"[E] Put down {CurrentColor} wire";
        }

        if (wireTask.CanPickUpWire(this, equippedTool))
        {
            return $"[E] Pick up {CurrentColor} wire";
        }

        return string.Empty;
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return wireTask != null && 
            (wireTask.CanPickUpWire(this, equippedTool) || wireTask.IsHoldingWire(this));
    }

    public override void Interact(ToolType equippedTool)
    {
        if (wireTask.IsHoldingWire(this))
        {
            wireTask.ReturnHeldWire();
        }
        else
        {
            wireTask.TryPickUpWire(this, equippedTool);
        }
    }

    public void SetWireColor(WireColor wireColor)
    {
        CurrentColor = wireColor;

        Color displayColor = WireColorUtility.toColor(wireColor);

        if (wireLine != null)
        {
            wireLine.startColor = displayColor;
            wireLine.endColor = displayColor;

            ApplyMaterialColor(wireLine.material, displayColor);
        }

        if (plugRenderer != null)
        {
            ApplyMaterialColor(plugRenderer.material, displayColor);
        }
    }

    public void SetHeldPosition(Transform holdAnchor)
    {
        if (holdAnchor == null || plugVisual == null)
        {
            return;
        }

        plugVisual.SetPositionAndRotation(
            holdAnchor.position,
            holdAnchor.rotation
        );

        UpdateLine();
    }

    public void SetDisconnected()
    {
        IsConnected = false;

        if (connectedSocket != null)
        {
            connectedSocket.SetOccupied(false);
            connectedSocket = null;
        }

        if (looseEndResetPoint != null && plugVisual != null)
        {
            plugVisual.SetPositionAndRotation(
                looseEndResetPoint.position,
                looseEndResetPoint.rotation
            );
        }

        UpdateLine();
    }

    public void ConnectTo(WireSocket socket)
    {
        IsConnected = true;

        connectedSocket = socket;
        socket.SetOccupied(true);

        if (plugVisual != null && socket.PlugPoint != null)
        {
            plugVisual.SetPositionAndRotation(
                socket.PlugPoint.position,
                socket.PlugPoint.rotation
            );
        }

        UpdateLine();
    }

    private void UpdateLine()
    {
        if (wireLine == null || plugVisual == null)
        {
            return;
        }

        wireLine.positionCount = 2;
        wireLine.SetPosition(0, transform.position);
        wireLine.SetPosition(1, plugVisual.position);
    }

    private static void ApplyMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
}