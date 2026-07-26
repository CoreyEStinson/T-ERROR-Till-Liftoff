using UnityEngine;

public class ToolReturnStation : Interactable
{
    [SerializeField] private ToolManager toolManager;

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        return equippedTool == ToolType.None
            ? "Tool bench"
            : $"[E] Put down {equippedTool.ToDisplayName()}";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return equippedTool != ToolType.None;
    }

    public override void Interact(ToolType equippedTool)
    {
        if (toolManager != null)
        {
            toolManager.ReturnEquippedTool();
        }
    }
}
