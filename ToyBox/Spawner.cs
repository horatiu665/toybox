using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class Spawner : MonoBehaviour
{
	public Transform[] spawnWhat;
	public int howMany = 1;
	public float spawnRadius = 10;
	[Range(1, 1.999f)]
	public float variousSizes = 1;
	// Finds the furthest away position compared to all other objects.
	[Tooltip("Finds the furthest away position compared to all other objects.")]
	public bool furthestAway = false;
	// tries to position object away from others, using a max number of iterations of random pos.
	[Tooltip("tries to position object away from others, using a max number of iterations of random pos.")]
	public float minDistanceToOthers = 0;
	public int monteCarloMaxIterations = 50;
	// places objects closer to center, using average of two random positions
	[Tooltip("places objects closer to center, using average of two random positions")]
	public bool distributedRandom = true;
	// Set the seed of the randomizer
	[Tooltip("Set the seed of the randomizer")]
	public bool randomize;
	public int randomSeed = 0;
	// spawns after delay
	[Tooltip("spawns after delay")]
	public float delay = 0;
	public Color editorColor;
	public bool showGizmos = false;
	public Transform parentTo;
	public bool spawnOnStart = false;
	public bool doItNow = false;
	public bool clearChildren = false;

	void Update()
	{
		if (doItNow) {
			doItNow = false;
			StartCoroutine(Spawn());
		}

		if (clearChildren) {
			clearChildren = false;
			while (transform.childCount > 0) {
				DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
			}
		}

	}

	void Randomize(int s)
	{
		Random.seed = s;
	}

	void Start()
	{
		if (Application.isPlaying && spawnOnStart) {
			StartCoroutine(Spawn());
		}
	}

	IEnumerator Spawn()
	{

		if (delay > 0) {
			yield return new WaitForSeconds(delay);
		}

		if (randomize)
			Randomize(randomSeed);

		// spawner
		for (int i = 0; i < howMany; i++) {
			// little more normally distributed random position
			Vector3 pos;

			// count how many times we attempt to set position (Monte Carlo anyone?)
			int iterations = 0;
			// set pos
			do {
				iterations++;
				if (furthestAway) {
					pos = FindRandomAvoidedPosition(GetComponentsInChildren<Transform>().Where(t => t != transform).ToArray());
				} else {
					pos = distributedRandom
						? (Random.insideUnitCircle + Random.insideUnitCircle) / 2 * spawnRadius
						: Random.insideUnitCircle * spawnRadius;
					pos = new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y);
				}

			} while (minDistanceToOthers != 0 && !PositionFarFromAll(pos, minDistanceToOthers) && iterations < monteCarloMaxIterations);

			Transform a = Instantiate(spawnWhat[Random.Range(0, spawnWhat.Length)], pos, Quaternion.identity) as Transform;
			a.localScale *= Random.Range(2 - variousSizes, variousSizes);
			if (parentTo != null) {
				a.parent = parentTo;
			}

		}

	}


	/// <summary>
	/// return the one with the largest distance to the nearest point (calculate all huhuhhuhuhhuhuuh very inefficient
	/// </summary>
	/// <param name="points">which points to avoid</param>
	/// <returns></returns>
	Vector3 FindRandomAvoidedPosition(Transform[] points)
	{
		Vector3 maxPos = Vector3.zero;
		float maxDist = 0;
		for (int i = 0; i < Mathf.Clamp(points.Length, 20, 100); i++) {
			var x = Random.Range(0, 2 * Mathf.PI);
			var dist = Random.Range(0, 1f);
			Vector3 randomPos = transform.position + new Vector3(Mathf.Sin(x), 0, Mathf.Cos(x)) * dist * spawnRadius;
			// get nearest point
			var nearestPoint = float.MaxValue;
			if (points.Length > 0) {
				foreach (var p in points) {
					var distToP = (p.position - randomPos).sqrMagnitude;
					if (nearestPoint > distToP) {
						nearestPoint = distToP;
					}
				}
				if (maxDist < nearestPoint) {
					maxDist = nearestPoint;
					maxPos = randomPos;
				}
			} else {
				return randomPos;
			}
		}
		return maxPos;

	}

	bool PositionFarFromAll(Vector3 pos, float minDistance)
	{
		// get all children
		List<Transform> children = new List<Transform>();
		for (int i = 0; i < transform.childCount; i++) {
			children.Add(transform.GetChild(i));
		}

		// check distance to each child. if one of them is out of bounds, return false
		foreach (var c in children) {
			if ((c.position - pos).sqrMagnitude < minDistance * minDistance) {
				return false;
			}
		}

		return true;
	}

	void OnDrawGizmos()
	{
		if (showGizmos) {
			Gizmos.color = editorColor;
			Gizmos.DrawWireSphere(transform.position, spawnRadius);
		}
	}

	/// <summary>
	/// could be used for finding unique positions for each element, so they are not overlapping terribly. instead, a dirty fix can also be found.
	/// </summary>
	float radiusOfObjects(int numObjects, float circleRadius)
	{
		return circleRadius / Mathf.Sqrt(numObjects);

	}

}
