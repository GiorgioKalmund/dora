using SpaceFoundationSystem;
using UnityEngine;

namespace giorgiokalmund.Dora
{
    public class LocationMember : MonoBehaviour
    {
        [SerializeField] internal Anchor connectedAnchor;

        public void FindClosestAnchor()
        {
            connectedAnchor = transform.DetermineLocation();
            if (!connectedAnchor)
                QuestLogger.LogWarning($"[{GetType().Name}]: {name} cannot find closest anchor! If this is unintentional, please make sure the layer of the collider is not targeted by the SFS.");
        }
    }
}