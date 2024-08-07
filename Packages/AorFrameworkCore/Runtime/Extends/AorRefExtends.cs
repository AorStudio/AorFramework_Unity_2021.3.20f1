#pragma warning disable
#if FRAMEWORKDEF
#else

using System;
using System.Collections.Generic;
using System.Reflection;

namespace AORCore
{

    /// <summary>
    /// 常用反射Extends;
    /// </summary>
    /// Last Update : 4-1-2021
    public static class AorRefExtends
    {

        //缓存
        private static Dictionary<Type, Dictionary<string, FieldInfo>> __FieldsCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static Dictionary<Type, Dictionary<string, MethodInfo>> __MethodInfoCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> __PropertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        //tmp
        private static Dictionary<string, FieldInfo> __fieldCacheTmp;
        private static Dictionary<string, MethodInfo> __methodCacheTmp;
        private static Dictionary<string, PropertyInfo> __propertyCacheTmp;

        // -- Field get
        #region Field Get

        private static object getFieldLoop(ref object obj, ref string fieldName, ref BindingFlags inst, ref BindingFlags area, ref Type t, ref FieldInfo fieldInfo)
        {
            if(__FieldsCache.TryGetValue(t, out __fieldCacheTmp))
            {
                if(__fieldCacheTmp.TryGetValue(fieldName, out fieldInfo))
                {
                    return fieldInfo.GetValue(obj);
                }
                else
                {
                    fieldInfo = t.GetField(fieldName, inst | area | BindingFlags.GetField);
                    if(fieldInfo == null)
                    {
                        if(t.BaseType == null)
                            return null;
                        t = t.BaseType;
                        return getFieldLoop(ref obj, ref fieldName, ref inst, ref area, ref t, ref fieldInfo);
                    }
                    else
                    {
                        __FieldsCache[t].Add(fieldName, fieldInfo);
                        return fieldInfo.GetValue(obj);
                    }
                }

            }
            else
            {
                fieldInfo = t.GetField(fieldName, inst | area | BindingFlags.GetField);
                if(fieldInfo == null)
                {
                    if(t.BaseType == null)
                        return null;
                    t = t.BaseType;
                    return getFieldLoop(ref obj, ref fieldName, ref inst, ref area, ref t, ref fieldInfo);
                }
                else
                {
                    __FieldsCache.Add(t, new Dictionary<string, FieldInfo>());
                    __FieldsCache[t].Add(fieldName, fieldInfo);
                    return fieldInfo.GetValue(obj);
                }
            }
        }

        private static object getField(ref object obj, ref string fieldName, BindingFlags inst, BindingFlags area)
        {
            Type t = obj.GetType();
            FieldInfo fieldInfo = null;
            return getFieldLoop(ref obj, ref fieldName, ref inst, ref area, ref t, ref fieldInfo);
        }

        //---

        public static object GetNonPublicField(this object obj, string fieldName)
        {
            return getField(ref obj, ref fieldName, BindingFlags.Instance, BindingFlags.NonPublic);
        }

        public static object GetPublicField(this object obj, string fieldName)
        {
            return getField(ref obj, ref fieldName, BindingFlags.Instance, BindingFlags.Public);
        }

        //未验证
        public static object GetNonPublicStaticField(this object obj, string fieldName)
        {
            return getField(ref obj, ref fieldName, BindingFlags.Static, BindingFlags.NonPublic);
        }

        //未验证
        public static object GetPublicStaticField(this object obj, string fieldName)
        {
            return getField(ref obj, ref fieldName, BindingFlags.Static, BindingFlags.Public);
        }

        #endregion

        // -- Field set
        #region Field Set

        private static bool setFieldLoop(ref object obj, ref string fieldName, ref object value, ref BindingFlags inst, ref BindingFlags area, ref Type t, ref FieldInfo fieldInfo)
        {
            if(__FieldsCache.TryGetValue(t, out __fieldCacheTmp))
            {
                if(__fieldCacheTmp.TryGetValue(fieldName, out fieldInfo))
                {
                    fieldInfo.SetValue(obj, value);
                    return true;
                }
                else
                {
                    fieldInfo = t.GetField(fieldName, inst | area | BindingFlags.GetField);
                    if(fieldInfo == null)
                    {
                        if(t.BaseType == null)
                            return false;
                        t = t.BaseType;
                        return setFieldLoop(ref obj, ref fieldName, ref value, ref inst, ref area, ref t, ref fieldInfo);
                    }
                    else
                    {
                        __FieldsCache[t].Add(fieldName, fieldInfo);
                        fieldInfo.SetValue(obj, value);
                        return true;
                    }
                }

            }
            else
            {
                fieldInfo = t.GetField(fieldName, inst | area | BindingFlags.GetField);
                if(fieldInfo == null)
                {
                    if(t.BaseType == null)
                        return false;
                    t = t.BaseType;
                    return setFieldLoop(ref obj, ref fieldName, ref value, ref inst, ref area, ref t, ref fieldInfo);
                }
                else
                {
                    __FieldsCache.Add(t, new Dictionary<string, FieldInfo>());
                    __FieldsCache[t].Add(fieldName, fieldInfo);
                    fieldInfo.SetValue(obj, value);
                    return true;
                }
            }
        }

