using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ARFireNavigation.Navigation;

namespace ARFireNavigation.Tests.EditMode
{
    public class WaypointRouteTests
    {
        private GameObject testGo;
        private WaypointRoute route;

        [SetUp]
        public void SetUp()
        {
            testGo = new GameObject("TestRouteObject");
            route = testGo.AddComponent<WaypointRoute>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGo != null) Object.DestroyImmediate(testGo);
        }

        [Test]
        public void DemonstrationRoute_HasAtLeastFiveWaypointsAndValidExit()
        {
            route.SetupDemonstrationEmergencyRoute();
            Assert.GreaterOrEqual(route.WaypointCount, 5, "Demonstration route must contain at least 5 waypoints.");
            
            bool valid = route.ValidateRoute(out string err);
            Assert.IsTrue(valid, $"Demonstration route should be valid: {err}");

            var lastWp = route.GetWaypoint(route.WaypointCount - 1);
            Assert.IsNotNull(lastWp);
            Assert.IsTrue(lastWp.isExit, "Last waypoint must be designated as the emergency exit.");
        }

        [Test]
        public void ValidateRoute_RejectsInsufficientWaypoints()
        {
            route.SetWaypoints(new List<WaypointNode> { new WaypointNode(Vector3.zero, "Only One", false) });
            bool valid = route.ValidateRoute(out string err);
            Assert.IsFalse(valid, "Route with only 1 waypoint should fail validation.");
            Assert.IsTrue(err.Contains("at least 2 valid waypoints"), "Error message should mention minimum waypoint count.");
        }

        [Test]
        public void ValidateRoute_RejectsNearlyCoincidentWaypoints()
        {
            var nodes = new List<WaypointNode>
            {
                new WaypointNode(Vector3.zero, "Start", false),
                new WaypointNode(new Vector3(0.01f, 0f, 0f), "Way too close", false),
                new WaypointNode(new Vector3(5f, 0f, 0f), "Exit", true)
            };
            route.SetWaypoints(nodes);
            bool valid = route.ValidateRoute(out string err);
            Assert.IsFalse(valid, "Adjacent waypoints under 0.1m should fail validation.");
            Assert.IsTrue(err.Contains("too close"), "Error message should mention spacing.");
        }

        [Test]
        public void GetTotalRouteDistance_CalculatesAccurately()
        {
            var nodes = new List<WaypointNode>
            {
                new WaypointNode(new Vector3(0f, 0f, 0f), "A", false),
                new WaypointNode(new Vector3(0f, 0f, 10f), "B", false),
                new WaypointNode(new Vector3(5f, 0f, 10f), "C", true)
            };
            route.SetWaypoints(nodes);
            float total = route.GetTotalRouteDistance();
            Assert.AreEqual(15f, total, 0.001f, "Total route distance should be 10m + 5m = 15m.");
        }

        [Test]
        public void GetProgress_CalculatesCorrectPercentage()
        {
            var nodes = new List<WaypointNode>
            {
                new WaypointNode(new Vector3(0f, 0f, 0f), "A", false),
                new WaypointNode(new Vector3(0f, 0f, 10f), "B", false),
                new WaypointNode(new Vector3(0f, 0f, 20f), "C", true)
            };
            route.SetWaypoints(nodes);

            // User at start (0, 0, 0), target index 1
            float progStart = route.GetProgress(new Vector3(0f, 0f, 0f), 1);
            Assert.AreEqual(0.0f, progStart, 0.05f);

            // User midway to waypoint 1 (0, 0, 5)
            float progMid = route.GetProgress(new Vector3(0f, 0f, 5f), 1);
            Assert.AreEqual(0.25f, progMid, 0.05f);

            // User at waypoint 1 (0, 0, 10), target index 2
            float progWp1 = route.GetProgress(new Vector3(0f, 0f, 10f), 2);
            Assert.AreEqual(0.5f, progWp1, 0.05f);
        }

        [Test]
        public void IsAtWaypoint_RespectsArrivalRadius()
        {
            var nodes = new List<WaypointNode>
            {
                new WaypointNode(new Vector3(0f, 0f, 0f), "A", false),
                new WaypointNode(new Vector3(10f, 0f, 0f), "B", false)
            };
            route.SetWaypoints(nodes);
            route.DefaultArrivalRadius = 1.2f;

            Assert.IsTrue(route.IsAtWaypoint(new Vector3(9.5f, 0f, 0f), 1));
            Assert.IsTrue(route.IsAtWaypoint(new Vector3(10.8f, 0f, 0f), 1));
            Assert.IsFalse(route.IsAtWaypoint(new Vector3(8.0f, 0f, 0f), 1));
        }
    }
}
