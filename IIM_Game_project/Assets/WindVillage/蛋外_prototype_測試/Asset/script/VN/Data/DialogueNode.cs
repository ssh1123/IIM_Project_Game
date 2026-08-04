using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string speakerName;
    [TextArea(3, 6)]
    public string bodyText;

    public Sprite backgroundSprite;
    public Sprite leftPortrait;
    public Sprite centerPortrait;
    public Sprite rightPortrait;
    public bool showchatboxPortrait;



    public List<ChoiceData> choices = new List<ChoiceData>();

    public string nextNodeId;

    public List<string> setFlags = new List<string>();
    public List<string> requiredFlags = new List<string>();

    public string eventType;
    /*
    nodeId：這句話的編號，像 N001、N002。

    speakerName：誰在講話。

    bodyText：台詞內容。

    backgroundSprite：這句話出現時要不要切背景。
    leftPortrait / centerPortrait / rightPortrait：角色立繪槽位。

    choices：如果這句有選項，就把選項放這裡。

    nextNodeId：如果沒有選項，按下一句要去哪。

    setFlags：進入這個節點時順便設定哪些旗標。

    requiredFlags：需要有哪些旗標，這句才成立。

    eventType：要不要觸發事件，例如 FadeIn、PlayBGM。


    */
}