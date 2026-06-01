using System;
using UnityEngine;
using UnityEngine.UI;

namespace Crashmania.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class DynamicOrientationManager : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GameObject portraitRoot;
        [SerializeField] private GameObject landscapeRoot;
        [SerializeField] private float portraitMatchWidthOrHeight = 0f;
        [SerializeField] private float landscapeMatchWidthOrHeight = 1f;
        [SerializeField] private bool applyGameOrientationPolicy = true;

        private Vector2Int lastScreenSize;
        private bool isPortrait = true;
        private bool hasApplied;

        public event Action<bool> OrientationChanged;

        public bool IsPortrait => isPortrait;
        public GameObject ActiveRoot => isPortrait ? portraitRoot : landscapeRoot;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (applyGameOrientationPolicy)
            {
                SceneOrientationPolicy.AllowAutoRotation();
            }

            CheckOrientation(true);
        }

        private void Update()
        {
            var currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize != lastScreenSize)
            {
                CheckOrientation(false);
            }
        }

        public void CheckOrientation(bool force)
        {
            ResolveReferences();

            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            lastScreenSize = new Vector2Int(width, height);
            var currentIsPortrait = width <= height;

            if (!force && hasApplied && currentIsPortrait == isPortrait)
            {
                return;
            }

            isPortrait = currentIsPortrait;
            hasApplied = true;
            ApplyLayout();
            OrientationChanged?.Invoke(isPortrait);
        }

        public void ForcePortraitForVerifier()
        {
            isPortrait = true;
            hasApplied = true;
            ApplyLayout();
            OrientationChanged?.Invoke(isPortrait);
        }

        public void ForceLandscapeForVerifier()
        {
            isPortrait = false;
            hasApplied = true;
            ApplyLayout();
            OrientationChanged?.Invoke(isPortrait);
        }

        private void ApplyLayout()
        {
            if (portraitRoot != null)
            {
                portraitRoot.SetActive(isPortrait);
            }

            if (landscapeRoot != null)
            {
                landscapeRoot.SetActive(!isPortrait);
            }

            if (canvasScaler != null)
            {
                canvasScaler.matchWidthOrHeight = isPortrait ? portraitMatchWidthOrHeight : landscapeMatchWidthOrHeight;
            }

            Canvas.ForceUpdateCanvases();
            RebuildActiveLayout();
        }

        private void RebuildActiveLayout()
        {
            var activeRoot = ActiveRoot;
            if (activeRoot == null)
            {
                return;
            }

            foreach (var rect in activeRoot.GetComponentsInChildren<RectTransform>(true))
            {
                LayoutRebuilder.MarkLayoutForRebuild(rect);
            }
        }

        private void ResolveReferences()
        {
            if (canvasScaler == null)
            {
                canvasScaler = GetComponent<CanvasScaler>();
            }

            if (portraitRoot == null)
            {
                portraitRoot = FindChild("Portrait_LayoutRoot");
            }

            if (landscapeRoot == null)
            {
                landscapeRoot = FindChild("Landscape_LayoutRoot");
            }
        }

        private GameObject FindChild(string objectName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                {
                    return child.gameObject;
                }
            }

            return null;
        }
    }
}