        private static bool setField(ref object obj, ref string fieldName, ref object value, BindingFlags inst, BindingFlags area)
        {
            Type t = obj.GetType();
            FieldInfo fieldInfo = null;
            return setFieldLoop(ref obj, ref fieldName, ref value, ref inst, ref area, ref t, ref fieldInfo);
        }

        public static bool SetNonPublicField(this object obj, string fieldName, object value)
        {
            return setField(ref obj, ref fieldName, ref value, BindingFlags.Instance, BindingFlags.NonPublic);
        }

        public static bool SetPublicField(this object obj, string fieldName, object value)
        {
            return setField(ref obj, ref fieldName, ref value, BindingFlags.Instance, BindingFlags.Public);
        }

        //未验证
        public static bool SetNonPublicStaticField(this object obj, string fieldName, object value)
        {
            return setField(ref obj, ref fieldName, ref value, BindingFlags.Static, BindingFlags.NonPublic);
        }

        //未验证
        public static bool SetPublicStaticField(this object obj, string fieldName, object value)
        {
            return setField(ref obj, ref fieldName, ref value, BindingFlags.Static, BindingFlags.Public);
        }

        #endregion

        // -- InvokeMethod
        #region InvokeMethod

        private static object invokeMethodLoop(ref object obj, ref string methodName, ref object[] parameters, ref BindingFlags inst, ref BindingFlags area, ref Type t, ref MethodInfo methodInfo)
        {
            if(__MethodInfoCache.TryGetValue(t, out __methodCacheTmp))
            {
                if(__methodCacheTmp.TryGetValue(methodName, out methodInfo))
                {
                    return methodInfo.Invoke(obj, parameters);
                }
                else
                {
                    methodInfo = t.GetMethod(methodName, inst | area | BindingFlags.InvokeMethod);
                    if(methodInfo == null)
                    {
                        if(t.BaseType == null)
                            return null;
                        t = t.BaseType;
                        return invokeMethodLoop(ref obj, ref methodName,ref parameters, ref inst, ref area, ref t, ref methodInfo);
                    }
                    else
                    {
                        __MethodInfoCache[t].Add(methodName, methodInfo);
                        return methodInfo.Invoke(obj, parameters);
                    }
                }

            }
            else
            {
                methodInfo = t.GetMethod(methodName, inst | area | BindingFlags.GetField);
                if(methodInfo == null)
                {
                    if(t.BaseType == null)
                        return null;
                    t = t.BaseType;
                    return invokeMethodLoop(ref obj, ref methodName, ref parameters, ref inst, ref area, ref t, ref methodInfo);
                }
                else
                {
                    __MethodInfoCache.Add(t, new Dictionary<string, MethodInfo>());
                    __MethodInfoCache[t].Add(methodName, methodInfo);
                    return methodInfo.Invoke(obj, parameters);
                }
            }
        }

        private static object invokeMethod(ref object obj, ref string MethodName, ref object[] parameters, BindingFlags inst, BindingFlags area)
        {
            Type t = obj.GetType();
            MethodInfo methodInfo = null;
            return invokeMethodLoop(ref obj, ref MethodName, ref parameters, ref inst, ref area, ref t, ref methodInfo);
        }

        //---

        public static object InvokeNonPublicMethod(this object obj, string MethodName, object[] parameters)
        {
            return invokeMethod(ref obj, ref MethodName, ref parameters, BindingFlags.Instance, BindingFlags.NonPublic);
        }

        public static object InvokePublicMethod(this object obj, string MethodName, object[] parameters)
        {
            return invokeMethod(ref obj, ref MethodName, ref parameters, BindingFlags.Instance, BindingFlags.Public);
        }

        public static object InvokeNonPublicStaticMethod(this object obj, string MethodName, object[] parameters)
        {
            return invokeMethod(ref obj, ref MethodName, ref parameters, BindingFlags.Static, BindingFlags.NonPublic);
        }

