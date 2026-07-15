using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private HashSet<string> flags = new HashSet<string>();

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            flags.Add(flag);
        }
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public bool HasAllFlags(List<string> requiredFlags)
    {
        if (requiredFlags == null || requiredFlags.Count == 0)
            return true;

        foreach (string flag in requiredFlags)
        {
            if (!flags.Contains(flag))
                return false;
        }

        return true;
    }

    public void ClearFlags()
    {
        flags.Clear();
    }
}