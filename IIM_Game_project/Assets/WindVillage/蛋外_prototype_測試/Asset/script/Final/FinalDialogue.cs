using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalDialogueManager : MonoBehaviour
{
    [Header("Story Data")]
    [SerializeField] private StoryData currentStory;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text bodyText;


    [Header("Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterLeftImage;
    [SerializeField] private Image characterMidImage;
    [SerializeField] private Image characterRightImage;
    [SerializeField] private Image chatboxImage;



    private readonly Dictionary<string, DialogueNode> nodeLookup =
        new Dictionary<string, DialogueNode>();

    private DialogueNode currentNode;

    private void Start()
    {
        StartStory(currentStory);
    }

    private void Update()
    {
        // 左鍵或 Space 可以閱讀下一句
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            ShowNextNode();
        }
    }

    public void StartStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogWarning("StoryDatabaseDialogueManager：沒有指定 StoryData。", this);
            return;
        }

        currentStory = story;

        BuildNodeLookup();

        if (string.IsNullOrEmpty(currentStory.startNodeId))
        {
            Debug.LogWarning("StoryData 的 startNodeId 是空的。", this);
            return;
        }

        ShowNode(currentStory.startNodeId);
    }

    private void BuildNodeLookup()
    {
        nodeLookup.Clear();

        if (currentStory.nodes == null)
        {
            Debug.LogWarning("StoryData 沒有 DialogueNode。", this);
            return;
        }

        foreach (DialogueNode node in currentStory.nodes)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId))
                continue;

            if (nodeLookup.ContainsKey(node.nodeId))
            {
                Debug.LogWarning(
                    $"StoryData 有重複的 nodeId：{node.nodeId}",
                    this
                );

                continue;
            }

            nodeLookup.Add(node.nodeId, node);
        }
    }

    private void ShowNode(string nodeId)
    {
        if (!nodeLookup.TryGetValue(nodeId, out DialogueNode node))
        {
            Debug.LogWarning($"找不到節點：{nodeId}", this);
            EndStory();
            return;
        }

        currentNode = node;

        UpdateText(currentNode);
        UpdateVisuals(currentNode);
    }

    private void UpdateText(DialogueNode node)
    {
        if (nameText != null)
        {
            nameText.text = node.speakerName;
        }

        if (bodyText != null)
        {
            bodyText.text = node.bodyText;
        }


    }

    private void ShowNextNode()
    {
        if (currentNode == null)
            return;

        if (!string.IsNullOrEmpty(currentNode.nextNodeId))
        {
            ShowNode(currentNode.nextNodeId);
        }
        else
        {
            EndStory();
        }
    }

    private void EndStory()
    {
        currentNode = null;
        //遊戲結束
       
    }

    private void UpdateVisuals(DialogueNode node)
    {
        if (chatboxImage != null)
        {
            chatboxImage.gameObject.SetActive(node.showchatboxPortrait);
        }

        SetImageSprite(backgroundImage, node.backgroundSprite);
        SetImageSprite(characterLeftImage, node.leftPortrait);
        SetImageSprite(characterMidImage, node.centerPortrait);
        SetImageSprite(characterRightImage, node.rightPortrait);
    }

    private void SetImageSprite(Image targetImage, Sprite sprite)
    {
        if (targetImage == null)
            return;

        if (sprite == null)
        {
            targetImage.enabled = false;
            return;
        }

        targetImage.sprite = sprite;
        targetImage.enabled = true;
    }
}