using TMPro;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [SerializeField] private TMP_Text equippedToolLabel;

    public ToolType EquippedTool { get; private set; } = ToolType.None;

    private ToolPickup equippedPickup;

    private void Start()
    {
        RefreshHud();
    }

    public void Equip(ToolPickup newPickup)
    {
        if (newPickup == null || equippedPickup == newPickup)
        {
            return;
        }

        // Put the old tool back on its original position before taking another one.
        if (equippedPickup != null)
        {
            equippedPickup.ReturnToBench();
        }

        equippedPickup = newPickup;
        EquippedTool = newPickup.Tool;
        newPickup.SetEquippedState();
        RefreshHud();
    }

    public void ReturnEquippedTool()
    {
        if (equippedPickup == null)
        {
            return;
        }

        ToolPickup toolToReturn = equippedPickup;

        equippedPickup = null;
        EquippedTool = ToolType.None;

        toolToReturn.ReturnToBench();
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (equippedToolLabel == null)
        {
            return;
        }

        equippedToolLabel.text = EquippedTool == ToolType.None
            ? "Equipped: None"
            : $"Equipped: {EquippedTool}";
    }
}