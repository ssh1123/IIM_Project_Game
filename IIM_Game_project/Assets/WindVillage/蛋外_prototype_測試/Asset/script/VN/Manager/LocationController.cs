using TMPro;
using UnityEngine;

public class LocationPanelController : MonoBehaviour
{
    [SerializeField] private GameObject locationPanel;
    [SerializeField] private TMP_Text locationTitleText;

    private string selectedLocationId;

    private void Start()
    {
        locationPanel.SetActive(false);
    }

    public void OpenLocationPanel(string locationId)
    {
        selectedLocationId = locationId;
        locationTitleText.text = "要前往 " + locationId + " 嗎？";
        locationPanel.SetActive(true);
    }

    public void CloseLocationPanel()
    {
        locationPanel.SetActive(false);
        selectedLocationId = "";
    }

    public void EnterLocation()
    {
        Debug.Log("進入地點: " + selectedLocationId);

        locationPanel.SetActive(false);

        switch (selectedLocationId)
        {
            case "School":
                Debug.Log("載入學校劇情");
                break;

            case "Station":
                Debug.Log("載入車站劇情");
                break;

            case "Park":
                Debug.Log("載入公園劇情");
                break;
        }
    }
}