using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.LightmapConfigurator
{
    /// <summary>
    /// 此脚本为LightmapConfigurator辅助标记脚本
    /// 标记上此脚本的节点在LightmapConfigurator执行烘焙时将被激活已参与烘焙过程，烘焙结束后将自行恢复到原始的激活状态
    /// </summary>
    public class LCBakeHelperNodeTag : MonoBehaviour
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