        public static object InvokePublicStaticMethod(this object obj, string MethodName, object[] parameters)
        {
            return invokeMethod(ref obj, ref MethodName, ref parameters, BindingFlags.Static, BindingFlags.Public);
        }

        #endregion

        // -- Property get
        #region Property Get
        private static object getPropertyLoop(ref object obj, ref string propertyName, ref BindingFlags inst, ref BindingFlags area, ref Type t, ref PropertyInfo propertyInfo)
        {
            if(__PropertyInfoCache.TryGetValue(t, out __propertyCacheTmp))
            {
                if(__propertyCacheTmp.TryGetValue(propertyName, out propertyInfo))
                {
                    if(propertyInfo.CanRead)
                        return propertyInfo.GetValue(obj, null);
                    return null;
                }
                else
                {
                    propertyInfo = t.GetProperty(propertyName, inst | area | BindingFlags.GetProperty);
                    if(propertyInfo == null)
                    {
                        if(t.BaseType == null)
                            return null;
                        t = t.BaseType;
                        return getPropertyLoop(ref obj, ref propertyName, ref inst, ref area, ref t, ref propertyInfo);
                    }
                    else
                    {
                        if(propertyInfo.CanRead)
                        {
                            __PropertyInfoCache[t].Add(propertyName, propertyInfo);
                            return propertyInfo.GetValue(obj, null);
                        }
                        return null;
                    }
                }

            }
            else
            {
                propertyInfo = t.GetProperty(propertyName, inst | area | BindingFlags.GetProperty);
                if(propertyInfo == null)
                {
                    if(t.BaseType == null)
                        return null;
                    t = t.BaseType;
                    return getPropertyLoop(ref obj, ref propertyName, ref inst, ref area, ref t, ref propertyInfo);
                }
                else
                {
                    if(propertyInfo.CanRead)
                    {
                        __PropertyInfoCache.Add(t, new Dictionary<string, PropertyInfo>());
                        __PropertyInfoCache[t].Add(propertyName, propertyInfo);
                        return propertyInfo.GetValue(obj, null);
                    }
                    return null;
                }
            }
        }

        private static object getProperty(ref object obj, ref string propertyName, BindingFlags inst, BindingFlags area)
        {
            Type t = obj.GetType();
            PropertyInfo propertyInfo = null;
            return getPropertyLoop(ref obj, ref propertyName, ref inst, ref area, ref t, ref propertyInfo);
        }

        //---

        public static object GetNonPublicProperty(this object obj, string fieldName)
        {
            return getProperty(ref obj, ref fieldName, BindingFlags.Instance, BindingFlags.NonPublic);
        }

        public static object GetPublicProperty(this object obj, string fieldName)
        {
            return getProperty(ref obj, ref fieldName, BindingFlags.Instance, BindingFlags.Public);
        }

        //未验证
        public static object GetNonPublicStaticProperty(this object obj, string fieldName)
        {
            return getProperty(ref obj, ref fieldName, BindingFlags.Static, BindingFlags.NonPublic);
        }

        //未验证
        public static object GetPublicStaticProperty(this object obj, string fieldName)
        {
            return getProperty(ref obj, ref fieldName, BindingFlags.Static, BindingFlags.Public);
        }

        #endregion

        // -- Property set
        #region Property Set

        private static bool setPropertyLoop(ref object obj, ref string propertyName, ref object value, ref BindingFlags inst, ref BindingFlags area, ref Type t, ref PropertyInfo propertyInfo)
        {

            if(__PropertyInfoCache.TryGetValue(t, out __propertyCacheTmp))
            {

                if(__propertyCacheTmp.TryGetValue(propertyName, out propertyInfo))
                {
                    if(propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(obj, value, null);
                        return true;
                    }
                    return false;
                }
                else
                {
                    propertyInfo = t.GetProperty(propertyName, inst | area | BindingFlags.GetProperty);
                    if(propertyInfo == null)
                    {
                        if(t.BaseType == null)
                            return false;
                        t = t.BaseType;
                        return setPropertyLoop(ref obj, ref propertyName, ref value, ref inst, ref area, ref t, ref propertyInfo);
                    }
                    else
                    {
                        if(propertyInfo.CanWrite)
                        {
                            __PropertyInfoCache[t].Add(propertyName, propertyInfo);
                            propertyInfo.SetValue(obj, value, null);
                            return true;
                        }
                        return false;
                    }
                }

            }
            else
            {
                propertyInfo = t.GetProperty(propertyName, inst | area | BindingFlags.GetProperty);
                if(propertyInfo == null)
                {
                    if(t.BaseType == null)
                        return false;
                    t = t.BaseType;
                    return setPropertyLoop(ref obj, ref propertyName, ref value, ref inst, ref area, ref t, ref propertyInfo);
                }
                else
                {
                    if(propertyInfo.CanWrite)
                    {
                        __PropertyInfoCache.Add(t, new Dictionary<string, PropertyInfo>());
                        __PropertyInfoCache[t].Add(propertyName, propertyInfo);
                        propertyInfo.SetValue(obj, value, null);
                        return true;
                    }
                    return false;
                }
            }
        }

