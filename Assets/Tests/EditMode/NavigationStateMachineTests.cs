using NUnit.Framework;
using UnityEngine;
using ARFireNavigation.Core;

namespace ARFireNavigation.Tests.EditMode
{
    public class NavigationStateMachineTests
    {
        private GameObject go;
        private NavigationManager navMgr;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("NavManagerTestObject");
            navMgr = go.AddComponent<NavigationManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        [Test]
        public void InitialState_IsWaitingForTracking()
        {
            Assert.AreEqual(NavigationState.WaitingForTracking, navMgr.CurrentState);
        }

        [Test]
        public void LegalStateTransitions_WorkCorrectly()
        {
            navMgr.TransitionState(NavigationState.ReadyToStart);
            Assert.AreEqual(NavigationState.ReadyToStart, navMgr.CurrentState);

            navMgr.TransitionState(NavigationState.Navigating);
            Assert.AreEqual(NavigationState.Navigating, navMgr.CurrentState);

            navMgr.TransitionState(NavigationState.Completed);
            Assert.AreEqual(NavigationState.Completed, navMgr.CurrentState);
        }

        [Test]
        public void TrackingLossAndRecovery_CycleTransitionsCleanly()
        {
            navMgr.TransitionState(NavigationState.Navigating);
            Assert.AreEqual(NavigationState.Navigating, navMgr.CurrentState);

            navMgr.TransitionState(NavigationState.TrackingLost);
            Assert.AreEqual(NavigationState.TrackingLost, navMgr.CurrentState);

            navMgr.TransitionState(NavigationState.Navigating);
            Assert.AreEqual(NavigationState.Navigating, navMgr.CurrentState);
        }

        [Test]
        public void ResetNavigation_CleansState()
        {
            navMgr.TransitionState(NavigationState.Navigating);
            navMgr.ResetNavigation();
            Assert.AreEqual(0, navMgr.CurrentWaypointIndex);
            Assert.AreEqual(0f, navMgr.DistanceToNext);
            Assert.AreEqual(0f, navMgr.RouteProgress);
        }

        [Test]
        public void StateChangedEvent_FiresWithPreviousAndNewStates()
        {
            NavigationState observedOld = NavigationState.Error;
            NavigationState observedNew = NavigationState.Error;

            navMgr.OnStateChanged += (oldS, newS) =>
            {
                observedOld = oldS;
                observedNew = newS;
            };

            navMgr.TransitionState(NavigationState.ReadyToStart);
            Assert.AreEqual(NavigationState.WaitingForTracking, observedOld);
            Assert.AreEqual(NavigationState.ReadyToStart, observedNew);
        }
    }
}
