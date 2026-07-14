using UnityEngine;

namespace WindVillage
{
    public class LocationButton : MonoBehaviour
    {
        [SerializeField] private string locationId;
        [SerializeField] private WindVillageMapController controller;

        public void Configure(string id, WindVillageMapController mapController)
        {
            locationId = id;
            controller = mapController;
        }

        public void Click()
        {
            if (controller != null) controller.SelectLocation(locationId);
        }
    }
}
