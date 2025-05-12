using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CatchCounter : MonoBehaviour
{
    [Tooltip("Maximum number of catches before game reset")]
    public int maxCatches = 1;
    
    [Tooltip("UI Text to display the current catch count (optional)")]
    public TextMeshProUGUI countText;
    
    [Tooltip("Scene to load when max catches is reached (defaults to current scene)")]
    public string sceneToLoad = "";
    
    // Singleton instance
    public static CatchCounter Instance { get; private set; }
    
    // Current catch count
    private int catchCount = 0;
    
    // Property to access the current catch count
    public int CatchCount 
    { 
        get { return catchCount; } 
        private set 
        { 
            catchCount = value;
            UpdateUI();
        }
    }
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize counter
            CatchCount = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // If no scene is specified, use current scene
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = SceneManager.GetActiveScene().name;
        }
        
        UpdateUI();
        
        // Add initial debug log
        Debug.Log($"CatchCounter initialized. Current count: {CatchCount}/{maxCatches}");
    }
    
    // Call this method when player is caught
    public void IncrementCatchCount()
    {
        CatchCount++;
        Debug.Log($"Player caught! Catch count: {CatchCount}/{maxCatches}");
        
        // Check if player has reached max catches
        if (CatchCount >= maxCatches)
        {
            Debug.Log("Maximum catches reached. Resetting game...");
            ResetGame();
        }
    }
    
    // Reset the catch counter without resetting the game
    public void ResetCounter()
    {
        int previousCount = CatchCount;
        CatchCount = 0;
        Debug.Log($"Catch counter manually reset from {previousCount} to 0");
    }
    
    // Reset the game by reloading the scene
    public void ResetGame()
    {
        // Reset counter first
        Debug.Log($"Game reset triggered at catch count: {CatchCount}");
        CatchCount = 0;
        
        // Reload the scene
       /*  SceneManager.LoadScene(sceneToLoad); */

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
    
    // Update the UI text if available
    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = $"Catches: {CatchCount}/{maxCatches}";
            Debug.Log($"UI updated. Displaying catch count: {CatchCount}/{maxCatches}");
        }
    }

    // Log the current count when enabled/disabled for debugging
    private void OnEnable()
    {
        Debug.Log($"CatchCounter enabled. Current count: {CatchCount}/{maxCatches}");
    }
}
