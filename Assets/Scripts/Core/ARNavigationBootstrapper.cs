using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARFireNavigation.Navigation;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace ARFireNavigation.Core
{
    public class ARNavigationBootstrapper : MonoBehaviour
    {
        [Header("Scene Component Verification")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARTrackingManager trackingManager;
        [SerializeField] private NavigationManager navigationManager;
        [SerializeField] private ArrowController arrowController;
        [SerializeField] private NavigationUI navigationUI;

        private static bool isInitialized = false;

        private void Awake()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ARNavigationBootstrapper] Duplicate initialization avoided.");
                return;
            }
            isInitialized = true;

            VerifyComponents();
        }

        private void Start()
        {
            StartCoroutine(InitializeApplicationFlowRoutine());
        }

        private void VerifyComponents()
        {
            if (arSession == null) arSession = FindAnyObjectByType<ARSession>();
            if (trackingManager == null) trackingManager = FindAnyObjectByType<ARTrackingManager>();
            if (navigationManager == null) navigationManager = FindAnyObjectByType<NavigationManager>();
            if (arrowController == null) arrowController = FindAnyObjectByType<ArrowController>();
            if (navigationUI == null) navigationUI = FindAnyObjectByType<NavigationUI>();

            if (arSession == null)
            {
                Debug.LogWarning("[ARNavigationBootstrapper] ARSession component not found in scene. Creating dynamic ARSession.");
                var sessionGo = new GameObject("AR Session_Dynamic");
                arSession = sessionGo.AddComponent<ARSession>();
                sessionGo.AddComponent<ARInputManager>();
            }

            if (trackingManager == null)
            {
                Debug.LogWarning("[ARNavigationBootstrapper] ARTrackingManager not found. Attaching to Bootstrapper GameObject.");
                trackingManager = gameObject.AddComponent<ARTrackingManager>();
            }

            if (navigationManager == null)
            {
                Debug.LogWarning("[ARNavigationBootstrapper] NavigationManager not found. Attaching to Bootstrapper GameObject.");
                navigationManager = gameObject.AddComponent<NavigationManager>();
            }

            if (arrowController == null)
            {
                Debug.LogWarning("[ARNavigationBootstrapper] ArrowController not found. Attaching to Bootstrapper GameObject.");
                arrowController = gameObject.AddComponent<ArrowController>();
            }
        }

        private IEnumerator InitializeApplicationFlowRoutine()
        {
            Debug.Log("[ARNavigationBootstrapper] Starting AR emergency navigation application flow...");

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("[ARNavigationBootstrapper] Requesting Android CAMERA permission...");
                Permission.RequestUserPermission(Permission.Camera);

                float waitTimeout = 4f;
                while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && waitTimeout > 0f)
                {
                    waitTimeout -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }
#endif
            yield return null;

            if (trackingManager != null)
            {
                trackingManager.StartTrackingCheck();
            }

            Debug.Log("[ARNavigationBootstrapper] Initialization flow completed successfully.");
        }
    }
}
