using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public class LocationMember : MonoBehaviour
    {
        [SerializeField] internal Anchor connectedAnchor;
        [SerializeField] internal bool keepLastValidAnchor = true;

        public void FindClosestAnchor()
        {
            var res = transform.DetermineLocation();
            if (res || !keepLastValidAnchor)
                connectedAnchor = res;
            if (!connectedAnchor)
                QuestLogger.LogWarning($"[{GetType().Name}]: {name} cannot find closest anchor! If this is unintentional, please make sure the layer of the collider is not targeted by the SFS.");
        }
    }
}