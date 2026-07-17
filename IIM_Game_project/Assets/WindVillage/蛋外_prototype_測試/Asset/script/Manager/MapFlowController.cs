using TMPro;
using UnityEngine;

public class MapFlowController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject locationPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject CharactorLayer;

    [Header("Location UI")]
    [SerializeField] private TMP_Text locationTitleText;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Locations")]
    [SerializeField] private LocationData[] locations;

    private LocationData selectedLocation;

    private void Start()
    {
        mapPanel.SetActive(true);
        locationPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        CharactorLayer.SetActive(false);
    }

    public void SelectLocation(string locationId)
    {
        selectedLocation = FindLocation(locationId);

        if (selectedLocation == null)
        {
            Debug.LogWarning("找不到地點: " + locationId);
            return;
        }

        locationTitleText.text = "要前往 " + selectedLocation.displayName + " 嗎";
        locationPanel.SetActive(true);
    }

    public void CloseLocationPanel()
    {
        locationPanel.SetActive(false);
        selectedLocation = null;
    }

    public void EnterSelectedLocation()
    {
        if (selectedLocation == null)
        {
            Debug.LogWarning("尚未選擇地點");
            return;
        }

        locationPanel.SetActive(false);
        mapPanel.SetActive(false);

        if (dialogueManager != null && selectedLocation.storyData != null)
        {
            dialoguePanel.SetActive(true);
            CharactorLayer.SetActive(true);
            dialogueManager.StartStory(selectedLocation.storyData);
        }
        else
        {
            Debug.LogWarning("DialogueManager 或 StoryData 沒有設定");
        }
    }

    private LocationData FindLocation(string locationId)
    {
        foreach (LocationData loc in locations)
        {
            if (loc.locationId == locationId)
                return loc;
        }

        return null;
    }

    public void BackToMap()
    {
        if (locationPanel != null)
            locationPanel.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (CharactorLayer != null)
            CharactorLayer.SetActive(false);
        
        if (mapPanel != null)
            mapPanel.SetActive(true);
    }
}