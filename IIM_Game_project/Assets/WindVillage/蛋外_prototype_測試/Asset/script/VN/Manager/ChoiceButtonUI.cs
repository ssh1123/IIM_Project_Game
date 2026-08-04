using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceText;
    private Button button;
    private ChoiceData currentChoice;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(ChoiceData choiceData, DialogueManager manager)
    {
        currentChoice = choiceData;
        dialogueManager = manager;
        choiceText.text = choiceData.choiceText;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickChoice);
    }

    private void OnClickChoice()
    {
        dialogueManager.SelectChoice(currentChoice);
    }
}