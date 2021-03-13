namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dates back to 2014, Sheepy
    /// 
    /// made by @horatiu665
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Refs")]
        public List<GameObject> prefabsList = new List<GameObject>();
        public Transform spawnParent;
        public List<GameObject> instancesSpawned = new List<GameObject>();

        [Header("Settings")]
        public int howMany = 1;

        public float spawnRadius = 10;

        [Tooltip("Finds the furthest away position compared to all other objects.")]
        public bool furthestAway = false;

        [Tooltip("tries to position object away from others, using a max number of iterations of random pos.")]
        public float minDistanceToOthers = 0;
        public int monteCarloMaxIterations = 50;

        [Tooltip("places objects closer to center, using average of two random positions")]
        public bool distributedRandom = true;

        [Header("Random seed (works best without delays)")]
        // Set the seed of the randomizer
        [Tooltip("Set the seed of the randomizer using UnityEngine.Random.InitState(seed)")]
        public bool useRandomSeed = false;
        public int randomSeed = 1337;

        [Header("Behaviour")]
        [EnumButtons]
        public SpawnTiming whenToSpawn = SpawnTiming.External;
        public enum SpawnTiming
        {
            External,
            OnEnable,
            Start,
        }
        public bool clearBeforeEverySpawn = true;

        public float startDelay = 0f;
        public float delayBetweenItems = 0f;

        [Header("Debug")]
        public bool showGizmos = false;
        public Color gizmoColor = Color.green;

        [DebugButton]
        public void ClearAll()
        {
            for (int i = 0; i < instancesSpawned.Count; i++)
            {
                if (instancesSpawned[i] != null)
                {
                    Destroy(instancesSpawned[i].gameObject);
                }
            }
            instancesSpawned.Clear();
        }

        public GameObject SpawnOne(Vector3 pos)
        {
            var p = prefabsList.Random();

            var s = Instantiate(p, pos, Quaternion.identity, spawnParent);

            instancesSpawned.Add(s);
            return s;
        }

        void InitRandomState(int s)
        {
            Random.InitState(s);
        }

        private void Reset()
        {
            this.spawnParent = transform;
        }

        private void OnEnable()
        {
            if (Application.isPlaying && (whenToSpawn == SpawnTiming.OnEnable))
            {
                StartCoroutine(Spawn());
            }
        }

        void Start()
        {
            if (Application.isPlaying && (whenToSpawn == SpawnTiming.Start))
            {
                StartCoroutine(Spawn());
            }
        }

        [DebugButton]
        public void EditorSpawn()
        {
            StartCoroutine(Spawn());
        }

        IEnumerator Spawn()
        {
            if (startDelay > 0)
            {
                yield return new WaitForSeconds(startDelay);
            }

            if (clearBeforeEverySpawn)
            {
                ClearAll();
            }

            if (useRandomSeed)
                InitRandomState(randomSeed);

            // spawner
            for (int i = 0; i < howMany; i++)
            {
                // little more normally distributed random position
                Vector3 pos;

                // count how many times we attempt to set position (Monte Carlo anyone?)
                int iterations = 0;
                // set pos
                do
                {
                    iterations++;
                    if (furthestAway)
                    {
                        pos = FindRandomAvoidedPosition(instancesSpawned);
                    }
                    else
                    {
                        pos = distributedRandom
                            ? (Random.insideUnitCircle + Random.insideUnitCircle) / 2 * spawnRadius
                            : Random.insideUnitCircle * spawnRadius;
                        pos = new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y);
                    }

                } while (minDistanceToOthers != 0 && !IsPositionFarFromAll(pos, minDistanceToOthers) && iterations < monteCarloMaxIterations);

                var a = SpawnOne(pos);

                // consider randomizing rotation and scale here


                if (delayBetweenItems > 0)
                    yield return new WaitForSeconds(delayBetweenItems);
            }

        }


        /// <summary>
        /// return the one with the largest distance to the nearest point
        /// </summary>
        /// <returns></returns>
        Vector3 FindRandomAvoidedPosition(IEnumerable<GameObject> pointsToAvoid)
        {
            int pointsCount = pointsToAvoid.Count();

            Vector3 maxPos = RandomPositionXZAroundTransform();
            float maxDist = 0;
            for (int i = 0; i < Mathf.Clamp(pointsCount, 0, 100); i++)
            {
                // random on unit circle on xz
                var randomPos = RandomPositionXZAroundTransform();

                // get nearest point
                var nearestPoint = float.MaxValue;
                if (pointsCount > 0)
                {
                    foreach (var p in pointsToAvoid)
                    {
                        var distToP = (p.transform.position - randomPos).sqrMagnitude;
                        if (nearestPoint > distToP)
                        {
                            nearestPoint = distToP;
                        }
                    }
                    if (maxDist < nearestPoint)
                    {
                        maxDist = nearestPoint;
                        maxPos = randomPos;
                    }
                }
                else
                {
                    return randomPos;
                }
            }
            return maxPos;

        }

        private Vector3 RandomPositionXZAroundTransform()
        {
            var unitCircXY = Random.insideUnitCircle;
            Vector3 randomPos = transform.position + new Vector3(unitCircXY.x, 0, unitCircXY.y) * spawnRadius;
            return randomPos;
        }

        private bool IsPositionFarFromAll(Vector3 pos, float minDistance)
        {
            // check distance to each child. if one of them is out of bounds, return false
            foreach (var c in instancesSpawned)
            {
                if ((c.transform.position - pos).sqrMagnitude < minDistance * minDistance)
                {
                    return false;
                }
            }

            return true;
        }

        void OnDrawGizmos()
        {
            if (showGizmos)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }
        }



    }
}