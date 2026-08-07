using UnityEngine;

[System.Serializable]
public class QuestionData
{
    [TextArea]
    public string questionText;

    public string topText;
    public string middleText;
    public string bottomText;

    public LaneType correctLane;

    [Header("Explanation / Feedback")]
    [TextArea]
    public string explanation;   // ★ 答題後顯示的一句說明
}