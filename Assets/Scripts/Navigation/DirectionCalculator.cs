using UnityEngine;

namespace ARFireNavigation.Navigation
{
    public static class DirectionCalculator
    {
        private const float Epsilon = 0.0001f;

        public static Vector3 CalculateDirection(Vector3 from, Vector3 to, bool flattenY = true)
        {
            Vector3 diff = to - from;
            if (flattenY)
            {
                diff.y = 0f;
            }

            float sqrMag = diff.sqrMagnitude;
            if (sqrMag < Epsilon)
            {
                return Vector3.forward;
            }

            return diff.normalized;
        }

        public static float CalculateDistance(Vector3 from, Vector3 to, bool ignoreY = true)
        {
            if (ignoreY)
            {
                float dx = to.x - from.x;
                float dz = to.z - from.z;
                return Mathf.Sqrt(dx * dx + dz * dz);
            }
            return Vector3.Distance(from, to);
        }

        public static float CalculateYawAngle(Vector3 from, Vector3 to)
        {
            Vector3 dir = CalculateDirection(from, to, true);
            return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }

        public static float CalculateRelativeAngle(Vector3 userForward, Vector3 targetDirection)
        {
            userForward.y = 0f;
            targetDirection.y = 0f;

            if (userForward.sqrMagnitude < Epsilon) userForward = Vector3.forward;
            if (targetDirection.sqrMagnitude < Epsilon) targetDirection = Vector3.forward;

            userForward.Normalize();
            targetDirection.Normalize();

            return Vector3.SignedAngle(userForward, targetDirection, Vector3.up);
        }

        public static string GetTurnInstruction(float relativeAngle, float distanceToWaypoint, float arrivalRadius = 1.2f)
        {
            if (distanceToWaypoint <= arrivalRadius)
            {
                return "Approaching Waypoint";
            }

            if (relativeAngle >= -25f && relativeAngle <= 25f)
            {
                return $"Continue Straight ({distanceToWaypoint:F1}m)";
            }
            else if (relativeAngle > 25f && relativeAngle <= 65f)
            {
                return $"Bear Right in {distanceToWaypoint:F1}m";
            }
            else if (relativeAngle > 65f && relativeAngle <= 120f)
            {
                return $"Turn Right in {distanceToWaypoint:F1}m";
            }
            else if (relativeAngle > 120f)
            {
                return $"Sharp Right / Turn Around ({distanceToWaypoint:F1}m)";
            }
            else if (relativeAngle >= -65f && relativeAngle < -25f)
            {
                return $"Bear Left in {distanceToWaypoint:F1}m";
            }
            else if (relativeAngle >= -120f && relativeAngle < -65f)
            {
                return $"Turn Left in {distanceToWaypoint:F1}m";
            }
            else
            {
                return $"Sharp Left / Turn Around ({distanceToWaypoint:F1}m)";
            }
        }

        public static Quaternion SmoothRotation(Quaternion current, Quaternion target, float speed, float deltaTime)
        {
            if (speed <= 0f || deltaTime <= 0f) return target;
            float t = 1.0f - Mathf.Exp(-speed * deltaTime);
            return Quaternion.Slerp(current, target, t);
        }

        public static Quaternion ApplyModelOffset(Quaternion worldRotation, Vector3 modelOffsetEuler)
        {
            if (modelOffsetEuler.sqrMagnitude < Epsilon)
                return worldRotation;
            return worldRotation * Quaternion.Euler(modelOffsetEuler);
        }
    }
}
