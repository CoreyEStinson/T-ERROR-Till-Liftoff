using UnityEngine;

public class WireSocket : Interactable
{
    [SerializeField] private WireColor socketColor;
    [SerializeField] private Transform plugPoint;

    private WireTask wireTask;

    public WireColor SocketColor => socketColor;
    public Transform PlugPoint => plugPoint;
    public bool IsOccupied { get; private set; }

    private void Awake()
    {
        wireTask = GetComponentInParent<WireTask>();

        Material material = GetComponent<Renderer>().material;
        ApplyMaterialColor(material, WireColorUtility.toColor(socketColor));
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

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (wireTask == null ||
            !wireTask.IsTaskActive ||
            IsOccupied ||
            !wireTask.HasHeldWire)
        {
            return string.Empty;
        }

        return $"[E] Connect to {socketColor} socket";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return wireTask != null &&
            wireTask.IsTaskActive &&
            !IsOccupied &&
            wireTask.HasHeldWire;
    }

    public override void Interact(ToolType equippedTool)
    {
        wireTask?.TryConnectHeldWire(this);
    }

    public void SetOccupied(bool isOccupied)
    {
        IsOccupied = isOccupied;
    }
}