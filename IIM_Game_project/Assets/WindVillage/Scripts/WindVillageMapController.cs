using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WindVillage
{
    public class WindVillageMapController : MonoBehaviour
    {
        [Header("Locations")]
        [SerializeField] private MapLocation[] locations;

        [Header("UI")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button enterButton;
        [SerializeField] private Button closeButton;

        [Header("Player Marker")]
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private float moveDuration = 0.45f;

        private MapLocation selectedLocation;
        private Coroutine moveRoutine;

        private void Awake()
        {
            if (infoPanel != null) infoPanel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(CloseInfo);
            if (enterButton != null) enterButton.onClick.AddListener(EnterSelectedLocation);
        }

        public void SelectLocation(string locationId)
        {
            selectedLocation = FindLocation(locationId);
            if (selectedLocation == null)
            {
                Debug.LogWarning($"Location not found: {locationId}");
                return;
            }

            if (titleText != null) titleText.text = selectedLocation.displayName;
            if (descriptionText != null) descriptionText.text = selectedLocation.description;
            if (infoPanel != null) infoPanel.SetActive(true);

            if (playerMarker != null && selectedLocation.target != null)
            {
                if (moveRoutine != null) StopCoroutine(moveRoutine);
                moveRoutine = StartCoroutine(MoveMarker(selectedLocation.target.anchoredPosition));
            }
        }

        public void CloseInfo()
        {
            if (infoPanel != null) infoPanel.SetActive(false);
        }

        public void EnterSelectedLocation()
        {
            if (selectedLocation == null) return;
            Debug.Log($"Enter location: {selectedLocation.displayName} ({selectedLocation.id})");
            // Future extension point:
            // SceneManager.LoadScene("YourSceneName");
            // DialogueManager.Instance.StartDialogue(selectedLocation.id);
        }

        private MapLocation FindLocation(string id)
        {
            if (locations == null) return null;
            foreach (MapLocation location in locations)
            {
                if (location != null && location.id == id) return location;
            }
            return null;
        }

        private IEnumerator MoveMarker(Vector2 destination)
        {
            Vector2 start = playerMarker.anchoredPosition;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, moveDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                playerMarker.anchoredPosition = Vector2.Lerp(start, destination, t);
                yield return null;
            }

            playerMarker.anchoredPosition = destination;
            moveRoutine = null;
        }
    }
}
