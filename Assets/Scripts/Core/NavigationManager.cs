using System;
using UnityEngine;
using ARFireNavigation.Navigation;
using ARFireNavigation.Utilities;

namespace ARFireNavigation.Core
{
    public enum NavigationState
    {
        WaitingForTracking,
        ReadyToStart,
        Navigating,
        Completed,
        TrackingLost,
        Error
    }

    public class NavigationManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ARTrackingManager trackingManager;
        [SerializeField] private WaypointRoute route;
        [SerializeField] private ArrowController arrowController;
        [SerializeField] private NavigationUI navigationUI;
        [SerializeField] private NavigationConfiguration config;
        [SerializeField] private Transform userCameraTransform;

        public event Action<NavigationState, NavigationState> OnStateChanged;
        public event Action<int, WaypointNode> OnWaypointReached;
        public event Action OnDestinationReached;
        public event Action<string> OnInstructionUpdated;

        private NavigationState currentState = NavigationState.WaitingForTracking;
        private int currentWaypointIndex = 0;
        private int consecutiveArrivalFrames = 0;
        private float distanceToNext = 0f;
        private float distanceTotal = 0f;
        private float routeProgress = 0f;

        public NavigationState CurrentState => currentState;
        public int CurrentWaypointIndex => currentWaypointIndex;
        public float DistanceToNext => distanceToNext;
        public float DistanceTotal => distanceTotal;
        public float RouteProgress => routeProgress;

        private void Awake()
        {
            if (trackingManager == null) trackingManager = FindAnyObjectByType<ARTrackingManager>();
            if (route == null) route = FindAnyObjectByType<WaypointRoute>();
            if (arrowController == null) arrowController = FindAnyObjectByType<ArrowController>();
            if (navigationUI == null) navigationUI = FindAnyObjectByType<NavigationUI>();
            if (userCameraTransform == null)
            {
                var cam = Camera.main;
                if (cam != null) userCameraTransform = cam.transform;
            }
        }

        private void Start()
        {
            if (navigationUI != null)
            {
                navigationUI.OnStartButtonClicked += StartNavigation;
                navigationUI.OnRestartButtonClicked += RestartNavigation;
                navigationUI.OnStopButtonClicked += StopNavigation;
            }

            if (trackingManager != null)
            {
                trackingManager.OnTrackingReady += HandleTrackingReady;
                trackingManager.OnTrackingLost += HandleTrackingLost;
                trackingManager.OnTrackingResumed += HandleTrackingResumed;
                trackingManager.OnTrackingStatusChanged += HandleTrackingStatusChanged;
                trackingManager.OnTrackingError += HandleTrackingError;
            }

            TransitionState(NavigationState.WaitingForTracking);
        }

        private void OnDestroy()
        {
            if (navigationUI != null)
            {
                navigationUI.OnStartButtonClicked -= StartNavigation;
                navigationUI.OnRestartButtonClicked -= RestartNavigation;
                navigationUI.OnStopButtonClicked -= StopNavigation;
            }

            if (trackingManager != null)
            {
                trackingManager.OnTrackingReady -= HandleTrackingReady;
                trackingManager.OnTrackingLost -= HandleTrackingLost;
                trackingManager.OnTrackingResumed -= HandleTrackingResumed;
                trackingManager.OnTrackingStatusChanged -= HandleTrackingStatusChanged;
                trackingManager.OnTrackingError -= HandleTrackingError;
            }
        }

        public void TransitionState(NavigationState newState)
        {
            if (currentState == newState) return;

            NavigationState oldState = currentState;
            currentState = newState;

            ApplyStateBehaviors(oldState, newState);
            OnStateChanged?.Invoke(oldState, newState);
        }

        private void ApplyStateBehaviors(NavigationState oldState, NavigationState newState)
        {
            switch (newState)
            {
                case NavigationState.WaitingForTracking:
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("INITIALIZING AR", new Color(0.9f, 0.6f, 0.1f));
                        navigationUI.SetInstruction("Calibrating ARCore sensors... Please wait.");
                        navigationUI.SetButtonStates(false, false, false);
                        navigationUI.ShowCompletion(false);
                        navigationUI.ShowTrackingAlert(false);
                    }
                    if (arrowController != null) arrowController.ClearArrows();
                    break;

                case NavigationState.ReadyToStart:
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("TRACKING READY", new Color(0.1f, 0.85f, 0.3f));
                        navigationUI.SetInstruction("Tracking locked. Press Start Navigation to begin.");
                        navigationUI.SetButtonStates(true, false, false);
                        navigationUI.ShowCompletion(false);
                        navigationUI.ShowTrackingAlert(false);
                    }
                    if (arrowController != null) arrowController.ClearArrows();
                    break;

