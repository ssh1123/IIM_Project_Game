using System;
using UnityEngine;

[Serializable]
public class ChoiceData
{
    public string choiceText;
    public string nextNodeId;
    public string setFlag;
    public int fundingDelta;
    public int interestDelta;
    public int sustainabilityDelta;
    public string feedbackText;
    /*
    choiceText：按鈕上顯示的字。
    nextNodeId：選了這個選項後，跳到哪一個節點。
    setFlag：選這個選項時，要不要順便記錄一個旗標。
    */
}