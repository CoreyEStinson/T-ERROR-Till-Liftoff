using UnityEngine;

public enum ToolType
{
    None, 
    Wrench,
    WireCutters,
    Hammer
}

public abstract class Interactable : MonoBehaviour
{
    public virtual bool UsesUniversalInteraction => true;

    // Text shown while the player is looking at this object
    public abstract string GetInteractionPrompt(ToolType equippedTool);

    // Allows an object to show a prompt but refuse interaction.
    // such as a machine that needs a missing tool
    public virtual bool CanInteract(ToolType equippedTool)
    {
        return true;
    }

    // Called when the player presses E while looking at this object
    public abstract void Interact(ToolType equippedTool); 

    // Used by physical tasks 
    public virtual void OnFocusEnter(ToolType equippedTool) { }

    public virtual void OnFocusStay(ToolType equippedTool) { }

    public virtual void OnFocusExit() { }
}
