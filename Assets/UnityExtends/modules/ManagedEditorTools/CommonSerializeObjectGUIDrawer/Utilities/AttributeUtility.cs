using System;
using System.Reflection;

namespace CommonSerializeObjectGUI
{

    /// <summary>
    /// 用于判断类/方法/属性/字段是否添加Attribute的工具方法类
    /// 
    /// Author : Aorition
    /// Update : 2023/03/04
    /// 
    /// </summary>
    public class AttributeUtility
    {

        public static bool HasAttribute<T>(object[] attributes, out T attributeIns) where T : Attribute
        {
            if(attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

        public static bool HasAttribute<T>(Type type, out T attributeIns) where T : Attribute
        {
            object[] attributes = type.GetCustomAttributes(true);
            if (attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

        public static bool HasAttribute<T>(FieldInfo info, out T attributeIns) where T : Attribute
        {
            object[] attributes = info.GetCustomAttributes(true);
            if (attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

        public static bool HasAttribute<T>(MethodInfo info, out T attributeIns) where T : Attribute
        {
            object[] attributes = info.GetCustomAttributes(true);
            if (attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

        public static bool HasAttribute<T>(PropertyInfo info, out T attributeIns) where T : Attribute
        {
            object[] attributes = info.GetCustomAttributes(true);
            if (attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

        public static bool HasAttribute<T>(MemberInfo info, out T attributeIns) where T : Attribute
        {
            object[] attributes = info.GetCustomAttributes(true);
            if (attributes != null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is T)
                    {
                        attributeIns = (T)attribute;
                        return true;
                    }
                }
            }
            attributeIns = null;
            return false;
        }

    }

}


