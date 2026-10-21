using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFireNavigation.Navigation
{
    [Serializable]
    public class WaypointNode
    {
        public Vector3 position;
        public string instruction;
        public float customArrivalRadius;
        public bool isExit;

        public WaypointNode() { }

        public WaypointNode(Vector3 position, string instruction, bool isExit = false, float customArrivalRadius = 0f)
        {
            this.position = position;
            this.instruction = instruction;
            this.isExit = isExit;
            this.customArrivalRadius = customArrivalRadius;
        }
    }

    public class WaypointRoute : MonoBehaviour
    {
        [Header("Route Definition")]
        [SerializeField] private List<WaypointNode> waypoints = new List<WaypointNode>();
        [SerializeField] private float defaultArrivalRadius = 1.2f;

        public IReadOnlyList<WaypointNode> Waypoints => waypoints;
        public int WaypointCount => waypoints != null ? waypoints.Count : 0;
        public float DefaultArrivalRadius
        {
            get => defaultArrivalRadius;
            set => defaultArrivalRadius = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                SetupDemonstrationEmergencyRoute();
            }
        }

        public void SetupDemonstrationEmergencyRoute()
        {
            waypoints = new List<WaypointNode>
            {
                new WaypointNode(new Vector3(0f, 0f, 0f), "Evacuation Start: Main Hallway Entrance", false),
                new WaypointNode(new Vector3(0f, 0f, 6f), "Walk straight north along central corridor (6m)", false),
                new WaypointNode(new Vector3(5f, 0f, 6f), "Turn Right into East Wing Fire Exit Corridor (5m)", false),
                new WaypointNode(new Vector3(5f, 0f, 14f), "Turn Left toward Stairwell B (8m)", false),
                new WaypointNode(new Vector3(1.5f, 0f, 14f), "Turn Left: Emergency Fire Exit Door [SAFE EVACUATION REACHED]", true)
            };
        }

        public void SetWaypoints(List<WaypointNode> newWaypoints)
        {
            waypoints = newWaypoints ?? new List<WaypointNode>();
        }

        public bool ValidateRoute(out string errorMessage)
        {
            if (waypoints == null || waypoints.Count < 2)
            {
                errorMessage = "Route must contain at least 2 valid waypoints.";
                return false;
            }

            for (int i = 0; i < waypoints.Count; i++)
            {
                var wp = waypoints[i];
                if (float.IsNaN(wp.position.x) || float.IsNaN(wp.position.y) || float.IsNaN(wp.position.z) ||
                    float.IsInfinity(wp.position.x) || float.IsInfinity(wp.position.y) || float.IsInfinity(wp.position.z))
                {
                    errorMessage = $"Waypoint index {i} contains invalid (NaN/Infinity) coordinate values.";
                    return false;
                }

                if (i > 0)
                {
                    float dist = Vector3.Distance(waypoints[i - 1].position, wp.position);
                    if (dist < 0.1f)
                    {
                        errorMessage = $"Adjacent waypoints {i - 1} and {i} are too close ({dist:F2}m < 0.1m).";
                        return false;
                    }
                }
            }

            if (!waypoints[waypoints.Count - 1].isExit)
            {
                waypoints[waypoints.Count - 1].isExit = true;
            }

            errorMessage = string.Empty;
            return true;
        }

        public WaypointNode GetWaypoint(int index)
        {
            if (waypoints == null || index < 0 || index >= waypoints.Count)
                return null;
            return waypoints[index];
        }

        public float GetTotalRouteDistance()
        {
            if (waypoints == null || waypoints.Count < 2) return 0f;
            float total = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                total += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }
            return total;
        }

        public float GetRemainingDistance(Vector3 currentPosition, int currentWaypointIndex)
        {
            if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
                return 0f;

            float dist = DirectionCalculator.CalculateDistance(currentPosition, waypoints[currentWaypointIndex].position, true);
            for (int i = currentWaypointIndex; i < waypoints.Count - 1; i++)
            {
                dist += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }
            return dist;
        }

        public float GetProgress(Vector3 currentPosition, int currentWaypointIndex)
        {
            float total = GetTotalRouteDistance();
            if (total <= 0.001f) return 1.0f;
            float remaining = GetRemainingDistance(currentPosition, currentWaypointIndex);
            float progress = 1.0f - (remaining / total);
            return Mathf.Clamp01(progress);
        }

        public bool IsAtWaypoint(Vector3 currentPosition, int waypointIndex, float overrideRadius = -1f)
        {
            if (waypoints == null || waypointIndex < 0 || waypointIndex >= waypoints.Count)
                return false;

            var wp = waypoints[waypointIndex];
            float radius = overrideRadius > 0f ? overrideRadius :
                           (wp.customArrivalRadius > 0f ? wp.customArrivalRadius : defaultArrivalRadius);

            float horizontalDist = DirectionCalculator.CalculateDistance(currentPosition, wp.position, true);
            return horizontalDist <= radius;
        }
    }
}
