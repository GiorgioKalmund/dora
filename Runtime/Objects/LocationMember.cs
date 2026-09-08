using SpaceFoundationSystem;
using UnityEngine;
using UnityEngine.Events;

namespace giorgiokalmund.Dora
{
    public class LocationMember : MonoBehaviour
    {
        [SerializeField, Tooltip("The current location of the member. Can be used to filter out members based on their anchor / location.")]
        public Anchor currentLocation;
        [Header("SpaceFoundation")]
        [SerializeField, Tooltip("Optional reference to a SpaceFoundation, avoiding the need to re-search for it when trying to determine a location.")] 
        internal SpaceFoundation spaceFoundation;
        [SerializeField, Tooltip("Whether to constantly check and update the member's location.")]
        private bool doUpdate = true;

        public UnityEvent<Anchor> onLocationChanged = new UnityEvent<Anchor>();
        public UnityEvent<Anchor, Anchor> onLocationChangedWithPrevious = new UnityEvent<Anchor, Anchor>();

        private void FixedUpdate()
        {
            if (doUpdate)
                FindClosestAnchor();
        }

        public void FindClosestAnchor()
        {
            var res = transform.DetermineLocation(spaceFoundation);
            if (res)
            {
                if (spaceFoundation == null)
                {
                    // update reference to sfs.
                    // this is important if we had no sfs before, DetermineLocation will find one
                    // which we then need to re-assign back here!
                    spaceFoundation = res.correspondingSpaceFoundation; 
                }
            
                if (res != currentLocation)
                {
                    onLocationChanged.Invoke(res);
                    onLocationChangedWithPrevious.Invoke(currentLocation, res);
                    currentLocation = res;
                }
            }
            else
                DoraLogger.LogWarning($"[{GetType().Name}]: {gameObject.name} cannot find closest anchor! If this is unintentional, please make sure the layer of the collider is not targeted by the SFS and the object is positioned in a non-void or non-border voxel!", this);
        }
    }
}