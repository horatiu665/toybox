namespace ToyBoxHHH
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// A script that helps you move a parent without moving the children.
    /// Always useful when messing around with transform hierarchies.
    /// Usage: add it to an object in the scene, move it, then disable the script or remove it, to be safe.
    /// 
    /// made by @horatiu665
    /// </summary>
    [ExecuteAlways]
    public class MovePivotWithoutChildren : MonoBehaviour
    {
        public bool executeInEditMode = true;
        public bool executeInPlayMode = true;

        [Space]
        // update is false by default so the object you want to affect doesn't jump around as soon as you put the script on.
        public bool update = false;

        public Vector3 prevPos;

        private void Reset()
        {
            prevPos = transform.position;
        }

        void Update()
        {
            if (update)
                DoIt();

        }

        private void DoIt()
        {
            if (!executeInPlayMode && Application.isPlaying)
                return;

            if (!executeInEditMode && !Application.isPlaying)
                return;

            var delta = transform.position - prevPos;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).position -= delta;
            }

            prevPos = transform.position;

        }

    }
}