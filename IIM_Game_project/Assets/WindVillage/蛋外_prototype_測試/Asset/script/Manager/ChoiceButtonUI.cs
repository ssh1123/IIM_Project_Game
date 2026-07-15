using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceText;
    private Button button;
    private string nextNodeId;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(ChoiceData choiceData, DialogueManager manager)
    {
        dialogueManager = manager;
        nextNodeId = choiceData.nextNodeId;
        choiceText.text = choiceData.choiceText;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickChoice);
    }

    private void OnClickChoice()
    {
        dialogueManager.SelectChoice(nextNodeId);
    }
}