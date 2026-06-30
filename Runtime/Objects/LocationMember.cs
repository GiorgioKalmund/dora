using SpaceFoundationSystem;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    public class LocationMember : MonoBehaviour
    {
        [SerializeField, Tooltip("The currently connected anchor. Can be used to filter out members based on their location.")]
        internal Anchor connectedAnchor;
        [SerializeField, Tooltip("If search for an anchor fails, keep the reference to the last valid anchor instead of referencing null.")] 
        internal bool keepLastValidAnchor;
        [Header("SpaceFoundation")]
        [SerializeField, Tooltip("Optional reference to a SpaceFoundation, avoiding the need to re-search for it when trying to determine a location.")] 
        internal SpaceFoundation spaceFoundation;
        [SerializeField, Tooltip("Whether to constantly check and update the member's location.")] private bool doUpdate = false;

        public UnityEvent<Anchor> onLocationChanged = new UnityEvent<Anchor>();
        public UnityEvent<Anchor, Anchor> onLocationChangedWithPrevious = new UnityEvent<Anchor, Anchor>();

        private void Update()
        {
            if (doUpdate)
                FindClosestAnchor();
        }

        public void FindClosestAnchor()
        {
            var res = transform.DetermineLocation(spaceFoundation);
            if (res || !keepLastValidAnchor)
            {
                onLocationChanged.Invoke(res);
                onLocationChangedWithPrevious.Invoke(connectedAnchor, res);
                connectedAnchor = res;
            }
            if (!connectedAnchor)
                QuestLogger.LogWarning($"[{GetType().Name}]: {gameObject.name} cannot find closest anchor! If this is unintentional, please make sure the layer of the collider is not targeted by the SFS and the object is positioned in a non-void or non-border voxel!", this);
        }
    }
}