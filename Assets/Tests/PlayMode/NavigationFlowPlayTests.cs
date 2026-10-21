using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ARFireNavigation.Core;
using ARFireNavigation.Navigation;

namespace ARFireNavigation.Tests.PlayMode
{
    public class NavigationFlowPlayTests
    {
        private GameObject sceneRoot;
        private NavigationManager navManager;
        private WaypointRoute route;
        private GameObject cameraGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            sceneRoot = new GameObject("PlayTestSceneRoot");

            cameraGo = new GameObject("MockCamera");
            cameraGo.transform.SetParent(sceneRoot.transform);
            var cam = cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";

            var routeGo = new GameObject("Route");
            routeGo.transform.SetParent(sceneRoot.transform);
            route = routeGo.AddComponent<WaypointRoute>();

            var customWaypoints = new List<WaypointNode>
            {
                new WaypointNode(new Vector3(0, 0, 0), "Start", false),
                new WaypointNode(new Vector3(0, 0, 4), "Turn Right into East Corridor", false),
                new WaypointNode(new Vector3(3, 0, 4), "Fire Exit B", true)
            };
            route.SetWaypoints(customWaypoints);
            route.DefaultArrivalRadius = 1.0f;

            var navGo = new GameObject("NavManager");
            navGo.transform.SetParent(sceneRoot.transform);
            navManager = navGo.AddComponent<NavigationManager>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (sceneRoot != null) Object.Destroy(sceneRoot);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NavigationFlow_AdvancesWaypointsAndCompletesAtExit()
        {
            navManager.TransitionState(NavigationState.ReadyToStart);
            navManager.StartNavigation();
            Assert.AreEqual(NavigationState.Navigating, navManager.CurrentState);

            // 1. Move mock camera to waypoint 0 (0, 0, 0)
            cameraGo.transform.position = new Vector3(0, 0, 0);
            for (int i = 0; i < 4; i++)
            {
                navManager.UpdateNavigationCalculations();
                yield return null;
            }

            // Waypoint 0 should be confirmed, advancing toward waypoint 1
            Assert.AreEqual(1, navManager.CurrentWaypointIndex);

            // 2. Move mock camera to waypoint 1 (0, 0, 4)
            cameraGo.transform.position = new Vector3(0, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                navManager.UpdateNavigationCalculations();
                yield return null;
            }

            // Waypoint 1 should be reached, advancing toward waypoint 2 (exit)
            Assert.AreEqual(2, navManager.CurrentWaypointIndex);

            // 3. Move mock camera to final exit (3, 0, 4)
            cameraGo.transform.position = new Vector3(3, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                navManager.UpdateNavigationCalculations();
                yield return null;
            }

            // Should complete evacuation navigation!
            Assert.AreEqual(NavigationState.Completed, navManager.CurrentState);
        }

        [UnityTest]
        public IEnumerator NavigationFlow_RestartResetsProgress()
        {
            navManager.TransitionState(NavigationState.ReadyToStart);
            navManager.StartNavigation();

            // 1. Move to waypoint 0
            cameraGo.transform.position = new Vector3(0, 0, 0);
            for (int i = 0; i < 4; i++)
            {
                navManager.UpdateNavigationCalculations();
                yield return null;
            }

            Assert.AreEqual(1, navManager.CurrentWaypointIndex);

            // 2. Move to waypoint 1
            cameraGo.transform.position = new Vector3(0, 0, 4);
            for (int i = 0; i < 4; i++)
            {
                navManager.UpdateNavigationCalculations();
                yield return null;
            }

            Assert.AreEqual(2, navManager.CurrentWaypointIndex);

            // Reset navigation
            navManager.ResetNavigation();
            Assert.AreEqual(0, navManager.CurrentWaypointIndex);
            Assert.AreEqual(NavigationState.WaitingForTracking, navManager.CurrentState);
        }
    }
}
