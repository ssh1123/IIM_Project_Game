using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "Runner/Question Database")]
public class QuestionDatabase : ScriptableObject
{
    public List<QuestionData> questions = new List<QuestionData>();
}