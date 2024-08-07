using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YoukiaEngine
{
    public class DefaultEngineIO : IEngineIO
    {
        private Dictionary<string, ObjectCount> resourceDic = new Dictionary<string, ObjectCount>();


        /// <summary>
        /// 文件是否存在，增加后缀为参数，如果不给，默认读取所有的，但是很卡
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="extension">扩展名，例如：bytes，png，assets</param>
        /// <returns></returns>
        public bool HasFile(string assetPath, string extension)
        {
#if UNITY_EDITOR
            // editor模式可以判断
            bool isFile = File.Exists(Application.dataPath + "/Resources/" + assetPath + extension);
            return isFile;
#else
            // 其它平台不能判断，默认返回true
            return true;
#endif

        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath"></param>
        public void UnLoadAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            ObjectCount objCount;
            if (resourceDic.TryGetValue(assetPath, out objCount))
            {
                objCount.Count--;
                if (objCount.Count == 0)
                {
                    resourceDic.Remove(assetPath);

                    if (objCount.obj)
                    {
                        if (objCount.obj is TextAsset || objCount.obj is Material || objCount.obj is Texture)
                        {
                            Resources.UnloadAsset(objCount.obj);//res模式下面需要卸载。
                        }
                    }
                    objCount = null;
                }
            }
        }

        /// <summary>
        /// 加载资源，引擎使用的接口
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callBack">加成完成回调</param>
        /// <param name="isSync">是否同步加载？默认同步</param>
        public void LoadAsset(string assetPath, Action<UnityEngine.Object, string> callBack = null, bool isSync = true)
        {
            // 计数需要在异步加载之前，避免被错误卸载
            ObjectCount objCount;
            if (resourceDic.TryGetValue(assetPath, out objCount))
            {
                objCount.Count++;
            }
            else
            {
                objCount = new ObjectCount
                {
                    obj = null,
                    Count = 1,
                };
                resourceDic.Add(assetPath, objCount);
            }

            if (isSync)
            {
                UnityEngine.Object obj = Resources.Load(assetPath);
                if (obj != null)
                {
                    objCount.obj = obj;
                }
                callBack?.Invoke(obj, assetPath);
            }
            else
            {
                ResourceRequest request = Resources.LoadAsync(assetPath);
                request.completed += (AsyncOperation operation) =>
                {
                    if (operation.isDone)
                    {
                        UnityEngine.Object obj = request.asset;
                        if (obj != null)
                        {
                            objCount.obj = obj;
                        }

                        callBack?.Invoke(obj, assetPath);
                    }
                };
            }
        }


        public void ShaderFind(string shaderPath,Action<Shader>action,bool isSync=false)
        {
            Shader shader = Shader.Find(shaderPath);
            action?.Invoke(shader);
        }

        public void ComputeShaderFind(string shaderPath, Action<ComputeShader> action,bool isSync=false)
        {
            UnityEngine.Object obj = Resources.Load("shader/" + shaderPath);
            if (obj != null)
            {
                action?.Invoke(obj as ComputeShader);
            }
            action?.Invoke(null);
        }

        // 获取引用计数
        public int GetRefCount(string assetPath)
        {
            ObjectCount objCount;
            if (resourceDic.TryGetValue(assetPath, out objCount))
            {
                return objCount.Count;
            }
            return 0;
        }

        private class ObjectCount
        {
            // 加载文件缓存
            public UnityEngine.Object obj;
            // 资源引用计数
            public int Count;
        }

    }
}