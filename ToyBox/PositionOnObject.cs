using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class PositionOnObject : MonoBehaviour
{

	public Transform target;
	[Tooltip("Leave at 0,0,0 to position towards target position. set to global position to position towards direction")]
	public Vector3 direction = new Vector3(0, 0, 0);
	public Transform parentTo;
	public float height = 0;
	public float maxDistance = 500;

	[Tooltip("When this is true, sets rotation to the value below. When false, it sets rotation to normals of the surface applied to, just like Horatiu's pre-Mohawk haircut")]
	public bool setNewRotation = true;
	public Vector3 newRotation;
	[Tooltip("When this is true, sets the y rotation to a random value, for awesome effect.")]
	public bool randomYRotation = true;
	public bool DoItNow = false;

	// Use this for initialization
	void Start()
	{
		if (Application.isPlaying) {
			PositionOn(transform, target, direction, maxDistance, setNewRotation, newRotation, randomYRotation, height, parentTo);
		}
	}

	void Update()
	{
		if (!Application.isPlaying) {
			if (DoItNow) {
				DoItNow = false;
				PositionOn(transform, target, direction, maxDistance, setNewRotation, newRotation, randomYRotation, height, parentTo);
			}
		}
	}

	public static bool PositionOn(Transform transform, Transform target, Vector3 direction, float maxDistance, bool setNewRotation, Vector3 newRotation, bool randomYRotation, float height = 0, Transform parentTo = null)
	{
		Vector3 raycastDir;
		if (direction != Vector3.zero) {
			raycastDir = direction;
		} else {
			if (target != null) {
				raycastDir = target.position - transform.position;
			} else {
				raycastDir = Vector3.down;
			}
		}

		if (Physics.Raycast(transform.position, raycastDir, maxDistance)) {
			// instead of this, find better way to reposition stuff.
			RaycastHit[] rg = Physics.RaycastAll(transform.position, raycastDir, 500);
			if (rg.Any(r => r.transform == target) || target == null) {

				RaycastHit objHit;
				if (target != null) {
					objHit = rg.First(r => r.transform == target);
				} else {
					objHit = rg.OrderBy(rh => (rh.point - transform.position).sqrMagnitude).First();
				}

				if (setNewRotation) {
					transform.rotation = Quaternion.Euler(newRotation);
				} else {
					// rotate so it is looking towards normal of intersection with target
					
					transform.rotation = LookAtWithY(objHit.normal);// *Quaternion.Euler(0, 90, 0);
				}

				if (randomYRotation) {
					transform.Rotate(0, Random.Range(0, 360f), 0);
				}

				if (height != 0) {
					// move over to target
					EaseTo(transform, objHit.point + raycastDir.normalized * height);
				} else {
					EaseTo(transform, objHit.point);
				}
				//transform.position = rg.First(r => r.transform.tag == "Terrain").point;
			} else {
				return false;
			}
		} else {
			return false;
		}

		if (parentTo != null) {
			transform.parent = parentTo;
		}

		return true;
	}

	public static Vector3 GetPositionOnObject(Vector3 origin, Vector3 direction, Transform target = null)
	{
		RaycastHit[] rg = Physics.RaycastAll(origin, direction, 5000);
		if ((target == null && rg.Any())) {
			return rg.First().point;
		} else if (target != null && rg.Any(r => r.transform == target)) {
			return rg.First(r => r.transform == target).point;
		} else {
			return origin;
		}
	}

	// returns rotation which has the -Y axis pointing towards target, like transform.LookAt has the Z axis.
	static Quaternion LookAtWithY(Vector3 up)
	{
		// fwd does not matter as long as we have up
		// we must find fwd perpendicular to up, to use Quaternion.lookrotation
		Vector3 fwd = Quaternion.Euler(90, 0, 0) * up;
		return Quaternion.LookRotation(fwd, up);

	}

	static void EaseTo(Transform transform, Vector3 finalPos)
	{
		//Vector3 initPos = transform.position;

		transform.position = finalPos;

	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(transform.position, transform.position - transform.up);
		Gizmos.DrawSphere(transform.position, 0.05f);
	}

}
