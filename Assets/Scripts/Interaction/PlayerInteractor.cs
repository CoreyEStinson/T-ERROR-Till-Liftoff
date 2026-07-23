using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ToolManager toolManager;
    [SerializeField] private TMP_Text interactionPrompt;
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private Interactable focusedInteractable;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        interactAction?.action.Enable();
    }

    private void OnDisable()
    {
        interactAction?.action.Disable();
    }

    private void Start()
    {
        SetPromptVisible(false);
    }

    private void Update()
    {
        Interactable newFocusedInteractable = GetLookedAtInteractable();

        if (newFocusedInteractable != focusedInteractable)
        {
            ToolType previousTool = toolManager != null
                ? toolManager.EquippedTool
                : ToolType.None;

            focusedInteractable?.OnFocusExit();

            focusedInteractable = newFocusedInteractable;
            focusedInteractable?.OnFocusEnter(previousTool);
        }

        ToolType equippedTool = toolManager != null 
            ? toolManager.EquippedTool
            : ToolType.None;

        focusedInteractable?.OnFocusStay(equippedTool);

        UpdatePrompt();

        if (focusedInteractable == null || 
            !focusedInteractable.UsesUniversalInteraction ||
            interactAction == null || 
            !interactAction.action.WasPressedThisFrame())
        {
            return;
        }

        if (focusedInteractable.CanInteract(equippedTool))
        {
            focusedInteractable.Interact(equippedTool);
        } 
    }

    private Interactable GetLookedAtInteractable()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(
        playerCamera.transform.position,
        playerCamera.transform.forward);

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                interactionLayers,
                QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<Interactable>();
        }

        return null;
    }

    private void UpdatePrompt()
    {
        if (interactionPrompt == null)
        {
            return;
        }

        if (focusedInteractable == null)
        {
            {
                SetPromptVisible(false);
            }
            return;
        }

        ToolType equippedTool = toolManager != null 
            ? toolManager.EquippedTool
            : ToolType.None;

        string prompt = focusedInteractable.GetInteractionPrompt(equippedTool);

        if (string.IsNullOrWhiteSpace(prompt))
        {
            SetPromptVisible(false);
            return;
        }

        interactionPrompt.text = prompt;
        SetPromptVisible(true);
    }

    private void SetPromptVisible(bool isVisible)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(isVisible);
        }
    }
}