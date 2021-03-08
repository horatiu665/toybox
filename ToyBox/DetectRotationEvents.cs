using UnityEngine;
using System.Collections;

/// <summary>
/// PUT THIS ON AN OBJECT TO DETECT ITS ROTATION and if it flips.
/// </summary>
public class DetectRotationEvents : MonoBehaviour
{

    public enum Axes
    {
        x, y, z
    }


    Quaternion oldRotation;
    Vector3 oldDeltaEuler;

    /// <summary>
    /// sums up the consecutive delta angles with the same sign, to try and calculate a flip
    /// </summary>
    Vector3 flipAngleSum;

    public event System.Action<Axes> Flipped;

    // Use this for initialization
    void Start()
    {
        // test that it works
        //Flipped += (axis) => { print("Flipped! " + axis); };
    }

    // Update is called once per frame
    void Update()
    {
        // fire flip event!
        if (Mathf.Abs(flipAngleSum.x) > 360) {
            if (Flipped != null) {
                Flipped(Axes.x);
                flipAngleSum.x = 0;
            }
        }
        if (Mathf.Abs(flipAngleSum.y) > 360) {
            if (Flipped != null) {
                Flipped(Axes.y);
                flipAngleSum.y = 0;
            }
        }
        if (Mathf.Abs(flipAngleSum.z) > 360) {
            if (Flipped != null) {
                Flipped(Axes.z);
                flipAngleSum.z = 0;
            }
        }

        var deltaRotation = Quaternion.Inverse(oldRotation) * transform.rotation;
        var deltaEuler = deltaRotation.eulerAngles;
        oldRotation = transform.rotation;

        if (deltaEuler.x > 180) {
            deltaEuler.x -= 360;
        }
        if (deltaEuler.y > 180) {
            deltaEuler.y -= 360;
        }
        if (deltaEuler.z > 180) {
            deltaEuler.z -= 360;
        }
        if (deltaEuler.x < -180) {
            deltaEuler.x += 360;
        }
        if (deltaEuler.y < -180) {
            deltaEuler.y += 360;
        }
        if (deltaEuler.z < -180) {
            deltaEuler.z += 360;
        }


        if (deltaEuler.x * oldDeltaEuler.x > 0) {
            // deltaeuler.x and old.x have the same sign, add to the counter.
            flipAngleSum.x += deltaEuler.x;
        } else if (deltaEuler.x * oldDeltaEuler.x < 0) {
            // angle changed. we can start counting the spin here.
            flipAngleSum.x = 0;
        }

        if (deltaEuler.y * oldDeltaEuler.y > 0) {
            // deltaeuler.y and old.y have the same sign, add to the counter.
            flipAngleSum.y += deltaEuler.y;
        } else if (deltaEuler.y * oldDeltaEuler.y < 0) {
            // angle changed. we can start counting the spin here.
            flipAngleSum.y = 0;
        }

        if (deltaEuler.z * oldDeltaEuler.z > 0) {
            // deltaeuler.z and old.z have the same sign, add to the counter.
            flipAngleSum.z += deltaEuler.z;
        } else if (deltaEuler.z * oldDeltaEuler.z < 0) {
            // angle changed. we can start counting the spin here.
            flipAngleSum.z = 0;
        }

        if (deltaEuler.x != 0) {
            oldDeltaEuler.x = deltaEuler.x;
        }
        if (deltaEuler.y != 0) {
            oldDeltaEuler.y = deltaEuler.y;
        }
        if (deltaEuler.z != 0) {
            oldDeltaEuler.z = deltaEuler.z;
        }
    }
}
