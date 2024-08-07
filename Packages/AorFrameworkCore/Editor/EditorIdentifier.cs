using System;
using System.Collections.Generic;
using UnityEngine;

namespace AORCore
{
    /// <summary>
    /// 为当前编辑器提供标识符
    /// </summary>
    public class EditorIdentifier
    {

        private static string m_editorIdentifierTag;
        public static string EditorIdentifierTag
        {
            get
            {
                if (string.IsNullOrEmpty(m_editorIdentifierTag))
                {
                    m_editorIdentifierTag = $"{Application.unityVersion}-{Application.version}-{Application.companyName}{Application.buildGUID}";
                }
                return m_editorIdentifierTag;
            }
        }

    }
}
