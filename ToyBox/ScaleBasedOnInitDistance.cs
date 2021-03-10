namespace ToyBoxHHH
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Useful when you wanna make a fake rope, or a hand, or something that points to something else.
    /// Make an object that stretches for the desired length until a target.
    /// Requires ref to <see cref="LookAtObj"/>
    /// 
    /// made by @horatiu665
    /// </summary>
    public class ScaleBasedOnInitDistance : MonoBehaviour
    {
        public const float EPSILON = 0.001f;

        [SerializeField]
        private LookAtObj _lookAtObj;
        public LookAtObj lookAtObj
        {
            get
            {
                if (_lookAtObj == null)
                {
                    _lookAtObj = GetComponent<LookAtObj>();
                }
                return _lookAtObj;
            }
        }

        public Transform target => lookAtObj.target;

        float initdist;

        Vector3 initScale;

        void Start()
        {
            initScale = transform.localScale;
            initdist = Mathf.Max(EPSILON, (target.position - transform.position).magnitude);
        }

        void Update()
        {
            var curDist = (target.position - transform.position).magnitude;
            var scale = curDist / initdist;
            var ssss = initScale;
            ssss.z *= scale;
            transform.localScale = ssss;
        }
    }
}