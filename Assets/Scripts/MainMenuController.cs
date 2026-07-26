using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Starts the game from the main menu when the player presses E.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplayScene = "SampleScene";

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(gameplayScene);
        }
    }
}
