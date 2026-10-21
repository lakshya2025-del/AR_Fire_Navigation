using UnityEngine;

namespace ARFireNavigation.Utilities
{
    [CreateAssetMenu(fileName = "NavigationConfig", menuName = "AR Fire/Navigation Config")]
    public class NavigationConfiguration : ScriptableObject
    {
        [Header("Route & Arrival Tolerances")]
        [Tooltip("Distance in meters to trigger arrival at an intermediate or final waypoint.")]
        [SerializeField] private float arrivalRadius = 1.2f;

        [Tooltip("Consecutive frames required inside arrival radius to prevent erratic waypoint jumps.")]
        [SerializeField] private int arrivalConfirmationFrames = 3;

        [Header("3D Arrow Visuals")]
        [Tooltip("Distance in meters between consecutive arrows along a navigation segment.")]
        [SerializeField] private float arrowSpacing = 1.0f;

        [Tooltip("Elevation offset above the floor or detected AR plane surface.")]
        [SerializeField] private float arrowHeightOffset = 0.05f;

        [Tooltip("Uniform or component scale applied to instantiated navigation arrows.")]
        [SerializeField] private Vector3 arrowScale = new Vector3(0.45f, 0.45f, 0.45f);

        [Tooltip("Correction rotation for 3D arrow models that do not face +Z.")]
        [SerializeField] private Vector3 modelForwardRotationOffset = Vector3.zero;

        [Tooltip("Smoothing speed for rotational heading transitions.")]
        [SerializeField] private float rotationSmoothingSpeed = 8.0f;

        [Tooltip("Maximum active pooled arrows visible simultaneously along route.")]
        [SerializeField] private int maxActiveArrows = 25;

        [Header("Emergency Destination Metadata")]
        [SerializeField] private string destinationExitLabel = "EMERGENCY FIRE EXIT B";

        public float ArrivalRadius => arrivalRadius;
        public int ArrivalConfirmationFrames => arrivalConfirmationFrames;
        public float ArrowSpacing => arrowSpacing;
        public float ArrowHeightOffset => arrowHeightOffset;
        public Vector3 ArrowScale => arrowScale;
        public Vector3 ModelForwardRotationOffset => modelForwardRotationOffset;
        public float RotationSmoothingSpeed => rotationSmoothingSpeed;
        public int MaxActiveArrows => maxActiveArrows;
        public string DestinationExitLabel => destinationExitLabel;
    }
}
