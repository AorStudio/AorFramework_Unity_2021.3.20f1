using System;
using UnityEngine;

public static class BoundsExtends
{

    /// <summary>
    /// 重置Bounds (设置无限大的bounds)
    /// </summary>
    /// <param name="bounds"></param>
    public static void Reset(this Bounds bounds)
    {
        bounds.SetMinMax(Vector3.positiveInfinity, Vector3.negativeInfinity);
    }

}
