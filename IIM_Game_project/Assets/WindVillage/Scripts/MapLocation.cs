using UnityEngine;

namespace WindVillage
{
    [System.Serializable]
    public class MapLocation
    {
        public string id;
        public string displayName;
        [TextArea(2, 5)] public string description;
        public RectTransform target;
    }
}
