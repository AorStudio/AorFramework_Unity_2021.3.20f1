using System;
using System.Collections.Generic;
using UnityEngine;

namespace NodeGraph
{
    public interface IGameObjectProcess
    {
        /// <summary>
        /// 重置IPrefabProcess以便于初始化一系列标记/变量
        /// </summary>
        void Reset();

        /// <summary>
        /// 处理结果描述
        /// </summary>
        string ResultInfoDescribe();

        /// <summary>
        /// NodeGraph GameObject对象自定义处理方法
        /// </summary>
        /// <param name="ins">输入的预制体</param>
        /// <returns>返回true为通过Process</returns>
        bool GameObjectProcess(GameObject ins, ref List<string> ResultInfoList);
    }
}
