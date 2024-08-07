using UnityEditor;
using UnityEngine;

namespace NodeGraph.Editor
{
    public static class MaterialPropertiesCleanUtils
    {

        /// <summary>
        /// 解析(并移除)材质球上冗余的序列化数据
        /// </summary>
        /// <param name="material">目标材质球</param>
        /// <param name="report">报告(接收变量)</param>
        /// <param name="justReport">仅报告但不移除冗余数据</param>
        /// <returns></returns>
        public static bool ParseMaterialProperties(Material material, ref string report, bool justReport)
        {
            bool dirty = false;
            SerializedObject serializedObject = new SerializedObject(material);
            serializedObject.Update();
            report += $"Materail: {material.name} (shader:{(material.shader != null ? material.shader.name : "null")})\n";
            //Properties
            report += "Properties:\n";
            //Textures
            report += "\tTextures[";
            if(ProcessProperties(material, serializedObject, "m_SavedProperties.m_TexEnvs", ref report, justReport))
            {
                dirty = true;
            }
            report += "]\n";
            //Floats
            report += "\tFloats[";
            if(ProcessProperties(material, serializedObject, "m_SavedProperties.m_Floats", ref report, justReport))
            {
                dirty = true;
            }
            report += "]\n";
            //Colors
            report += "\tColors[";
            if(ProcessProperties(material, serializedObject, "m_SavedProperties.m_Colors", ref report, justReport))
            {
                dirty = true;
            }
            report += "]\n";
            return dirty;
        }

        private static bool ProcessProperties(Material material, SerializedObject serializedObject, string propertyName, ref string report, bool justReport)
        {
            bool dirty = false;
            var properties = serializedObject.FindProperty(propertyName);
            if(properties != null && properties.isArray)
            {
                int ii = 0;
                for(int i = 0; i < properties.arraySize; i++)
                {
                    string propName = properties.GetArrayElementAtIndex(i).displayName;
                    bool exist = material.HasProperty(propName);

                    if(!exist)
                    {
                        if(ii > 0)
                            report += ",";
                        report += $"{propName}\t(Obsolete)";
                        if(!justReport)
                        {
                            properties.DeleteArrayElementAtIndex(i);
                            serializedObject.ApplyModifiedProperties();
                        }
                        dirty = true;
                    }
                }
            }
            return dirty;
        }


    }
}
