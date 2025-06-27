using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "Scenes/Menu";
    [SerializeField] private string gameSceneName = "Scenes/Scene1";

    [Header("UI References")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject quitButton;

    private void Start()
    {
        // Ensure cursor is visible and unlocked in menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Called when the Start button is clicked
    /// Loads the main game scene (Scene1)
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("Starting game...");
        LoadScene(gameSceneName);
    }

    /// <summary>
    /// Called when the Quit button is clicked
    /// Quits the application
    /// </summary>
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quitting game...");
        QuitGame();
    }

    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty!");
            return;
        }

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{sceneName}': {e.Message}");
        }
    }

    /// <summary>
    /// Returns to the main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        LoadScene(menuSceneName);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    private void QuitGame()
    {
#if UNITY_EDITOR
        // If running in the Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a built application
        Application.Quit();
#endif
    }

    /// <summary>
    /// Alternative method to load the game scene
    /// Can be called from other scripts if needed
    /// </summary>
    public void StartGame()
    {
        OnStartButtonClicked();
    }

    /// <summary>
    /// Method to reload the current scene
    /// Useful for restart functionality
    /// </summary>
    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LoadScene(currentScene.name);
    }
}