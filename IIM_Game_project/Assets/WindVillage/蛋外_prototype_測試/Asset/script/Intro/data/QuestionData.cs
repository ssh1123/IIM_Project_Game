using UnityEngine;

[System.Serializable]
public class QuestionData
{
    [Header("Question")]
    [TextArea(2, 4)]
    public string questionText;

    [Header("Choices")]
    public string topText;
    public string middleText;
    public string bottomText;

    [Header("Answer")]
    public LaneType correctLane;

    [Header("Feedback")]
    [TextArea(2, 4)]
    public string correctExplanation;

    [TextArea(2, 4)]
    public string wrongExplanation;
}