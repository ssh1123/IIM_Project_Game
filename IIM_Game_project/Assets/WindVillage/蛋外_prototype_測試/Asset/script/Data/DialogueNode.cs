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

    public List<ChoiceData> choices = new List<ChoiceData>();

    public string nextNodeId;

    public List<string> setFlags = new List<string>();
    public List<string> requiredFlags = new List<string>();

    public string eventType;
}