namespace ToyBoxHHH
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class ConeCastExtension
    {
        private static List<RaycastHit> coneCastHitListCache = new List<RaycastHit>();
        private static RaycastHit[] sphereCastResultsCache = new RaycastHit[200];

        public static int ConeCastNonAlloc(Vector3 origin, float maxRadius, Vector3 direction, RaycastHit[] hits, float maxDistance, float coneAngle, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var count = Physics.SphereCastNonAlloc(origin - direction.normalized * maxRadius, maxRadius, direction, sphereCastResultsCache, maxDistance, layerMask, queryTriggerInteraction);
            coneCastHitListCache.Clear();

            var succCount = 0;
            for (int i = 0; i < count; i++)
            {
                var hit = sphereCastResultsCache[i];
                Vector3 hitPoint = hit.point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);
                if (angleToHit < coneAngle)
                {
                    if (succCount < hits.Length)
                    {
                        hits[succCount] = hit;
                        succCount++;
                    }
                    else
                        return succCount;
                }
            }

            return succCount;
        }
    }
}