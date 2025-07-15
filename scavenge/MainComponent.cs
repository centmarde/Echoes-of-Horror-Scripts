using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainComponent : MonoBehaviour
{
    [Header("Scavenge Settings")]
    public KeyCode scavengeKey = KeyCode.Y;
    public float scavengeTime = 3f;
    public float interactionRange = 4f;
    public int maxScavengeUses = 3; // How many times this object can be scavenged
    
    [Header("Audio Settings")]
    public AudioClip scavengeStartSound;
    public AudioClip scavengeCompleteSound;
    public AudioClip scavengeFailSound;
    public float audioVolume = 1.0f;
    
    [Header("Visual Effects")]
    public GameObject scavengeEffect; // Optional particle effect during scavenging
    public Color highlightColor = Color.cyan;
    public bool enableOutline = true;
    
    [Header("UI Settings")]
    public GameObject scavengePromptUI;
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press \"Y\" to scavenge";
    
    [Header("Loading UI")]
    public GameObject loadingCanvasUI;
    public GameObject loadingSpinner;
    public TextMeshProUGUI loadingText;
    public Image progressBar;
    
    [Header("Item Generation")]
    [Range(0f, 1f)]
    public float itemDropChance = 0.7f; // 70% chance to find an item
    public List<ScavengeItem> possibleItems = new List<ScavengeItem>();
    
    // Private variables
    private bool playerInRange = false;
    private bool isScavenging = false;
    private GameObject currentPlayer;
    private InventorySystem playerInventory; // Reference to player's inventory
    private int currentScavengeUses = 0;
    private Renderer objectRenderer;
    private Color originalColor;
    private Material originalMaterial;
    
    [System.Serializable]
    public class ScavengeItem
    {
        [Header("Basic Item Info")]
        public string itemName;
        [TextArea(2, 4)]
        public string itemDescription = "A useful item found while scavenging.";
        public Sprite itemIcon; // Icon for inventory display
        public float dropChance = 1f; // Individual item drop chance
        public int minQuantity = 1;
        public int maxQuantity = 1;
        
        [Header("Inventory Properties")]
        public bool isStackable = true;
        public int maxStackSize = 10;
        
        [Header("Special Item Properties")]
        public bool isBattery = false;
        public float batteryAmount = 30f; // If it's a battery item
        
        public bool isHealthItem = false;
        public float healthAmount = 25f; // If it's a health item
        
        public bool isKeyItem = false;
        public string keyItemID; // Unique identifier for key items
        
        [Header("Legacy Support")]
        public GameObject itemPrefab; // Optional: for spawning physical items
    }
    
    private void Start()
    {
        SetupComponents();
        InitializeUI();
        InitializePossibleItems();
    }
    
    private void SetupComponents()
    {
        // Get renderer for visual effects
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = objectRenderer.material.color;
        }
        
        // Add collider if not present
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
    }
    
    private void InitializeUI()
    {
        if (scavengePromptUI == null)
        {
            CreateScavengePromptUI();
        }
        
        if (loadingCanvasUI == null)
        {
            CreateLoadingUI();
        }
        
        // Initially hide all UI
        if (scavengePromptUI != null) scavengePromptUI.SetActive(false);
        if (loadingCanvasUI != null) loadingCanvasUI.SetActive(false);
    }
    
    private void InitializePossibleItems()
    {
        // If no items are set up, create some default ones
        if (possibleItems.Count == 0)
        {
            // Sample Battery Item
            ScavengeItem battery = new ScavengeItem();
            battery.itemName = "Battery";
            battery.itemDescription = "A rechargeable battery that can power your flashlight.";
            battery.dropChance = 0.4f;
            battery.isBattery = true;
            battery.batteryAmount = 25f;
            battery.minQuantity = 1;
            battery.maxQuantity = 2;
            battery.isStackable = true;
            battery.maxStackSize = 5;
            possibleItems.Add(battery);
            
            // Sample Health Item
            ScavengeItem healthPack = new ScavengeItem();
            healthPack.itemName = "Health Pack";
            healthPack.itemDescription = "A medical kit that can restore your health.";
            healthPack.dropChance = 0.3f;
            healthPack.isHealthItem = true;
            healthPack.healthAmount = 20f;
            healthPack.minQuantity = 1;
            healthPack.maxQuantity = 1;
            healthPack.isStackable = true;
            healthPack.maxStackSize = 3;
            possibleItems.Add(healthPack);
            
            // Sample Key Item
            ScavengeItem keyCard = new ScavengeItem();
            keyCard.itemName = "Access Card";
            keyCard.itemDescription = "A security card that might unlock doors.";
            keyCard.dropChance = 0.1f;
            keyCard.isKeyItem = true;
            keyCard.keyItemID = "access_card_01";
            keyCard.minQuantity = 1;
            keyCard.maxQuantity = 1;
            keyCard.isStackable = false;
            keyCard.maxStackSize = 1;
            possibleItems.Add(keyCard);
            
            // Sample Rare Item
            ScavengeItem rareItem = new ScavengeItem();
            rareItem.itemName = "Emergency Flare";
            rareItem.itemDescription = "A bright flare that can illuminate dark areas.";
            rareItem.dropChance = 0.15f;
            rareItem.minQuantity = 1;
            rareItem.maxQuantity = 3;
            rareItem.isStackable = true;
            rareItem.maxStackSize = 5;
            possibleItems.Add(rareItem);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentScavengeUses < maxScavengeUses)
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
            
            // Get player's inventory system
            playerInventory = currentPlayer.GetComponent<InventorySystem>();
            if (playerInventory == null)
            {
                Debug.LogWarning("MainComponent: Player doesn't have an InventorySystem component!");
            }
            
            ShowScavengePrompt();
            HighlightObject(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
            playerInventory = null;
            HideScavengePrompt();
            HighlightObject(false);
        }
    }
    
    private void Update()
    {
        if (playerInRange && !isScavenging && currentPlayer != null && Input.GetKeyDown(scavengeKey))
        {
            if (currentScavengeUses < maxScavengeUses)
            {
                StartCoroutine(ScavengeProcess());
            }
        }
        
        // Rotate loading spinner if it exists
        if (isScavenging && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(0, 0, -360f * Time.deltaTime);
        }
    }
    
    private IEnumerator ScavengeProcess()
    {
        isScavenging = true;
        currentScavengeUses++;
        
        // Hide prompt and show loading UI
        HideScavengePrompt();
        ShowLoadingUI();
        
        // Play start sound
        if (scavengeStartSound != null)
        {
            AudioSource.PlayClipAtPoint(scavengeStartSound, transform.position, audioVolume);
        }
        
        // Start scavenge effect
        if (scavengeEffect != null)
        {
            scavengeEffect.SetActive(true);
        }
        
        // Scavenging progress
        float elapsedTime = 0f;
        while (elapsedTime < scavengeTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scavengeTime;
            
            // Update progress bar
            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }
            
            // Update loading text
            if (loadingText != null)
            {
                int dots = Mathf.FloorToInt(elapsedTime * 2) % 4;
                loadingText.text = "Scavenging" + new string('.', dots);
            }
            
            yield return null;
        }
        
        // Determine if item is found
        bool itemFound = Random.Range(0f, 1f) <= itemDropChance;
        
        // Stop effects
        if (scavengeEffect != null)
        {
            scavengeEffect.SetActive(false);
        }
        
        HideLoadingUI();
        
        if (itemFound)
        {
            GenerateRandomItem();
            
            // Play success sound
            if (scavengeCompleteSound != null)
            {
                AudioSource.PlayClipAtPoint(scavengeCompleteSound, transform.position, audioVolume);
            }
        }
        else
        {
            ShowResultMessage("Nothing found...", Color.gray);
            
            // Play fail sound
            if (scavengeFailSound != null)
            {
                AudioSource.PlayClipAtPoint(scavengeFailSound, transform.position, audioVolume);
            }
        }
        
        // Check if this object is depleted
        if (currentScavengeUses >= maxScavengeUses)
        {
            ShowResultMessage("This area has been thoroughly searched.", Color.red);
            HighlightObject(false);
        }
        else if (playerInRange)
        {
            // Show prompt again if player is still in range
            ShowScavengePrompt();
        }
        
        isScavenging = false;
    }
    
    private void GenerateRandomItem()
    {
        if (possibleItems.Count == 0)
        {
            ShowResultMessage("Nothing found...", Color.gray);
            return;
        }
        
        // Create weighted list based on drop chances
        List<ScavengeItem> availableItems = new List<ScavengeItem>();
        foreach (var item in possibleItems)
        {
            if (Random.Range(0f, 1f) <= item.dropChance)
            {
                availableItems.Add(item);
            }
        }
        
        if (availableItems.Count == 0)
        {
            ShowResultMessage("Nothing found...", Color.gray);
            return;
        }
        
        // Select random item from available items
        ScavengeItem selectedItem = availableItems[Random.Range(0, availableItems.Count)];
        int quantity = Random.Range(selectedItem.minQuantity, selectedItem.maxQuantity + 1);
        
        // Handle different item types
        if (selectedItem.isBattery)
        {
            // Give battery power directly to flashlight
            GiveBatteryToPlayer(selectedItem.batteryAmount * quantity);
            ShowResultMessage($"Found {quantity}x {selectedItem.itemName}!", Color.yellow);
        }
        else if (selectedItem.isHealthItem)
        {
            GiveHealthToPlayer(selectedItem.healthAmount * quantity);
            ShowResultMessage($"Found {quantity}x {selectedItem.itemName}!", Color.green);
        }
        else if (selectedItem.isKeyItem)
        {
            // Add key item to inventory
            AddItemToInventory(selectedItem, 1);
            ShowResultMessage($"Found {selectedItem.itemName}!", Color.cyan);
        }
        else
        {
            // Add regular item to inventory
            bool success = AddItemToInventory(selectedItem, quantity);
            if (success)
            {
                ShowResultMessage($"Found {quantity}x {selectedItem.itemName}!", Color.white);
            }
            else
            {
                // If inventory is full, spawn as pickup instead
                if (selectedItem.itemPrefab != null)
                {
                    SpawnItemPrefab(selectedItem.itemPrefab, quantity);
                    ShowResultMessage($"Found {quantity}x {selectedItem.itemName}! (Dropped nearby)", new Color(1f, 0.5f, 0f)); // Orange color
                }
                else
                {
                    ShowResultMessage($"Found {selectedItem.itemName} but inventory is full!", Color.red);
                }
            }
        }
    }
    
    private bool AddItemToInventory(ScavengeItem scavengeItem, int quantity)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("No player inventory found!");
            return false;
        }
        
        // Create inventory item from scavenge item
        InventoryItem inventoryItem = new InventoryItem(
            scavengeItem.itemName,
            scavengeItem.itemDescription,
            scavengeItem.itemIcon,
            quantity,
            scavengeItem.isStackable,
            scavengeItem.maxStackSize
        );
        
        return playerInventory.AddItem(inventoryItem);
    }
    
    private void GiveBatteryToPlayer(float batteryAmount)
    {
        if (currentPlayer != null)
        {
            flashlight playerFlashlight = currentPlayer.GetComponentInChildren<flashlight>();
            if (playerFlashlight != null)
            {
                playerFlashlight.AddBattery(batteryAmount);
            }
        }
    }
    
    private void GiveHealthToPlayer(float healthAmount)
    {
        // Try to add as inventory item first
        ScavengeItem healthItem = new ScavengeItem();
        healthItem.itemName = "Health Restoration";
        healthItem.itemDescription = $"Restores {healthAmount} health points.";
        healthItem.isStackable = true;
        healthItem.maxStackSize = 5;
        
        bool addedToInventory = AddItemToInventory(healthItem, 1);
        
        if (addedToInventory)
        {
            Debug.Log($"Added health item to inventory: +{healthAmount} health");
        }
        else
        {
            // If inventory is full, apply health directly
            Debug.Log($"Player gained {healthAmount} health directly (inventory full)");
            // Example: currentPlayer.GetComponent<PlayerHealth>().AddHealth(healthAmount);
        }
    }
    
    private void GiveKeyItemToPlayer(string keyItemID)
    {
        // This is now handled in GenerateRandomItem() by adding to inventory
        Debug.Log($"Key item {keyItemID} should be added to inventory");
    }
    
    private void SpawnItemPrefab(GameObject prefab, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1f + Random.insideUnitSphere * 0.5f;
            Instantiate(prefab, spawnPos, Random.rotation);
        }
    }
    
    private void HighlightObject(bool highlight)
    {
        if (!enableOutline || objectRenderer == null) return;
        
        if (highlight)
        {
            objectRenderer.material.color = highlightColor;
        }
        else
        {
            objectRenderer.material.color = originalColor;
        }
    }
    
    private void CreateScavengePromptUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("ScavengeCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        scavengePromptUI = new GameObject("ScavengePrompt");
        scavengePromptUI.transform.SetParent(canvas.transform, false);
        
        promptText = scavengePromptUI.AddComponent<TextMeshProUGUI>();
        promptText.text = promptMessage;
        promptText.fontSize = 24;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontStyle = FontStyles.Bold;
        promptText.outlineColor = Color.black;
        promptText.outlineWidth = 0.3f;
        
        RectTransform rectTransform = scavengePromptUI.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, -100);
        rectTransform.sizeDelta = new Vector2(400, 50);
    }
    
    private void CreateLoadingUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Create loading canvas
        loadingCanvasUI = new GameObject("LoadingCanvas");
        loadingCanvasUI.transform.SetParent(canvas.transform, false);
        
        RectTransform canvasRect = loadingCanvasUI.AddComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;
        
        // Background
        Image background = loadingCanvasUI.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        
        // Spinner container
        GameObject spinnerContainer = new GameObject("SpinnerContainer");
        spinnerContainer.transform.SetParent(loadingCanvasUI.transform, false);
        
        RectTransform spinnerRect = spinnerContainer.AddComponent<RectTransform>();
        spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.anchoredPosition = Vector2.zero;
        spinnerRect.sizeDelta = new Vector2(100, 100);
        
        // Spinner
        loadingSpinner = new GameObject("Spinner");
        loadingSpinner.transform.SetParent(spinnerContainer.transform, false);
        
        RectTransform spinnerImageRect = loadingSpinner.AddComponent<RectTransform>();
        spinnerImageRect.anchorMin = Vector2.zero;
        spinnerImageRect.anchorMax = Vector2.one;
        spinnerImageRect.offsetMin = Vector2.zero;
        spinnerImageRect.offsetMax = Vector2.zero;
        
        Image spinnerImage = loadingSpinner.AddComponent<Image>();
        // You can assign a spinner texture here or create a simple circle
        spinnerImage.color = Color.white;
        
        // Loading text
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(loadingCanvasUI.transform, false);
        
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Scavenging...";
        loadingText.fontSize = 28;
        loadingText.color = Color.white;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, -80);
        textRect.sizeDelta = new Vector2(300, 50);
        
        // Progress bar background
        GameObject progressBG = new GameObject("ProgressBarBG");
        progressBG.transform.SetParent(loadingCanvasUI.transform, false);
        
        Image progressBGImage = progressBG.AddComponent<Image>();
        progressBGImage.color = Color.gray;
        
        RectTransform progressBGRect = progressBG.GetComponent<RectTransform>();
        progressBGRect.anchorMin = new Vector2(0.5f, 0.5f);
        progressBGRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressBGRect.anchoredPosition = new Vector2(0, -130);
        progressBGRect.sizeDelta = new Vector2(300, 20);
        
        // Progress bar fill
        GameObject progressFill = new GameObject("ProgressBarFill");
        progressFill.transform.SetParent(progressBG.transform, false);
        
        progressBar = progressFill.AddComponent<Image>();
        progressBar.color = Color.cyan;
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform progressFillRect = progressFill.GetComponent<RectTransform>();
        progressFillRect.anchorMin = Vector2.zero;
        progressFillRect.anchorMax = Vector2.one;
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
    }
    
    private void ShowScavengePrompt()
    {
        if (scavengePromptUI != null && currentScavengeUses < maxScavengeUses)
        {
            scavengePromptUI.SetActive(true);
        }
    }
    
    private void HideScavengePrompt()
    {
        if (scavengePromptUI != null)
        {
            scavengePromptUI.SetActive(false);
        }
    }
    
    private void ShowLoadingUI()
    {
        if (loadingCanvasUI != null)
        {
            loadingCanvasUI.SetActive(true);
            if (progressBar != null)
            {
                progressBar.fillAmount = 0f;
            }
        }
    }
    
    private void HideLoadingUI()
    {
        if (loadingCanvasUI != null)
        {
            loadingCanvasUI.SetActive(false);
        }
    }
    
    private void ShowResultMessage(string message, Color color)
    {
        StartCoroutine(DisplayResultMessage(message, color));
    }
    
    private IEnumerator DisplayResultMessage(string message, Color color)
    {
        if (promptText != null)
        {
            promptText.text = message;
            promptText.color = color;
            scavengePromptUI.SetActive(true);
            
            yield return new WaitForSeconds(2f);
            
            promptText.text = promptMessage;
            promptText.color = Color.white;
            
            if (!playerInRange || currentScavengeUses >= maxScavengeUses)
            {
                scavengePromptUI.SetActive(false);
            }
        }
    }
    
    private void OnDestroy()
    {
        if (scavengePromptUI != null)
        {
            Destroy(scavengePromptUI);
        }
        if (loadingCanvasUI != null)
        {
            Destroy(loadingCanvasUI);
        }
    }
}