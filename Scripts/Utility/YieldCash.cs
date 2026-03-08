using System.Collections.Generic;
using UnityEngine;
static class YieldCache
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y) => x == y;
        int IEqualityComparer<float>.GetHashCode(float o) => o.GetHashCode();
    }
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();
    private static readonly Dictionary<float, WaitForSeconds> t = new(new FloatComparer());
    private static readonly Dictionary<float, WaitForSecondsRealtime> r = new(new FloatComparer());
    public static WaitForSeconds WaitForSeconds(float s)
    {
        if (!t.TryGetValue(s, out var w))
            t.Add(s, w = new(s));
        return w;
    }
    public static WaitForSecondsRealtime WaitForSecondsRealTime(float s)
    {
        if (!r.TryGetValue(s, out var w))
            r.Add(s, w = new(s));
        return w;
    }
}