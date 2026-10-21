using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARFireNavigation.Core
{
    public enum TrackingQuality
    {
        None,
        CheckingAvailability,
        Unsupported,
        NeedsInstall,
        Initializing,
        Ready,
        Limited,
        Lost
    }

    public class ARTrackingManager : MonoBehaviour
    {
        [Header("AR References")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARCameraManager arCameraManager;

        public event Action OnTrackingReady;
        public event Action OnTrackingLost;
        public event Action OnTrackingResumed;
        public event Action<TrackingQuality, string> OnTrackingStatusChanged;
        public event Action<string> OnTrackingError;

        private TrackingQuality currentQuality = TrackingQuality.None;
        private bool wasTrackingReady = false;

        public TrackingQuality CurrentQuality => currentQuality;
        public bool IsTrackingReady => currentQuality == TrackingQuality.Ready;

        private void Awake()
        {
            if (arSession == null)
            {
                arSession = FindAnyObjectByType<ARSession>();
            }
            if (arCameraManager == null)
            {
                arCameraManager = FindAnyObjectByType<ARCameraManager>();
            }
        }

        private void OnEnable()
        {
            ARSession.stateChanged += HandleARSessionStateChanged;
            if (arCameraManager != null)
            {
                arCameraManager.frameReceived += HandleFrameReceived;
            }
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= HandleARSessionStateChanged;
            if (arCameraManager != null)
            {
                arCameraManager.frameReceived -= HandleFrameReceived;
            }
        }

        public void StartTrackingCheck()
        {
            StartCoroutine(CheckARCoreAvailabilityRoutine());
        }

        private IEnumerator CheckARCoreAvailabilityRoutine()
        {
            SetQuality(TrackingQuality.CheckingAvailability, "Checking ARCore device availability...");

            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                SetQuality(TrackingQuality.Unsupported, "Device does not support Google ARCore.");
                OnTrackingError?.Invoke("ARCore is not supported on this device.");
                yield break;
            }

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                SetQuality(TrackingQuality.NeedsInstall, "ARCore installation required. Requesting install...");
                yield return ARSession.Install();
            }

            EvaluateState(ARSession.state);
        }

        private void HandleARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            EvaluateState(args.state);
        }

        private void HandleFrameReceived(ARCameraFrameEventArgs args)
        {
            // Frame callback used to monitor ongoing tracking health
            if (currentQuality == TrackingQuality.Ready && arSession != null && ARSession.state != ARSessionState.SessionTracking)
            {
                EvaluateState(ARSession.state);
            }
        }

        private void EvaluateState(ARSessionState state)
        {
            switch (state)
            {
                case ARSessionState.None:
                case ARSessionState.CheckingAvailability:
                    SetQuality(TrackingQuality.CheckingAvailability, "Checking ARCore availability...");
                    break;

                case ARSessionState.Unsupported:
                    SetQuality(TrackingQuality.Unsupported, "Device does not support Google ARCore.");
                    OnTrackingError?.Invoke("Google ARCore is unsupported on this hardware.");
                    break;

                case ARSessionState.NeedsInstall:
                case ARSessionState.Installing:
                    SetQuality(TrackingQuality.NeedsInstall, "ARCore services installing/updating...");
                    break;

                case ARSessionState.Ready:
                case ARSessionState.SessionInitializing:
                    SetQuality(TrackingQuality.Initializing, "Initializing AR Session & sensors...");
                    if (wasTrackingReady)
                    {
                        wasTrackingReady = false;
                        OnTrackingLost?.Invoke();
                    }
                    break;

                case ARSessionState.SessionTracking:
                    SetQuality(TrackingQuality.Ready, "AR Tracking Active & Stable.");
                    if (!wasTrackingReady)
                    {
                        wasTrackingReady = true;
                        OnTrackingReady?.Invoke();
                    }
                    else
                    {
                        OnTrackingResumed?.Invoke();
                    }
                    break;

                default:
                    SetQuality(TrackingQuality.Limited, "AR Tracking limited or calibrating.");
                    break;
            }
        }

        private void SetQuality(TrackingQuality quality, string message)
        {
            currentQuality = quality;
            OnTrackingStatusChanged?.Invoke(quality, message);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (wasTrackingReady)
                {
                    OnTrackingLost?.Invoke();
                }
                SetQuality(TrackingQuality.Lost, "App paused. Re-evaluating AR tracking upon resume.");
            }
            else
            {
                StartTrackingCheck();
            }
        }
    }
}
