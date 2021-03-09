namespace ToyBox
{
    using UnityEngine;
    using System.Collections;

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

        float initdist;

        Vector3 initScale;

        void Start()
        {
            initScale = transform.localScale;
            initdist = Mathf.Max(EPSILON, (lookAtObj.target.position - transform.position).magnitude);
        }

        // Update is called once per frame
        void Update()
        {
            var curDist = (lookAtObj.target.position - transform.position).magnitude;
            var scale = curDist / initdist;
            var ssss = initScale;
            ssss.z *= scale;
            transform.localScale = ssss;
        }
    }
}