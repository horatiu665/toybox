using System.Collections.Generic;

public static class RandomUtil
{
    public static T Random<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
