using System.Collections.Generic;
using UnityEngine;
using ARFireNavigation.Utilities;

namespace ARFireNavigation.Navigation
{
    public class ArrowController : MonoBehaviour
    {
        [Header("Configuration & Prefab")]
        [SerializeField] private NavigationConfiguration config;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Material arrowMaterial;
        [SerializeField] private Material arrowGlowMaterial;

        [Header("Pooling")]
        [SerializeField] private int initialPoolSize = 25;
        [SerializeField] private Transform arrowContainer;

        private readonly List<GameObject> arrowPool = new List<GameObject>();
        private readonly List<GameObject> activeArrows = new List<GameObject>();
        private Mesh sharedProceduralMesh;

        private void Awake()
        {
            if (arrowContainer == null)
            {
                var containerGo = new GameObject("ArrowPoolContainer");
                containerGo.transform.SetParent(transform);
                arrowContainer = containerGo.transform;
            }

            InitializePool();
        }

        public void SetConfiguration(NavigationConfiguration newConfig)
        {
            config = newConfig;
        }

        public void SetArrowPrefab(GameObject prefab)
        {
            arrowPrefab = prefab;
            ClearArrows();
            InitializePool();
        }

        private void InitializePool()
        {
            foreach (var a in arrowPool)
            {
                if (a != null) Destroy(a);
            }
            arrowPool.Clear();
            activeArrows.Clear();

            int poolTarget = config != null ? config.MaxActiveArrows : initialPoolSize;
            for (int i = 0; i < poolTarget; i++)
            {
                GameObject arrow = CreateArrowInstance();
                arrow.SetActive(false);
                arrowPool.Add(arrow);
            }
        }

        private GameObject CreateArrowInstance()
        {
            GameObject arrowGo;
            if (arrowPrefab != null)
            {
                arrowGo = Instantiate(arrowPrefab, arrowContainer);
            }
            else
            {
                arrowGo = new GameObject("NavigationArrow_Instance");
                arrowGo.transform.SetParent(arrowContainer);

                var mf = arrowGo.AddComponent<MeshFilter>();
                if (sharedProceduralMesh == null)
                {
                    sharedProceduralMesh = ArrowMeshGenerator.CreateArrowMesh();
                }
                mf.sharedMesh = sharedProceduralMesh;

                var mr = arrowGo.AddComponent<MeshRenderer>();
                if (arrowMaterial != null)
                {
                    mr.sharedMaterial = arrowMaterial;
                }
            }

            Vector3 scale = config != null ? config.ArrowScale : new Vector3(0.45f, 0.45f, 0.45f);
            arrowGo.transform.localScale = scale;
            return arrowGo;
        }

        public void UpdateArrows(Vector3 userPosition, int currentWaypointIndex, WaypointRoute route)
        {
            if (route == null || currentWaypointIndex < 0 || currentWaypointIndex >= route.WaypointCount)
            {
                ClearArrows();
                return;
            }

            ClearArrows();

            float spacing = config != null ? config.ArrowSpacing : 1.0f;
            float heightOffset = config != null ? config.ArrowHeightOffset : 0.05f;
            Vector3 modelOffset = config != null ? config.ModelForwardRotationOffset : Vector3.zero;
            int maxArrows = config != null ? config.MaxActiveArrows : initialPoolSize;

            int arrowIdx = 0;
            Vector3 startPos = userPosition;

            for (int seg = currentWaypointIndex; seg < route.WaypointCount && arrowIdx < maxArrows; seg++)
            {
                var wp = route.GetWaypoint(seg);
                if (wp == null) break;

                Vector3 targetPos = wp.position;
                Vector3 segmentDir = DirectionCalculator.CalculateDirection(startPos, targetPos, true);
                float segmentDist = DirectionCalculator.CalculateDistance(startPos, targetPos, true);

                if (segmentDist <= 0.2f)
                {
                    startPos = targetPos;
                    continue;
                }

                Quaternion baseRot = Quaternion.LookRotation(segmentDir, Vector3.up);
                Quaternion finalRot = DirectionCalculator.ApplyModelOffset(baseRot, modelOffset);

                float placedDist = spacing * 0.5f;
                while (placedDist < segmentDist && arrowIdx < maxArrows)
                {
                    Vector3 arrowPos = Vector3.Lerp(startPos, targetPos, placedDist / segmentDist);
                    arrowPos.y = (seg == currentWaypointIndex ? userPosition.y : targetPos.y) + heightOffset;

                    GameObject arrowObj = GetPooledArrow(arrowIdx);
                    if (arrowObj != null)
                    {
                        arrowObj.transform.position = arrowPos;
                        arrowObj.transform.rotation = finalRot;
                        arrowObj.SetActive(true);
                        activeArrows.Add(arrowObj);
                        arrowIdx++;
                    }

                    placedDist += spacing;
                }

                startPos = targetPos;
            }
        }

        private GameObject GetPooledArrow(int index)
        {
            if (index < arrowPool.Count)
            {
                return arrowPool[index];
            }

            GameObject newArrow = CreateArrowInstance();
            arrowPool.Add(newArrow);
            return newArrow;
        }

        public void ClearArrows()
        {
            for (int i = 0; i < activeArrows.Count; i++)
            {
                if (activeArrows[i] != null)
                {
                    activeArrows[i].SetActive(false);
                }
            }
            activeArrows.Clear();
        }

        public void SetArrowsVisible(bool visible)
        {
            for (int i = 0; i < activeArrows.Count; i++)
            {
                if (activeArrows[i] != null)
                {
                    activeArrows[i].SetActive(visible);
                }
            }
        }

        private void Update()
        {
            if (activeArrows.Count == 0) return;

            // Subtle pulsing visual cue (sine bob and slight scale modulation)
            float pulse = Mathf.Sin(Time.time * 4f) * 0.05f;
            Vector3 baseScale = config != null ? config.ArrowScale : new Vector3(0.45f, 0.45f, 0.45f);
            Vector3 modScale = baseScale * (1f + pulse);

            for (int i = 0; i < activeArrows.Count; i++)
            {
                if (activeArrows[i] != null && activeArrows[i].activeSelf)
                {
                    activeArrows[i].transform.localScale = modScale;
                }
            }
        }
    }
}
