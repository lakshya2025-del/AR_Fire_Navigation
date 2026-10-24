using NUnit.Framework;
using UnityEngine;
using ARFireNavigation.Navigation;

namespace ARFireNavigation.Tests.EditMode
{
    public class DirectionCalculatorTests
    {
        [Test]
        public void CalculateDirection_ForwardStraight_ReturnsUnitForward()
        {
            Vector3 from = new Vector3(0, 0, 0);
            Vector3 to = new Vector3(0, 0, 10);
            Vector3 dir = DirectionCalculator.CalculateDirection(from, to, true);
            Assert.AreEqual(0f, dir.x, 0.001f);
            Assert.AreEqual(0f, dir.y, 0.001f);
            Assert.AreEqual(1f, dir.z, 0.001f);
        }

        [Test]
        public void CalculateDirection_CoincidentPoints_SafelyReturnsForwardWithoutNaN()
        {
            Vector3 p = new Vector3(5f, 2f, -3f);
            Vector3 dir = DirectionCalculator.CalculateDirection(p, p, true);
            Assert.IsFalse(float.IsNaN(dir.x));
            Assert.IsFalse(float.IsNaN(dir.y));
            Assert.IsFalse(float.IsNaN(dir.z));
            Assert.AreEqual(Vector3.forward, dir);
        }

        [Test]
        public void CalculateDistance_CalculatesHorizontalDistanceAccurately()
        {
            Vector3 from = new Vector3(1, 10, 2);
            Vector3 to = new Vector3(4, 50, 6);
            float dist2D = DirectionCalculator.CalculateDistance(from, to, true);
            // dx = 3, dz = 4 => sqrt(9 + 16) = 5
            Assert.AreEqual(5f, dist2D, 0.001f);
        }

        [Test]
        public void CalculateRelativeAngle_DetectsCardinalDirections()
        {
            Vector3 userForward = Vector3.forward;

            // Target straight ahead (0, 0, 1) -> ~0 deg
            float angleStraight = DirectionCalculator.CalculateRelativeAngle(userForward, Vector3.forward);
            Assert.AreEqual(0f, angleStraight, 0.01f);

            // Target 90 deg right (1, 0, 0) -> +90 deg
            float angleRight = DirectionCalculator.CalculateRelativeAngle(userForward, Vector3.right);
            Assert.AreEqual(90f, angleRight, 0.01f);

            // Target 90 deg left (-1, 0, 0) -> -90 deg
            float angleLeft = DirectionCalculator.CalculateRelativeAngle(userForward, Vector3.left);
            Assert.AreEqual(-90f, angleLeft, 0.01f);

            // Target behind (0, 0, -1) -> 180 deg
            float angleBack = DirectionCalculator.CalculateRelativeAngle(userForward, Vector3.back);
            Assert.AreEqual(180f, Mathf.Abs(angleBack), 0.01f);
        }

        [Test]
        public void GetTurnInstruction_ProducesCorrectInstructions()
        {
            string straight = DirectionCalculator.GetTurnInstruction(0f, 8.5f, 1.2f);
            Assert.IsTrue(straight.Contains("Continue Straight"));

            string right = DirectionCalculator.GetTurnInstruction(90f, 6.0f, 1.2f);
            Assert.IsTrue(right.Contains("Turn Right"));

            string left = DirectionCalculator.GetTurnInstruction(-90f, 4.0f, 1.2f);
            Assert.IsTrue(left.Contains("Turn Left"));

            string turnaround = DirectionCalculator.GetTurnInstruction(180f, 10.0f, 1.2f);
            Assert.IsTrue(turnaround.Contains("Turn Around"));

            string arrive = DirectionCalculator.GetTurnInstruction(0f, 0.8f, 1.2f);
            Assert.IsTrue(arrive.Contains("Approaching Waypoint"));
        }

        [Test]
        public void SmoothRotation_TransitionsSafelyTowardTarget()
        {
            Quaternion current = Quaternion.identity;
            Quaternion target = Quaternion.Euler(0, 90, 0);
            Quaternion smoothed = DirectionCalculator.SmoothRotation(current, target, 5f, 0.1f);

            Assert.AreNotEqual(current, smoothed);
            float angle = Quaternion.Angle(current, smoothed);
            Assert.Greater(angle, 0f);
            Assert.Less(angle, 90f);
        }

        [Test]
        public void ApplyModelOffset_AppliesModelRotationCorrections()
        {
            Quaternion world = Quaternion.Euler(0, 45, 0);
            Vector3 offset = new Vector3(0, 90, 0);
            Quaternion result = DirectionCalculator.ApplyModelOffset(world, offset);
            Assert.AreEqual(135f, result.eulerAngles.y, 0.05f);
        }
    }
}
