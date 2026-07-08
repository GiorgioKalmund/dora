using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace giorgiokalmund.Dora.Samples.Scripts
{
    public class SceneTeleporter : MonoBehaviour
    {
        [Header("Scene")] 
        [SerializeField] [Scene] private string sceneToSwitch;
        [Header("World")] 
        [SerializeField] private Collider markerArea;

        private void OnTriggerEnter(Collider other)
        {
            SceneManager.LoadSceneAsync(sceneToSwitch);
        }
    }
}
