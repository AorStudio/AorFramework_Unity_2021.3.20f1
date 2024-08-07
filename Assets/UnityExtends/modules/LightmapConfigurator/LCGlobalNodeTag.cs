using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator
{
    /// <summary>
    /// 此脚本为LightmapConfigurator辅助标记脚本
    /// 标记上此脚本的节点在LightmapConfigurator执行烘焙时作为公用节点参与烘焙过程。
    /// </summary>
    public class LCGlobalNodeTag : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
#else
        GameObject.Destroy(this);
#endif
        }
    }

}