        private static bool setProperty(ref object obj, ref string propertyName, ref object value, BindingFlags inst, BindingFlags area)
        {
            Type t = obj.GetType();
            PropertyInfo propertyInfo = null;
            return setPropertyLoop(ref obj, ref propertyName, ref value, ref inst, ref area, ref t, ref propertyInfo);
        }

        //---

        public static bool SetNonPublicProperty(this object obj, string fieldName, object value)
        {
            return setProperty(ref obj, ref fieldName, ref value, BindingFlags.Instance, BindingFlags.NonPublic);
        }

        public static bool SetPublicProperty(this object obj, string fieldName, object value)
        {
            return setProperty(ref obj, ref fieldName, ref value, BindingFlags.Instance, BindingFlags.Public);
        }

        //未验证
        public static bool SetNonPublicStaticProperty(this object obj, string fieldName, object value)
        {
            return setProperty(ref obj, ref fieldName, ref value, BindingFlags.Static, BindingFlags.NonPublic);
        }

        //未验证
        public static bool SetPublicStaticProperty(this object obj, string fieldName, object value)
        {
            return setProperty(ref obj, ref fieldName, ref value, BindingFlags.Static, BindingFlags.Public);
        }

        #endregion

        //---

        #region 废弃的方法

        [Obsolete("已由GetNonPublicField替代")]
        public static object ref_GetField_Inst_NonPublic(this object obj, string fieldName)
        {
            return GetNonPublicField(obj, fieldName);
        }
        [Obsolete("已由GetPublicField替代")]
        public static object ref_GetField_Inst_Public(this object obj, string fieldName)
        {
            return GetPublicField(obj, fieldName);
        }

        [Obsolete("已由GetNonPublicStaticField替代")]
        public static object ref_GetField_Static_NonPublic(this object obj, string fieldName)
        {
            return GetNonPublicStaticField(obj, fieldName);
        }

        [Obsolete("已由GetPublicStaticField替代")]
        public static object ref_GetField_Static_Public(this object obj, string fieldName)
        {
            return GetPublicStaticField(obj, fieldName);
        }

        [Obsolete("已由SetNonPublicField替代")]
        public static bool ref_SetField_Inst_NonPublic(this object obj, string fieldName, object value)
        {
            return SetNonPublicField(obj, fieldName, value);
        }

        [Obsolete("已由SetPublicField替代")]
        public static bool ref_SetField_Inst_Public(this object obj, string fieldName, object value)
        {
            return SetPublicField(obj, fieldName, value);
        }

        [Obsolete("已由SetNonPublicStaticField替代")]
        public static bool ref_SetField_Static_NonPublic(this object obj, string fieldName, object value)
        {
            return SetNonPublicStaticField(obj, fieldName, value);
        }

        [Obsolete("已由SetPublicStaticField替代")]
        public static bool ref_SetField_Static_Public(this object obj, string fieldName, object value)
        {
            return SetPublicStaticField(obj, fieldName, value);
        }

        [Obsolete("已由InvokeNonPublicMethod替代")]
        public static object ref_InvokeMethod_Inst_NonPublic(this object obj, string MethodName, object[] parameters)
        {
            return InvokeNonPublicMethod(obj, MethodName, parameters);
        }

        [Obsolete("已由InvokePublicMethod替代")]
        public static object ref_InvokeMethod_Inst_Public(this object obj, string MethodName, object[] parameters)
        {
            return InvokePublicMethod(obj, MethodName, parameters);
        }

        [Obsolete("已由InvokeNonPublicStaticMethod替代")]
        public static object ref_InvokeMethod_Static_NonPublic(this object obj, string MethodName, object[] parameters)
        {
            return InvokeNonPublicStaticMethod(obj, MethodName, parameters);
        }

        [Obsolete("已由InvokePublicStaticMethod替代")]
        public static object ref_InvokeMethod_Static_Public(this object obj, string MethodName, object[] parameters)
        {
            return InvokePublicStaticMethod(obj, MethodName, parameters);
        }

        #endregion

    }

}

#endif

