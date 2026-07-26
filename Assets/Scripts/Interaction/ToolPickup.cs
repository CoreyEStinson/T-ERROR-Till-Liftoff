using UnityEngine;

public class ToolPickup : Interactable
{
    [SerializeField] private ToolType tool = ToolType.Wrench;
    [SerializeField] private ToolManager toolManager;

    public ToolType Tool => tool;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        return $"[E] Pick up {tool.ToDisplayName()}";
    }

    public override void Interact(ToolType equippedTool)
    {
        if (toolManager == null)
        {
            Debug.LogWarning($"{name} needs a ToolManager reference.", this);
            return;
        }

        toolManager.Equip(this);
    }

    public void SetEquippedState()
    {
        gameObject.SetActive(false);
    }

    public void ReturnToBench()
    {
        transform.SetPositionAndRotation(originalPosition, originalRotation);
        gameObject.SetActive(true);
    }
}
