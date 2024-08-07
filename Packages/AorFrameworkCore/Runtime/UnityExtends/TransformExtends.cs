#if FRAMEWORKDEF
#else

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AORCore
{

    /// <summary>
    /// Transform扩展方法
    /// 
    /// Author  :   Arotion
    /// Update  :   2024-08-02
    /// 
    /// </summary>
    public static class TransformExtends
    {

        #region HierarchyPath

        /// <summary>
        /// 获取对象在Hierarchy中的节点路径
        /// </summary>
        public static string GetHierarchyPath(this Transform tran)
        {
            return _getHierarchPathLoop(tran, null, string.Empty);
        }

        /// <summary>
        /// 获取对象在Hierarchy中的节点路径
        /// </summary>
        /// <param name="root">根节点对象</param>
        public static string GetHierarchyPath(this Transform tran, Transform root)
        {
            return _getHierarchPathLoop(tran, root, string.Empty);
        }

        private static string _getHierarchPathLoop(Transform t, Transform root, string path)
        {
            if (string.IsNullOrEmpty(path))
                path = t.gameObject.name;
            else
                path = t.gameObject.name + "/" + path;

            if (t.parent != root)
                return _getHierarchPathLoop(t.parent, root, path);
            else
                return path;
        }

        #endregion

        #region Layer

        public static void SetLayer(this Transform obj, int layer, bool inculedChildern = true)
        {
            obj.gameObject.SetLayer(layer, inculedChildern);
        }

        #endregion

        #region Interface

        public static T GetInterface<T>(this Transform tran) where T : class
        {
            return tran.gameObject.GetInterface<T>();
        }

        public static T[] GetInterfaces<T>(this Transform tran) where T : class
        {
            return tran.gameObject.GetInterfaces<T>();
        }

        public static void GetInterfaces<T>(this Transform tran, List<T> resultList) where T : class
        {
            tran.gameObject.GetInterfaces<T>(resultList);
        }

        public static T GetInterfaceInChlidren<T>(this Transform tran, bool includeInactive = false) where T : class
        {
            return tran.gameObject.GetInterfaceInChlidren<T>(includeInactive);
        }

        public static T GetInterfaceInParent<T>(this Transform tran, bool includeInactive = false) where T : class
        {
            return tran.gameObject.GetInterfaceInParent<T>(includeInactive);
        }

        public static T[] GetInterfacesInChlidren<T>(this Transform tran, bool includeInactive = false) where T : class
        {
            return tran.gameObject.GetInterfacesInChlidren<T>(includeInactive);
        }

        public static void GetInterfacesInChlidren<T>(this Transform tran, List<T> resultList, bool includeInactive = false) where T : class
        {
            tran.gameObject.GetInterfacesInChlidren<T>(resultList, includeInactive);
        }

        public static T[] GetInterfacesInParent<T>(this Transform tran, bool includeInactive = false) where T : class
        {
            return tran.gameObject.GetInterfacesInParent<T>(includeInactive);
        }

        public static void GetInterfacesInParent<T>(this Transform tran, List<T> resultList, bool includeInactive = false) where T : class
        {
            tran.gameObject.GetInterfacesInParent<T>(resultList, includeInactive);
        }

        #endregion

        #region Component

        /// <summary>
        /// 查找或者创建Component(当前Component在当前节点对象找不到,则在当前对象上创建Component)
        /// </summary>
        public static T GetOrCreateComponent<T>(this Transform tran) where T : Component
        {
            return tran.gameObject.GetOrCreateComponent<T>();
        }


        /// <summary>
        /// 向上级节点查找Component, 默认不包含自身节点
        /// </summary>
        public static T FindComponentInParent<T>(this Transform tran, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            return tran.gameObject.FindComponentInParent<T>(incudeSelf, incudeInactive);
        }

        /// <summary>
        ///向下级节点查找Component, 默认不包含自身节点
        /// </summary>
        public static T FindComponentInChildren<T>(this Transform tran, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            return tran.gameObject.FindComponentInChildren<T>(incudeSelf, incudeInactive);
        }

        /// <summary>
        /// 向上级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static T[] FindComponentsInParent<T>(this Transform tran, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            return tran.gameObject.FindComponentsInParent<T>(incudeSelf, incudeInactive);
        }

        /// <summary>
        /// 向上级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static void FindComponentsInParent<T>(this Transform tran, List<T> result, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            tran.gameObject.FindComponentsInParent<T>(result, incudeSelf, incudeInactive);
        }

        /// <summary>
        ///向下级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static void FindComponentsInChildren<T>(this Transform tran, List<T> result, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            tran.gameObject.FindComponentsInChildren<T>(result, incudeSelf, incudeInactive);
        }

        /// <summary>
        ///向下级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static T[] FindComponentsInChildren<T>(this Transform tran, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            return tran.gameObject.FindComponentsInChildren<T>(incudeSelf, incudeInactive);
        }

        #endregion

    }

}

#endif