                case NavigationState.Navigating:
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("EVACUATING", new Color(0.15f, 0.55f, 1.0f));
                        navigationUI.SetButtonStates(false, true, true);
                        navigationUI.ShowCompletion(false);
                        navigationUI.ShowTrackingAlert(false);
                    }
                    break;

                case NavigationState.Completed:
                    string exitLabel = config != null ? config.DestinationExitLabel : "FIRE EXIT B";
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("EVACUATION COMPLETE", new Color(0.1f, 0.9f, 0.3f));
                        navigationUI.SetInstruction($"Arrived at {exitLabel}. Stay safe.");
                        navigationUI.SetButtonStates(false, true, true);
                        navigationUI.ShowCompletion(true, exitLabel);
                        navigationUI.ShowTrackingAlert(false);
                    }
                    if (arrowController != null) arrowController.ClearArrows();
                    OnDestinationReached?.Invoke();
                    break;

                case NavigationState.TrackingLost:
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("TRACKING LOST", new Color(0.95f, 0.25f, 0.25f));
                        navigationUI.SetButtonStates(false, true, true);
                        navigationUI.ShowTrackingAlert(true, "AR tracking interrupted. Point device at the floor.");
                    }
                    if (arrowController != null) arrowController.SetArrowsVisible(false);
                    break;

                case NavigationState.Error:
                    if (navigationUI != null)
                    {
                        navigationUI.SetTrackingStatus("ERROR", new Color(0.95f, 0.1f, 0.1f));
                        navigationUI.SetButtonStates(false, false, false);
                    }
                    if (arrowController != null) arrowController.ClearArrows();
                    break;
            }
        }

        public void StartNavigation()
        {
            if (currentState != NavigationState.ReadyToStart && currentState != NavigationState.Completed)
            {
                Debug.LogWarning($"[NavigationManager] Cannot start navigation from state: {currentState}");
                return;
            }

            if (route == null)
            {
                Debug.LogError("[NavigationManager] Route reference is null.");
                TransitionState(NavigationState.Error);
                return;
            }

            string err;
            if (!route.ValidateRoute(out err))
            {
                Debug.LogError($"[NavigationManager] Route validation failed: {err}");
                TransitionState(NavigationState.Error);
                return;
            }

            currentWaypointIndex = 0;
            consecutiveArrivalFrames = 0;
            TransitionState(NavigationState.Navigating);
            UpdateNavigationCalculations();
        }

        public void RestartNavigation()
        {
            currentWaypointIndex = 0;
            consecutiveArrivalFrames = 0;
            if (arrowController != null) arrowController.ClearArrows();

            if (trackingManager != null && trackingManager.IsTrackingReady)
            {
                TransitionState(NavigationState.Navigating);
                UpdateNavigationCalculations();
            }
            else
            {
                TransitionState(NavigationState.WaitingForTracking);
            }
        }

        public void StopNavigation()
        {
            ResetNavigation();
        }

        public void ResetNavigation()
        {
            currentWaypointIndex = 0;
            consecutiveArrivalFrames = 0;
            distanceToNext = 0f;
            distanceTotal = 0f;
            routeProgress = 0f;

            if (arrowController != null) arrowController.ClearArrows();

            if (trackingManager != null && trackingManager.IsTrackingReady)
            {
                TransitionState(NavigationState.ReadyToStart);
            }
            else
            {
                TransitionState(NavigationState.WaitingForTracking);
            }
        }

        private void Update()
        {
            if (currentState == NavigationState.Navigating)
            {
                UpdateNavigationCalculations();
            }
        }

        public void UpdateNavigationCalculations()
        {
            if (route == null || route.WaypointCount == 0) return;

            Vector3 userPos = userCameraTransform != null ? userCameraTransform.position : Vector3.zero;
            Vector3 userForward = userCameraTransform != null ? userCameraTransform.forward : Vector3.forward;

            if (currentWaypointIndex >= route.WaypointCount)
            {
                TransitionState(NavigationState.Completed);
                return;
            }

            var currentTargetNode = route.GetWaypoint(currentWaypointIndex);
            if (currentTargetNode == null) return;

            float arrivalRad = config != null ? config.ArrivalRadius : route.DefaultArrivalRadius;
            bool insideArrivalZone = route.IsAtWaypoint(userPos, currentWaypointIndex, arrivalRad);

            if (insideArrivalZone)
            {
                consecutiveArrivalFrames++;
                int requiredConfirmations = config != null ? config.ArrivalConfirmationFrames : 3;

                if (consecutiveArrivalFrames >= requiredConfirmations)
                {
                    consecutiveArrivalFrames = 0;
                    OnWaypointReached?.Invoke(currentWaypointIndex, currentTargetNode);

                    if (currentTargetNode.isExit || currentWaypointIndex == route.WaypointCount - 1)
                    {
                        TransitionState(NavigationState.Completed);
                        return;
                    }
                    else
                    {
                        currentWaypointIndex++;
                        currentTargetNode = route.GetWaypoint(currentWaypointIndex);
                    }
                }
            }
            else
            {
                consecutiveArrivalFrames = 0;
            }

            if (currentTargetNode != null)
            {
                distanceToNext = DirectionCalculator.CalculateDistance(userPos, currentTargetNode.position, true);
                distanceTotal = route.GetRemainingDistance(userPos, currentWaypointIndex);
                routeProgress = route.GetProgress(userPos, currentWaypointIndex);

                Vector3 targetDir = DirectionCalculator.CalculateDirection(userPos, currentTargetNode.position, true);
                float relAngle = DirectionCalculator.CalculateRelativeAngle(userForward, targetDir);

                string turnText = DirectionCalculator.GetTurnInstruction(relAngle, distanceToNext, arrivalRad);
                string instructionMessage = string.IsNullOrEmpty(currentTargetNode.instruction) ? turnText : $"{currentTargetNode.instruction} ({turnText})";

                if (navigationUI != null)
                {
                    string exitLabel = config != null ? config.DestinationExitLabel : "FIRE EXIT B";
                    navigationUI.SetInstruction(instructionMessage);
                    navigationUI.SetMetrics(distanceToNext, distanceTotal, routeProgress, exitLabel);
                }

                OnInstructionUpdated?.Invoke(instructionMessage);

                if (arrowController != null)
                {
                    arrowController.UpdateArrows(userPos, currentWaypointIndex, route);
                }
            }
        }

        public void SimulateUserPosition(Vector3 position, Vector3 forward)
        {
            if (userCameraTransform != null)
            {
                userCameraTransform.position = position;
                userCameraTransform.forward = forward;
            }
            UpdateNavigationCalculations();
        }

        private void HandleTrackingReady()
        {
            if (currentState == NavigationState.WaitingForTracking)
            {
                TransitionState(NavigationState.ReadyToStart);
            }
        }

        private void HandleTrackingLost()
        {
            if (currentState == NavigationState.Navigating)
            {
                TransitionState(NavigationState.TrackingLost);
            }
        }

        private void HandleTrackingResumed()
        {
            if (currentState == NavigationState.TrackingLost)
            {
                TransitionState(NavigationState.Navigating);
                if (arrowController != null) arrowController.SetArrowsVisible(true);
            }
        }

        private void HandleTrackingStatusChanged(TrackingQuality quality, string msg)
        {
            Color c = quality switch
            {
                TrackingQuality.Ready => new Color(0.1f, 0.85f, 0.3f),
                TrackingQuality.Initializing => new Color(0.9f, 0.6f, 0.1f),
                TrackingQuality.Limited => new Color(0.95f, 0.5f, 0.1f),
                TrackingQuality.Lost => new Color(0.95f, 0.25f, 0.25f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };

            if (navigationUI != null)
            {
                navigationUI.SetTrackingStatus(quality.ToString().ToUpper(), c);
            }
        }

        private void HandleTrackingError(string err)
        {
            TransitionState(NavigationState.Error);
            if (navigationUI != null)
            {
                navigationUI.SetInstruction($"AR Error: {err}");
            }
        }
    }
}
