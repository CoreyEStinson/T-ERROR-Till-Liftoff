using UnityEngine;

public class BoltPoint : Interactable
{
    [Header("Visual References")]
    [SerializeField] private Transform boltHead;
    [SerializeField] private GameObject looseIndicator;

    [Header("Rotation")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
    [SerializeField] private float totalTightneningRotation = 360f;

    private BoltTask boltTask;
    private Quaternion startingLocalRotation;

    public bool IsLoose { get; private set; }
    public float Progress { get; private set; }

    private void Awake()
    {
        boltTask = GetComponentInParent<BoltTask>();

        if (boltHead == null)
        {
            boltHead = transform;
        }

        startingLocalRotation = boltHead.localRotation;
        SetLoose(false);
    }

    public override bool UsesUniversalInteraction => false;

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (boltTask == null || !IsLoose)
        {
            return string.Empty;
        }

        if (!boltTask.CanWorkOnBolts(equippedTool))
        {
            return "Requires Wrench";
        }

        if (boltTask.IsTightening(this))
        {
            return $"Hold LMB: Tighten bolt ({Progress * 100f:0}%)";
        }

        return "Hold LMB: Tighten bolt";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return false;
    }

    public override void Interact(ToolType equippedTool) { }

    public override void OnFocusStay(ToolType equippedTool)
    {
        boltTask?.HandleBoltInput(this, equippedTool);
    }

    public override void OnFocusExit()
    {
        boltTask?.HandleBoltFocusLost(this);
    }

    public void SetLoose(bool isLoose)
    {
        IsLoose = isLoose;
        Progress = 0f;

        boltHead.localRotation = startingLocalRotation;

        if (looseIndicator != null)
        {
            looseIndicator.SetActive(isLoose);
        }
    }

    public void SetProgress(float progress, int turnDirection)
    {
        Progress = Mathf.Clamp01(progress);

        boltHead.localRotation = 
            startingLocalRotation * 
            Quaternion.AngleAxis(
                Progress * totalTightneningRotation * turnDirection,
                localRotationAxis
            );
    }

    public void SetTightened()
    {
        IsLoose = false;
        Progress = 1f;

        if (looseIndicator != null)
        {
            looseIndicator.SetActive(false);
        }
    }

}
