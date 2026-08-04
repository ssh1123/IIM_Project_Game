using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryData", menuName = "VN/Story Data")]
public class StoryData : ScriptableObject
{
    public string storyId;
    public string startNodeId;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}