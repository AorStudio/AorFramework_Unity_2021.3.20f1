using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace YoukiaEngine
{
    public static class EngineIO
    {
        private static IEngineIO _core;
        public static void SetIOCore(IEngineIO core)
        {
            _core = core;
        }

        public static bool IsNull()
        {
            return (_core == null);
        }

        public static bool IsDefaultEngineIO()
        {
            if (_core != null)
            {
                if (_core is DefaultEngineIO)
                {
                    return true;
                }
            }
            return false;
        }

        internal static IEngineIO Instance
        {
            get
            {
                if (_core == null)
                {
                    _core = new DefaultEngineIO();
                }
                return _core;
            }

        }
    }

    public interface IEngineIO
    {
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="extension">FileExtensionType 类，扩展名，例如：.bytes，.png，.asset,.json</param>
        /// <returns></returns>
        bool HasFile(string assetPath, string extension);
        /// <summary>
        /// 加载资源，默认异步
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callBack">加载回调</param>
        /// <param name="isSync">true=同步加载，false=异步加载</param>
        /// <param name="ifReleaseImmediate">主要解决AB模式，true=增加引用计数，false=不增加引用计数可以立即释放</param>
        void LoadAsset(string assetPath, Action<Object, string> callBack = null, bool isSync = false);
        // 卸载
        void UnLoadAsset(string assetPath);
        /// <summary>
        /// 获取Shader
        /// </summary>
        /// <param name="shaderPath"></param>
        /// <param name="call"></param>
        /// <param name="isSync"></param>
        void ShaderFind(string shaderName, Action<Shader> call, bool isSync = false);
        /// <summary>
        /// 获取ComputeShader
        /// </summary>
        /// <param name="shaderPath"></param>
        /// <param name="call"></param>
        /// <param name="isSync"></param>
        void ComputeShaderFind(string shaderName, Action<ComputeShader> call, bool isSync = false);

        // 获取引用计数
        int GetRefCount(string assetPath);
    }
}