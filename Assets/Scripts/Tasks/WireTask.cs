using System.Collections.Generic;
using UnityEngine;

public class WireTask : RepairTask
{
    [Header("References")]
    [SerializeField] private WireConnection[] wires;
    [SerializeField] private WireSocket[] sockets;
    [SerializeField] private Transform holdAnchor;

    private WireConnection heldWire;

    public bool HasHeldWire => heldWire != null;

    protected override string GetActivePrompt()
    {
        // Wires and sockets show thier own prompts.
        return string.Empty;
    }

    protected override void HandleFocusedInput() { }

    private void Start()
    {
        SetDefaultConnections();
    }

    private void Update()
    {
        if (heldWire != null)
        {
            heldWire.SetHeldPosition(holdAnchor);
        }
    }

    public bool CanPickUpWire(WireConnection wire, ToolType equippedTool)
    {
        return IsTaskActive
           && !IsCompleted
           && HasRequiredTool(equippedTool)
           && heldWire == null
           && wire != null
           && !wire.IsConnected;
    }

    public bool IsHoldingWire(WireConnection wire)
    {
        return heldWire == wire;    
    }

    public void TryPickUpWire(WireConnection wire, ToolType equippedTool)
    {
        if (!CanPickUpWire(wire, equippedTool))
        {
            return;
        }

        heldWire = wire;
        heldWire.SetHeldPosition(holdAnchor);
    }

    public void ReturnHeldWire()
    {
        if (heldWire == null)
        {
            return;
        }

        heldWire.SetDisconnected();
        heldWire = null;
    }

    public void TryConnectHeldWire(WireSocket socket)
    {
        if (heldWire == null || socket == null || socket.IsOccupied)
        {
            return;
        }

        // Wrong color, put the wire back
        if (heldWire.CurrentColor != socket.SocketColor)
        {
            ReturnHeldWire();
            return;
        }

        heldWire.ConnectTo(socket);
        heldWire = null;

        if (AllWiresConnected())
        {
            CompleteTask();
        }
    }

    protected override void OnTaskActivated()
    {
        ReturnHeldWire();
        ConfigureRandomIssue();
    }

    protected override void OnTaskDeactivated()
    {
        ReturnHeldWire();
    }

    private void SetDefaultConnections()
    {
        if (!HasValidSetup())
        {
            return;
        }

        ClearSockets();

        for (int i = 0; i < wires.Length; i++)
        {
            wires[i].SetWireColor(sockets[i].SocketColor);
            wires[i].ConnectTo(sockets[i]);
        }
    }

    private void ConfigureRandomIssue()
    {
        if (!HasValidSetup())
        {
            return;
        }

        ClearSockets();

        int connectedWireIndex = Random.Range(0, wires.Length);

        List<WireColor> avaliableColors = new List<WireColor>();

        for (int i = 0; i < sockets.Length; i++)
        {
            if (i != connectedWireIndex)
            {
                avaliableColors.Add(sockets[i].SocketColor);
            }
        }

        for (int i = 0; i < wires.Length; i++)
        {
            if (i == connectedWireIndex)
            {
                wires[i].SetWireColor(sockets[i].SocketColor);
                wires[i].ConnectTo(sockets[i]);
                continue;
            }

            int randomColorIndex = Random.Range(0, avaliableColors.Count);

            wires[i].SetWireColor(avaliableColors[randomColorIndex]);
            wires[i].SetDisconnected();

            avaliableColors.RemoveAt(randomColorIndex);
        }
    }

    private void ClearSockets()
    {
        foreach (WireSocket socket in sockets)
        {
            socket?.SetOccupied(false);
        } 

        foreach (WireConnection wire in wires)
        {
            wire?.SetDisconnected();
        }
    }

    private bool AllWiresConnected()
    {
        foreach (WireConnection wire in wires)
        {
            if (wire == null || !wire.IsConnected)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasValidSetup()
    {
        if (wires == null ||
            sockets == null ||
            wires.Length != 4 ||
            sockets.Length != 4)
        {
            Debug.LogWarning(
                $"{name} needs exactly four wires and four sockets.",
                this
            );

            return false;
        }

        return true;
    }
}