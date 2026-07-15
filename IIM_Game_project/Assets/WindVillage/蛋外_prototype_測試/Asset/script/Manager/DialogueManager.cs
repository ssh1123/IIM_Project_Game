using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Story Data")]
    [SerializeField] private StoryData currentStory;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject choiceGroup;
    [SerializeField] private ChoiceButtonUI[] choiceButtons;

    private Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();
    private DialogueNode currentNode;

    private void Start()
    {
        StartStory(currentStory);
    }

    public void StartStory(StoryData storyData)
    {
        currentStory = storyData;
        BuildNodeLookup();
        ShowNode(currentStory.startNodeId);
    }

    private void BuildNodeLookup()
    {
        nodeLookup.Clear();

        foreach (DialogueNode node in currentStory.nodes)
        {
            if (!string.IsNullOrEmpty(node.nodeId) && !nodeLookup.ContainsKey(node.nodeId))
            {
                nodeLookup.Add(node.nodeId, node);
            }
        }
    }

    public void ShowNode(string nodeId)
    {
        if (!nodeLookup.ContainsKey(nodeId))
        {
            Debug.LogWarning("找不到節點: " + nodeId);
            return;
        }

        currentNode = nodeLookup[nodeId];

        nameText.text = currentNode.speakerName;
        bodyText.text = currentNode.bodyText;

        UpdateChoices();
    }

    private void UpdateChoices()
    {
        bool hasChoices = currentNode.choices != null && currentNode.choices.Count > 0;

        choiceGroup.SetActive(hasChoices);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (hasChoices && i < currentNode.choices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].Setup(currentNode.choices[i], this);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnClickNext()
    {
        if (currentNode == null) return;

        bool hasChoices = currentNode.choices != null && currentNode.choices.Count > 0;
        if (hasChoices) return;

        if (!string.IsNullOrEmpty(currentNode.nextNodeId))
        {
            ShowNode(currentNode.nextNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    public void SelectChoice(string nextNodeId)
    {
        ShowNode(nextNodeId);
    }

    private void EndDialogue()
    {
        nameText.text = "";
        bodyText.text = "故事結束";
        choiceGroup.SetActive(false);
    }
}