using System.Linq;
using UnityEngine;

namespace giorgiokalmund.Dora.Generation.StdLib
{
    [CreateAssetMenu(fileName = "Line", menuName = "Dora/Generation/Line")]
    public class LinePattern : BulkObjectPattern
    {
        [SerializeField] [Min(1)]
        internal int segmentCount;
        [SerializeField]
        private Vector3 pos1;
        [SerializeField]
        private Vector3 pos2;

        internal Transform latestHandle1;
        internal Transform latestHandle2;
        
        private int ObjectsPerSegment => generationPool.ToDictionary().Values.Sum();
        
        protected override Vector3 GetNextPosition()
        {
            int elementCount = ObjectsPerSegment * segmentCount;
            if (elementCount <= 1)
                return pos1;
            
            float distance = (pos2 - pos1).magnitude;
            float spacing = distance / (elementCount - 1);
            
            Vector3 direction = (pos2 - pos1).normalized;
            return pos1 + direction * Generated.Count * spacing;
        }

        public override void Generate()
        {
            if (ShouldClearOnGenerate)
                Clear();
            ShouldClearOnGenerate = false;
            for (int i = 0; i < segmentCount; i++)
            {
                base.Generate();
            }
            ShouldClearOnGenerate = true;
        }

        public void SetPos1(Vector3 p1)
        {
            pos1 = p1;
        }
        public void SetPos2(Vector3 p2)
        {
            pos2 = p2;
        }
        public void SetPositions(Vector3 p1, Vector3 p2)
        {
            pos1 = p1;
            pos2 = p2;
        }
        
        protected override void OnFinishGeneration(GameObject obj)
        {
            
        }

        public override bool CanGenerate() => !pos1.Equals(pos2);
    }
}