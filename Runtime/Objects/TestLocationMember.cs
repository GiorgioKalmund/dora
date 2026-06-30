using UnityEngine;
using TMPro;

namespace giorgiokalmund.Dora
{
    public class TestLocationMember : LocationMember
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            onLocationChanged.AddListener(loc =>
            {
                text.text = loc?.name ?? "No Space";
            });
        }
    }
}