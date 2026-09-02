using UnityEngine;

public class PersistentControlBoard : MonoBehaviour
{
    public static PersistentControlBoard Instance { get; private set; }
    [SerializeField] private GameObject AIpanel;
    [SerializeField] private AIChatTester AItester;

    private bool isset = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (GameState.Instance.runnerIntroStarted && !isset && GameState.Instance.IsAIEnabled)
        {
            AItester.ResetPanel();
            AIpanel.SetActive(true);
            isset = true;
        }
        else if(!GameState.Instance.runnerIntroStarted)
        {
            
            AIpanel.SetActive(false);
            isset = false;
        }
    }
}