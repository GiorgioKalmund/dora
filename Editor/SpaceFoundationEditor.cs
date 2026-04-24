using SpaceFoundationSystem;
using UnityEditor;
using UnityEngine;

namespace giorgiokalmund.Dora.Editor
{
    [CustomEditor(typeof(SpaceFoundation))]
    public class SpaceFoundationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpaceFoundation sfs = (SpaceFoundation)target;

            var anchorDict = sfs.GetAnchors();
            if (anchorDict != null)
            {
                GUILayout.Label("Anchors", EditorStyles.boldLabel);
                foreach ((string id, Anchor anchor) in anchorDict)
                {
                    if (anchor == null) GUILayout.Label($"{id} destroyed");
                    else GUILayout.Label($"{id}: {anchor?.gameObject.name ?? "<i><NULL></i>"}", QuestDrawer.RichText);
                }
            }
            
            if (GUILayout.Button("Run Compute Graph"))
            {
                RunComputeGraph();
            }

            if (GUILayout.Button("Run Chunked Compute Graph"))
            {
                RunChunkedComputeGraph();
            }
            
            if (GUILayout.Button("Run Clear Data"))
            {
                RunClearData();
            }
            
            if (GUILayout.Button("Reattach Scene Anchors @ Data"))
            {
                sfs.ReattachSceneAnchorsInData();
            }
        }
        
        
        public static void RunComputeGraph()
        {
            
            SpaceFoundationBackend.ClearData();
            
            SpaceFoundationBackend.Compute();
                
            // SpaceFoundationBackend.Compute(_delimitingLayerMask, _voxelSize, _delimiterLayer, _spaceLayer);
        }
        
        public static void RunClearData()
        {
            SpaceFoundationBackend.ClearData();
        }
        
        public void RunChunkedComputeGraph()
        {
            SpaceFoundationBackend.RunChunkedComputeGraph();
        }
    }
}