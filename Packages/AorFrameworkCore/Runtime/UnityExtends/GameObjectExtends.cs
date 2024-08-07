#if FRAMEWORKDEF
#else

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AORCore
{

    /// <summary>
    /// GameObject扩展方法
    /// 
    /// Author  :   Arotion
    /// Update  :   2024-08-02
    /// 
    /// </summary>
    public static class GameObjectExtends
    {

        #region Dispose

        /// <summary>
        /// 销毁GameObject对象
        /// </summary>
        /// <param name="inculedChildren">是否包含所有子节点</param>
        public static void Dispose(this GameObject obj, bool inculedChildren = true)
        {
            if(inculedChildren)
            {
                Transform[] transforms = obj.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    transform.gameObject.DestroySelf();
                }
            }
            obj.DestroySelf();
        }
        /// <summary>
        /// 销毁GameObject自身对象
        /// </summary>
        private static void DestroySelf(this GameObject obj) 
        {
            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }

        #endregion

        #region Layer

        public static void SetLayer(this GameObject obj, int layer, bool inculedChildern = true)
        {
            if (inculedChildern)
            {
                Transform[] transforms = obj.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                {
                    transform.gameObject.layer = layer;
                }
            }
            else
            {
                obj.layer = layer;
            }
        }

        #endregion

        #region Interface 

        public static T GetInterface<T>(this GameObject obj) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return null;
            }

            Component[] cp = obj.GetComponents<Component>();
            int i, length = cp.Length;
            for (i = 0; i < length; i++)
            {
                if (cp[i] is T)
                {
                    T t = cp[i] as T;
                    return t;
                }
            }
            return null;
        }

        public static T[] GetInterfaces<T>(this GameObject obj) where T : class
        {
            List<T> list = new List<T>();
            obj.GetInterfaces(list);
            return list.ToArray();
        }

        public static void GetInterfaces<T>(this GameObject obj, List<T> resultList) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return;
            }

            Component[] cp = obj.GetComponents<Component>();
            if (cp != null && cp.Length > 0)
            {
                int i, len = cp.Length;
                for (i = 0; i < len; i++)
                {
                    if (cp[i] is T)
                    {
                        T a = cp[i] as T;
                        resultList.Add(a);
                    }
                }
            }
        }

        public static T GetInterfaceInChlidren<T>(this GameObject obj, bool includeInactive = false) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return null;
            }

            Component[] cp = obj.GetComponentsInChildren<Component>(includeInactive);
            int i, length = cp.Length;
            for (i = 0; i < length; i++)
            {
                if (cp[i] is T)
                {
                    T t = cp[i] as T;
                    return t;
                }
            }
            return null;
        }

        public static T GetInterfaceInParent<T>(this GameObject obj, bool includeInactive = false) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return null;
            }

            Component[] cp = obj.GetComponentsInParent<Component>(includeInactive);
            if (cp != null && cp.Length > 0)
            {
                int i, len = cp.Length;
                for (i = 0; i < len; i++)
                {
                    if (cp[i] is T)
                    {
                        T t = cp[i] as T;
                        return t;
                    }
                }
            }
            return null;
        }

        public static T[] GetInterfacesInChlidren<T>(this GameObject obj, bool includeInactive = false) where T : class
        {
            List<T> list = new List<T>();
            obj.GetInterfacesInChlidren(list, includeInactive);
            return list.ToArray();
        }

        public static void GetInterfacesInChlidren<T>(this GameObject obj, List<T> resultList, bool includeInactive = false) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return;
            }

            Component[] cp = obj.GetComponentsInChildren<Component>(includeInactive);
            if (cp != null && cp.Length > 0)
            {
                int i, len = cp.Length;
                for (i = 0; i < len; i++)
                {
                    if (cp[i] is T)
                    {
                        T a = cp[i] as T;
                        resultList.Add(a);
                    }
                }
            }
        }

        public static T[] GetInterfacesInParent<T>(this GameObject obj, bool includeInactive = false) where T : class
        {
            List<T> list = new List<T>();
            obj.GetInterfacesInParent(list, includeInactive);
            return list.ToArray();
        }

        public static void GetInterfacesInParent<T>(this GameObject obj, List<T> resultList, bool includeInactive = false) where T : class
        {
            if (!typeof(T).IsInterface)
            {
                return;
            }

            Component[] cp = obj.GetComponentsInParent<Component>(includeInactive);
            if (cp != null && cp.Length > 0)
            {
                int i, len = cp.Length;
                for (i = 0; i < len; i++)
                {
                    if (cp[i] is T)
                    {
                        T a = cp[i] as T;
                        resultList.Add(a);
                    }
                }
            }
        }

        #endregion

        #region Component

        /// <summary>
        /// 查找或者创建Component(当前Component在当前节点对象找不到,则在当前对象上创建Component)
        /// </summary>
        public static T GetOrCreateComponent<T>(this GameObject obj) where T : Component
        {
            T cp = obj.GetComponent<T>();
            if (!cp) cp = obj.AddComponent<T>();
            return cp;
        }

        /// <summary>
        /// 向上级节点查找Component, 默认不包含自身节点
        /// </summary>
        public static T FindComponentInParent<T>(this GameObject obj, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            if (incudeSelf)
                return obj.GetComponentInParent<T>(incudeInactive);
            if (obj.transform.parent)
                return obj.transform.parent.gameObject.GetComponentInParent<T>(incudeInactive);
            return null;
        }

        /// <summary>
        ///向下级节点查找Component, 默认不包含自身节点
        /// </summary>
        public static T FindComponentInChildren<T>(this GameObject obj, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            if (incudeSelf)
                return obj.GetComponentInChildren<T>(incudeInactive);

            int count = obj.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform sub = obj.transform.GetChild(i);
                T subComponet = sub.GetComponentInChildren<T>(incudeInactive);
                if(subComponet)
                    return subComponet;
            }
            return null;
        }

        /// <summary>
        /// 向上级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static T[] FindComponentsInParent<T>(this GameObject obj, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            if (incudeSelf)
                return obj.GetComponentsInParent<T>(incudeInactive);
            if (obj.transform.parent)
                return obj.transform.parent.gameObject.GetComponentsInParent<T>(incudeInactive);
            return null;
        }

        /// <summary>
        /// 向上级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static void FindComponentsInParent<T>(this GameObject obj, List<T> result, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            if (incudeSelf)
                obj.GetComponentsInParent<T>(incudeInactive, result);
            else if (obj.transform.parent)
                obj.transform.parent.gameObject.GetComponentsInParent<T>(incudeInactive, result);
        }

        /// <summary>
        ///向下级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static void FindComponentsInChildren<T>(this GameObject obj, List<T> result, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            if (incudeSelf)
                obj.GetComponentsInChildren<T>(incudeInactive, result);
            else
            {
                int count = obj.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform sub = obj.transform.GetChild(i);
                    sub.gameObject.GetComponentsInChildren(incudeInactive, result);
                }
            }
        }

        /// <summary>
        ///向下级节点查找Components, 默认不包含自身节点
        /// </summary>
        public static T[] FindComponentsInChildren<T>(this GameObject obj, bool incudeSelf = false, bool incudeInactive = false) where T : Component
        {
            List<T> list = new List<T>();
            obj.FindComponentsInChildren<T>(list, incudeSelf, incudeInactive);
            return list.ToArray();
        }

        #endregion

    }

}

#endif