using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    public sealed class ScrollingGridBackground : MonoBehaviour
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private float baseScrollSpeedX = 0.05f;
        [SerializeField] private float baseScrollSpeedY = 0.04f;

        private Material targetMaterial;
        private Vector2 offset;
        private float speedFactor = 1f;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<RawImage>();
            }

            if (targetImage != null && targetImage.material != null)
            {
                targetMaterial = Instantiate(targetImage.material);
                targetImage.material = targetMaterial;
            }
        }

        private void Update()
        {
            if (targetMaterial == null)
            {
                return;
            }

            offset.x += baseScrollSpeedX * speedFactor * Time.deltaTime;
            offset.y += baseScrollSpeedY * speedFactor * Time.deltaTime;
            targetMaterial.SetTextureOffset("_MainTex", offset);
        }

        public void SetSpeedFactor(float multiplier)
        {
            speedFactor = Mathf.Clamp(multiplier * 0.5f, 1f, 15f);
        }
    }
}
