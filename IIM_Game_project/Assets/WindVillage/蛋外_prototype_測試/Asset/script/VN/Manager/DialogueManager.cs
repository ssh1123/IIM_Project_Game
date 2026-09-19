using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("Story Data")]
    [SerializeField] private StoryData currentStory;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private GameObject choiceGroup;
    [SerializeField] private ChoiceButtonUI[] choiceButtons;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;


    [Header("Systems")]
    private Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();
    private DialogueNode currentNode;

    [Header("Learning Test Recording")]
    //[SerializeField] private LearningTestResultUploader testResultUploader;
    private LearningTestResultUploader testResultUploader;
    // 由 MapFlowController 根據玩家進入的地點設定。
    // 需為 group_1、group_2、group_3、group_4。
    private string currentQuestionGroup = "";

    // 顯示「有選項的題目」時更新
    private DateTimeOffset currentQuestionStartedAt;

    // 當前題組內的題號
    private int groupQuestionOrder = 0;

    // 四組合計的題號
    private int overallQuestionOrder = 0;

    [Header("Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterLeftImage;
    [SerializeField] private Image characterMidImage;
    [SerializeField] private Image characterRightImage;
    [SerializeField] private Image ChatboxImage;

    [Header("Controls")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private MapFlowController mapFlowController;


    private void Start()
    {
        Debug.Log("currentStory: " + currentStory);
        Debug.Log("nameText: " + nameText);
        Debug.Log("bodyText: " + bodyText);
        Debug.Log("choiceGroup: " + choiceGroup);
        Debug.Log("GameState.Instance: " + GameState.Instance);
        feedbackPanel.SetActive(false);
        StartStory(currentStory);
    }
    /*private void Update()
    {
        // 左鍵或 Space 可以閱讀下一句
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            ShowNextNode();
        }
    }*/
    private void Update()
    {
        if (dialogueRoot == null || !dialogueRoot.activeInHierarchy)
            return;

        bool isActive = feedbackPanel.activeInHierarchy;
        if (Input.GetKeyDown(KeyCode.Space) && !isActive)
        {
            ShowNextNode();
            return;
        }
    }
    public void StartStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogWarning("StoryData 是空的");
            return;
        }

        currentStory = story;
        BuildNodeLookup();

        if (!string.IsNullOrEmpty(currentStory.startNodeId))
        {
            ShowNode(currentStory.startNodeId);
        }
        else
        {
            Debug.LogWarning("startNodeId 是空的");
        }
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

        DialogueNode node = nodeLookup[nodeId];
        Debug.Log("GameState.Instance is null? " + (GameState.Instance == null));
        Debug.Log("requiredFlags is null? " + (node.requiredFlags == null));
        Debug.Log("node is null? " + (node == null));

        if (!GameState.Instance.HasAllFlags(node.requiredFlags))
        {
            Debug.Log("跳過節點: " + nodeId);

            if (!string.IsNullOrEmpty(node.nextNodeId))
            {
                ShowNode(node.nextNodeId);
            }
            else
            {
                Debug.LogWarning("節點條件不符，且沒有 nextNodeId: " + nodeId);
            }

            return;
        }


        currentNode = node;
        UpdateVisuals(currentNode);
        ApplyNodeFlags(currentNode);


        nameText.text = currentNode.speakerName;
        bodyText.text = currentNode.bodyText;
        bool hasChoices =
        currentNode.choices != null &&
        currentNode.choices.Count > 0;

        if (hasChoices)
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.ResetAIUsed();
            }
            currentQuestionStartedAt = DateTimeOffset.Now;
            groupQuestionOrder++;
            overallQuestionOrder++;

            Debug.Log(
                $"開始記錄題目時間："
                + $"題組={currentQuestionGroup}，"
                + $"題目={currentNode.nodeId}，"
                + $"組內第 {groupQuestionOrder} 題，"
                + $"全域第 {overallQuestionOrder} 題",
                this
            );
        }


        UpdateChoices();
    }
    public void FBClose()
    {
        feedbackPanel.SetActive(false);
    }
    private void ShowNextNode()
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
    private void ApplyNodeFlags(DialogueNode node)
    {
        if (node.setFlags == null) return;

        foreach (string flag in node.setFlags)
        {
            GameState.Instance.SetFlag(flag);
        }
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
    private void EndDialogue()
    {
        currentNode = null;
        nodeLookup.Clear();
        nameText.text = "";
        bodyText.text = "故事結束";

        currentStory = null;
        choiceGroup.SetActive(false);
        dialogueRoot.SetActive(false);
        mapFlowController.BackToMap();
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

    /*public void SelectChoice(ChoiceData choiceData)
    {
        if (!string.IsNullOrEmpty(choiceData.setFlag))
        {
            GameState.Instance.SetFlag(choiceData.setFlag);
        }
        GameState.Instance.GetScore(choiceData.fundingDelta, choiceData.interestDelta,choiceData.sustainabilityDelta);

        if (choiceData.feedbackText != null && GameState.Instance.IsFeedbackEnabled)
        {

            feedbackPanel.SetActive(true);
            feedbackText.text = choiceData.feedbackText;
        }


        ShowNode(choiceData.nextNodeId);
    }*/
    public void SelectChoice(ChoiceData choiceData)
    {
        if (choiceData == null)
        {
            Debug.LogWarning("選擇資料為空，無法記錄作答。", this);
            return;
        }

        bool currentNodeHasChoices =
            currentNode != null &&
            currentNode.choices != null &&
            currentNode.choices.Count > 0;

        if (currentNodeHasChoices)
        {
            if (testResultUploader == null)
            {
                testResultUploader =
                    FindFirstObjectByType<LearningTestResultUploader>();
            }

            if (testResultUploader == null)
            {
                Debug.LogWarning(
                    "找不到 LearningTestResultUploader，"
                    + "本題不會記錄到第二部分資料。",
                    this
                );
            }
            else if (string.IsNullOrWhiteSpace(currentQuestionGroup))
            {
                Debug.LogWarning(
                    "尚未設定 currentQuestionGroup，"
                    + "本題不會記錄到第二部分資料。",
                    this
                );
            }
            else
            {
                LearningTestResultUploader uploader = GetTestUploader();

                if (uploader == null)
                {
                    Debug.LogWarning(
                        "找不到 LearningTestResultUploader，"
                        + "本題作答不會記錄。",
                        this
                    );
                }
                else
                {
                    uploader.RecordTestAnswer(
                        questionGroup: currentQuestionGroup,
                        questionId: currentNode.nodeId,
                        questionOrder: groupQuestionOrder,
                        overallQuestionOrder: overallQuestionOrder,
                        selectedAnswer: choiceData.choiceText,
                        isCorrect: choiceData.isCorrect,
                        questionStartedAt: currentQuestionStartedAt,
                        usedAiHint: GameState.Instance != null &&
                         GameState.Instance.GetAIUsed()
                    );
                }
            }
        }

        if (!string.IsNullOrEmpty(choiceData.setFlag))
        {
            GameState.Instance.SetFlag(choiceData.setFlag);
        }

        GameState.Instance.GetScore(
            choiceData.fundingDelta,
            choiceData.interestDelta,
            choiceData.sustainabilityDelta
        );

        if (choiceData.feedbackText != null &&
            GameState.Instance.IsFeedbackEnabled)
        {
            feedbackPanel.SetActive(true);
            feedbackText.text = choiceData.feedbackText;
        }

        ShowNode(choiceData.nextNodeId);
    }

    private void UpdateVisuals(DialogueNode node)
    {
        if (node.showchatboxPortrait)
        {
            ChatboxImage.gameObject.SetActive(true);
        }
        else
        {
            ChatboxImage.gameObject.SetActive(false);
        }

        if (backgroundImage != null)
        {
            if (node.backgroundSprite != null)
            {
                backgroundImage.sprite = node.backgroundSprite;
                backgroundImage.enabled = true;
            }
        }

        if (characterLeftImage != null)
        {
            if (node.leftPortrait != null)
            {
                characterLeftImage.sprite = node.leftPortrait;
                characterLeftImage.enabled = true;
            }
            else
            {
                characterLeftImage.enabled = false;
            }
        }

        if (characterRightImage != null)
        {
            if (node.rightPortrait != null)
            {
                characterRightImage.sprite = node.rightPortrait;
                characterRightImage.enabled = true;
            }
            else
            {
                characterRightImage.enabled = false;
            }
        }

        if (characterMidImage != null)
        {
            if (node.centerPortrait != null)
            {
                characterMidImage.sprite = node.centerPortrait;
                characterMidImage.enabled = true;
            }
            else
            {
                characterMidImage.enabled = false;
            }
        }
    }
    public void StartTestGroup(string questionGroup)
    {
        if (questionGroup != "group_1" &&
            questionGroup != "group_2" &&
            questionGroup != "group_3" &&
            questionGroup != "group_4")
        {
            Debug.LogError(
                $"無效題組：{questionGroup}。"
                + "只允許 group_1 到 group_4。",
                this
            );
            return;
        }

        currentQuestionGroup = questionGroup;
        groupQuestionOrder = 0;

        Debug.Log($"開始第二部分題組：{currentQuestionGroup}", this);
    }
    private LearningTestResultUploader GetTestUploader()
    {
        if (testResultUploader == null)
        {
            testResultUploader =
                FindFirstObjectByType<LearningTestResultUploader>();
        }

        return testResultUploader;
    }
}