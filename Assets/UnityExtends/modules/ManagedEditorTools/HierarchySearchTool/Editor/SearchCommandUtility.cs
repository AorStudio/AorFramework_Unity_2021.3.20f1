using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering.Universal.Editor.Utility
{

    public enum SearchCommandConditionType
    {
        Equals,
        NotEquals,
        Greater,
        Less,
        GreaterEquals,
        LessEquals
    }

    public enum SearchCommandGroupLogicType
    {
        AND,
        OR
    }

    public enum SearchCommandType
    {
        Name,
        GameObjectWithFieldValue,
        Tag,
        Layer,
        Component,
        ComponentWithFieldValue
    }

    public class SearchCommandUtility
    {

        #region AllComponentTypes 静态实现

        private static bool CheckBaseType(Type type, Type BaseType)
        {
            if (type.FullName == BaseType.FullName)
                return true;
            Type pType = type.BaseType;
            if (pType != null)
                return CheckBaseType(pType, BaseType);
            return false;
        }

        private static Dictionary<string, List<Type>> GetAllComponentTypes()
        {
            var result = new Dictionary<string, List<Type>>();
            Type baseType = typeof(Component);
            Type[] types = Assembly.Load("Assembly-CSharp").GetTypes();
            foreach (var type in types)
            {
                if (CheckBaseType(type, baseType))
                {
                    if (!result.ContainsKey(type.Name))
                        result.Add(type.Name, new List<Type>());
                    result[type.Name].Add(type);
                }
            }
            types = typeof(GameObject).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (CheckBaseType(type, baseType))
                {
                    if (!result.ContainsKey(type.Name))
                        result.Add(type.Name, new List<Type>());
                    result[type.Name].Add(type);
                }
            }
            //types = typeof(UniversalRenderPipeline).Assembly.GetTypes();
            //foreach (var type in types)
            //{
            //    if (CheckBaseType(type, baseType))
            //    {
            //        if (!result.ContainsKey(type.Name))
            //            result.Add(type.Name, new List<Type>());
            //        result[type.Name].Add(type);
            //    }
            //}
            return result;
        }

        private static Dictionary<string, List<Type>> m_AllComponentTypes;
        public static Dictionary<string, List<Type>> AllComponentTypes
        {
            get
            {
                if (m_AllComponentTypes == null)
                {
                    m_AllComponentTypes = GetAllComponentTypes();
                }
                return m_AllComponentTypes;
            }
        }

        #endregion

        public static string[] AllLayers => UnityEditorInternal.InternalEditorUtility.layers;
        public static string[] AllTags => UnityEditorInternal.InternalEditorUtility.tags;

        public static SearchCommand ParseStringToSearchCommand(string stringkey)
        {
            stringkey = stringkey.Trim();
            //处理逻辑优先层次
            List<string> strForLevels = new List<string>();
            ParseStringForLogicLevel(stringkey, strForLevels);

            bool mutiLevel = strForLevels.Count > 1;
            SearchCommand root = new SearchCommand();
            SearchCommand parent = root;
            for (int i = 0; i < strForLevels.Count; i++)
            {
                //是否包含 ||
                string levelStr = strForLevels[i];
                if(Regex.IsMatch(levelStr, @" ?\|\| ?"))
                {
                    parent.subLogicType = SearchCommandGroupLogicType.OR;
                    string[] orStrs = Regex.Split(levelStr, @" ?\|\| ?");
                    for (int oIdx = 0; oIdx < orStrs.Length; oIdx++)
                    {
                        string orStr = orStrs[oIdx].Trim();
                        if (Regex.IsMatch(orStr, @" +"))
                        {
                            SearchCommand subContent = new SearchCommand();
                            subContent.subLogicType = SearchCommandGroupLogicType.AND;
                            parent.AddSubSearchCommand(subContent);
                            string[] aStrs = Regex.Split(orStr, @" +");
                            foreach (string aStr in aStrs)
                            {
                                subContent.AddSubSearchCommand(SearchCommand.Create(orStr));
                            }
                            if (oIdx + 1 == orStrs.Length && mutiLevel)
                            {
                                //准备下层容器
                                SearchCommand nextLevel = new SearchCommand();
                                subContent.AddSubSearchCommand(nextLevel);
                                parent = nextLevel;
                            }
                        }
                        else
                        {
                            parent.AddSubSearchCommand(SearchCommand.Create(orStr));
                            if (oIdx + 1 == orStrs.Length && mutiLevel)
                            {
                                //准备下层容器
                                SearchCommand nextLevel = new SearchCommand();
                                parent.AddSubSearchCommand(nextLevel);
                                parent = nextLevel;
                            }
                        }
                    } 
                }
                else
                {
                    parent.subLogicType = SearchCommandGroupLogicType.AND;
                    string lStr = levelStr.Trim();
                    if (Regex.IsMatch(lStr, @" +"))
                    {
                        SearchCommand subContent = new SearchCommand();
                        subContent.subLogicType = SearchCommandGroupLogicType.AND;
                        parent.AddSubSearchCommand(subContent);
                        string[] aStrs = Regex.Split(lStr, @" +");
                        for (int aIdx = 0; aIdx < aStrs.Length; aIdx++)
                        {
                            string aStr = aStrs[aIdx].Trim();
                            subContent.AddSubSearchCommand(SearchCommand.Create(aStr));
                            if (aIdx + 1 == aStrs.Length && mutiLevel)
                            {
                                //准备下层容器
                                SearchCommand nextLevel = new SearchCommand();
                                subContent.AddSubSearchCommand(nextLevel);
                                parent = nextLevel;
                            }
                        }
                    }
                    else
                    {
                        parent.AddSubSearchCommand(SearchCommand.Create(lStr));
                        if (mutiLevel)
                        {
                            //准备下层容器
                            SearchCommand nextLevel = new SearchCommand();
                            parent.AddSubSearchCommand(nextLevel);
                            parent = nextLevel;
                        }
                    }
                }
            }
            return root;
        }

        public static List<GameObject> ExecSearchCommand(string searchString, GameObject searchRoot = null)
        {
            return ExecSearchCommand(ParseStringToSearchCommand(searchString), searchRoot);
        }

        public static List<GameObject> ExecSearchCommand(SearchCommand searchCommand, GameObject searchRoot = null)
        {
            List<GameObject> result = new List<GameObject>();
            GameObject[] m_searchRoots = searchRoot ? new GameObject[] { searchRoot } : SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in m_searchRoots)
            {
                CheckSearchCommandLoop(result, root.transform, searchCommand);
            }
            return result;
        }

        public static void ExecSearchCommand(List<GameObject> result, string searchString, GameObject searchRoot = null)
        {
            ExecSearchCommand(result, ParseStringToSearchCommand(searchString), searchRoot);
        }

        public static void ExecSearchCommand(List<GameObject> result, SearchCommand searchCommand, GameObject searchRoot = null)
        {
            if (result == null)
                result = new List<GameObject>();
            else
                result.Clear();

            GameObject[] m_searchRoots = searchRoot ? new GameObject[] { searchRoot } : SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in m_searchRoots)
            {
                CheckSearchCommandLoop(result, root.transform, searchCommand);
            }
        }

        //处理逻辑优先层次
        private static void ParseStringForLogicLevel(string str, List<string> strForLevels)
        {
            Match match = Regex.Match(str, @"\((.+)\)");
            if (match.Success)
            {
                string subStr = match.Groups[1].Value;
                strForLevels.Add(str.Replace(match.Value, "").Trim());
                ParseStringForLogicLevel(subStr, strForLevels);
            }
            else
            {
                strForLevels.Add(str);
            }
        }

        private static void CheckSearchCommandLoop(List<GameObject> result, Transform node, SearchCommand searchCommand)
        {
            if (searchCommand.Check(node.gameObject))
            {
                result.Add(node.gameObject);
            }
            if (node.childCount > 0)
            {
                for (int i = 0; i < node.childCount; i++)
                {
                    Transform subNode = node.GetChild(i);
                    CheckSearchCommandLoop(result, subNode, searchCommand);
                }
            }
        }

    }

    public class SearchCommand
    {

        public static SearchCommand Create(string commandStr)
        {
            var searchCommand = new SearchCommand();
            if (commandStr.ToLower().StartsWith("t:"))
            {
                if (Regex.IsMatch(commandStr, @"{(.+)}"))
                {
                    searchCommand.Type = SearchCommandType.ComponentWithFieldValue;
                    Match m = Regex.Match(commandStr, @"{(.+)}");
                    string propStr = m.Groups[1].Value;
                    string other = commandStr.Replace(m.Value, "").Trim();
                    string typeName = Regex.Match(other, @"t:(.+)").Groups[1].Value;
                    searchCommand.CompoentType = SearchCommandUtility.AllComponentTypes[typeName][0];

                    string valueStr = string.Empty;
                    if (propStr.Contains("!="))
                    {
                        string[] p = propStr.Replace("!=", "=").Split('=');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.NotEquals;
                        valueStr = p[1];
                    }
                    else if (propStr.Contains(">="))
                    {
                        string[] p = propStr.Replace(">=", ">").Split('>');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.GreaterEquals;
                        valueStr = p[1];
                    }
                    else if (propStr.Contains("<="))
                    {
                        string[] p = propStr.Replace("<=", "<").Split('<');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.LessEquals;
                        valueStr = p[1];
                    }
                    else if (propStr.Contains(">"))
                    {
                        string[] p = propStr.Split('>');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.Greater;
                        valueStr = p[1];
                    }
                    else if (propStr.Contains("<"))
                    {
                        string[] p = propStr.Split('<');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.Less;
                        valueStr = p[1];
                    }
                    else if (propStr.Contains("="))
                    {
                        string[] p = propStr.Split('=');
                        searchCommand.SubName = p[0];
                        searchCommand.ConditionType = SearchCommandConditionType.Equals;
                        valueStr = p[1];
                    }

                    PropertyInfo propertyInfo = searchCommand.CompoentType.GetProperty(searchCommand.SubName, BindingFlags.Instance | BindingFlags.Public);
                    if (propertyInfo != null)
                    {
                        searchCommand.SetValue(propertyInfo.PropertyType, valueStr);
                    }
                    else
                    {
                        FieldInfo fieldInfo = searchCommand.CompoentType.GetField(searchCommand.SubName, BindingFlags.Instance | BindingFlags.Public);
                        if (fieldInfo != null)
                        {
                            searchCommand.SetValue(fieldInfo.FieldType, valueStr);
                        }
                    }

                }
                else
                {
                    searchCommand.Type = SearchCommandType.Component;
                    string typeName = Regex.Match(commandStr, @"t:(.+)").Groups[1].Value;
                    searchCommand.CompoentType = SearchCommandUtility.AllComponentTypes[typeName][0];
                    searchCommand.StringValue = string.Empty;
                }
            }
            else if (commandStr.ToLower().StartsWith("tag:"))
            {
                searchCommand.Type = SearchCommandType.Tag;
                string typeName = Regex.Match(commandStr, @"tag:(.+)").Groups[1].Value;
                searchCommand.StringValue = typeName;
            }
            else if (commandStr.ToLower().StartsWith("l:") || commandStr.ToLower().StartsWith("layer:"))
            {
                searchCommand.Type = SearchCommandType.Layer;
                if(Regex.IsMatch(commandStr, @"l:(.+)"))
                {
                    string typeName = Regex.Match(commandStr, @"l:(.+)").Groups[1].Value;
                    searchCommand.StringValue = typeName;
                }
                else if (Regex.IsMatch(commandStr, @"layer:(.+)"))
                {
                    string typeName = Regex.Match(commandStr, @"layer:(.+)").Groups[1].Value;
                    searchCommand.StringValue = typeName;
                }
            }else if (commandStr.StartsWith("{"))
            {
                searchCommand.Type = SearchCommandType.GameObjectWithFieldValue;
                Match m = Regex.Match(commandStr, @"{(.+)}");
                string propStr = m.Groups[1].Value;

                string valueStr = string.Empty;
                if (propStr.Contains("!="))
                {
                    string[] p = propStr.Replace("!=", "=").Split('=');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.NotEquals;
                    valueStr = p[1];
                }
                else if (propStr.Contains(">="))
                {
                    string[] p = propStr.Replace(">=", ">").Split('>');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.GreaterEquals;
                    valueStr = p[1];
                }
                else if (propStr.Contains("<="))
                {
                    string[] p = propStr.Replace("<=", "<").Split('<');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.LessEquals;
                    valueStr = p[1];
                }
                else if (propStr.Contains(">"))
                {
                    string[] p = propStr.Split('>');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.Greater;
                    valueStr = p[1];
                }
                else if (propStr.Contains("<"))
                {
                    string[] p = propStr.Split('<');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.Less;
                    valueStr = p[1];
                }
                else if (propStr.Contains("="))
                {
                    string[] p = propStr.Split('=');
                    searchCommand.SubName = p[0];
                    searchCommand.ConditionType = SearchCommandConditionType.Equals;
                    valueStr = p[1];
                }

                PropertyInfo propertyInfo = typeof(GameObject).GetProperty(searchCommand.SubName, BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    searchCommand.SetValue(propertyInfo.PropertyType, valueStr);
                }
                else
                {
                    FieldInfo fieldInfo = typeof(GameObject).GetField(searchCommand.SubName, BindingFlags.Instance | BindingFlags.Public);
                    if (fieldInfo != null)
                    {
                        searchCommand.SetValue(fieldInfo.FieldType, valueStr);
                    }
                }

            }
            else
            {
                searchCommand.Type = SearchCommandType.Name;
                searchCommand.StringValue = commandStr;
            }
            return searchCommand;
        }

        /// <summary>
        /// (目标只支持基础类型)
        /// </summary>
        private static bool ConditionCheck(Type valueType, object value, SearchCommand searchCommand)
        {
            switch (valueType.Name)
            {
                case "Boolean":
                    {
                        bool o = (bool)value;
                        switch (searchCommand.ConditionType)
                        {
                            case SearchCommandConditionType.Equals:
                                return o == searchCommand.BoolValue;
                            case SearchCommandConditionType.NotEquals:
                                return o != searchCommand.BoolValue;
                            default:
                                return false;
                        }
                    }
                case "Single":
                    {
                        float o = (float)value;
                        switch (searchCommand.ConditionType)
                        {
                            case SearchCommandConditionType.Equals:
                                return o == searchCommand.FloatValue;
                            case SearchCommandConditionType.NotEquals:
                                return o != searchCommand.FloatValue;
                            case SearchCommandConditionType.Greater:
                                return o > searchCommand.FloatValue;
                            case SearchCommandConditionType.Less:
                                return o < searchCommand.FloatValue;
                            case SearchCommandConditionType.GreaterEquals:
                                return o >= searchCommand.FloatValue;
                            case SearchCommandConditionType.LessEquals:
                                return o <= searchCommand.FloatValue;
                            default:
                                return false;
                        }
                    }
                case "Byte":
                case "Int16":
                case "Int32":
                    {
                        int o = (int)value;
                        switch (searchCommand.ConditionType)
                        {
                            case SearchCommandConditionType.Equals:
                                return o == searchCommand.IntValue;
                            case SearchCommandConditionType.NotEquals:
                                return o != searchCommand.IntValue;
                            case SearchCommandConditionType.Greater:
                                return o > searchCommand.IntValue;
                            case SearchCommandConditionType.Less:
                                return o < searchCommand.IntValue;
                            case SearchCommandConditionType.GreaterEquals:
                                return o >= searchCommand.IntValue;
                            case SearchCommandConditionType.LessEquals:
                                return o <= searchCommand.IntValue;
                            default:
                                return false;
                        }
                    }
                case "String":
                    {
                        string o = (string)value;
                        switch (searchCommand.ConditionType)
                        {
                            case SearchCommandConditionType.Equals:
                                return o == searchCommand.StringValue;
                            case SearchCommandConditionType.NotEquals:
                                return o != searchCommand.StringValue;
                            case SearchCommandConditionType.Greater:
                                return o.Contains(searchCommand.StringValue);
                            case SearchCommandConditionType.Less:
                                return o.Contains(searchCommand.StringValue);
                            case SearchCommandConditionType.GreaterEquals:
                                return o.ToLower().Contains(searchCommand.StringValue);
                            case SearchCommandConditionType.LessEquals:
                                return o.ToLower().Contains(searchCommand.StringValue);
                            default:
                                return false;
                        }
                    }
                default:
                    {
                        if(value is UnityEngine.Object)
                        {
                            switch (searchCommand.ConditionType)
                            {
                                case SearchCommandConditionType.Equals:
                                    return value == null;
                                case SearchCommandConditionType.NotEquals:
                                    return value != null;
                                default:
                                    return false;
                            }
                        }else if(value is Enum)
                        {
                            switch (searchCommand.ConditionType)
                            {
                                case SearchCommandConditionType.Equals:
                                    return value.ToString() == searchCommand.StringValue;
                                case SearchCommandConditionType.NotEquals:
                                    return value.ToString() != searchCommand.StringValue;
                                default:
                                    return false;
                            }
                        }
                    }
                    return false;
            }
        }

        private static bool CheckSingle(GameObject gameObject, SearchCommand command)
        {
            switch (command.Type)
            {
                case SearchCommandType.Name:
                    return gameObject.name.Contains(command.StringValue);
                case SearchCommandType.Tag:
                    return gameObject.tag.Equals(command.StringValue);
                case SearchCommandType.Layer:
                    return gameObject.layer == command.IntValue;
                case SearchCommandType.Component:
                    return gameObject.GetComponent(command.CompoentType);
                case SearchCommandType.GameObjectWithFieldValue:
                    {
                        PropertyInfo propertyInfo = typeof(GameObject).GetProperty(command.SubName, BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo != null)
                        {
                            return ConditionCheck(propertyInfo.PropertyType, propertyInfo.GetValue(gameObject), command);
                        }
                        else
                        {
                            FieldInfo fieldInfo = typeof(GameObject).GetField(command.SubName, BindingFlags.Instance | BindingFlags.Public);
                            if (fieldInfo != null)
                            {
                                return ConditionCheck(fieldInfo.FieldType, fieldInfo.GetValue(gameObject), command);
                            }
                        }
                        return false;
                    }
                case SearchCommandType.ComponentWithFieldValue:
                    {
                        var ins = gameObject.GetComponent(command.CompoentType);
                        if (ins)
                        {
                            PropertyInfo propertyInfo = command.CompoentType.GetProperty(command.SubName, BindingFlags.Public | BindingFlags.Instance);
                            if(propertyInfo != null)
                            {
                                return ConditionCheck(propertyInfo.PropertyType, propertyInfo.GetValue(ins), command);
                            }
                            else
                            {
                                FieldInfo fieldInfo = command.CompoentType.GetField(command.SubName, BindingFlags.Instance | BindingFlags.Public);
                                if (fieldInfo != null)
                                {
                                    return ConditionCheck(fieldInfo.FieldType, fieldInfo.GetValue(ins), command);
                                }
                            }
                        }
                        return false;
                    }
            }
            return false;
        }

        public SearchCommandType Type;
        public SearchCommandConditionType ConditionType;
        public Type CompoentType;
        public string SubName;
        public string StringValue;
        public int IntValue;
        public float FloatValue;
        public bool BoolValue;

        public void SetValue(Type valueType, string valueStr)
        {
            switch (valueType.Name)
            {
                case "Boolean":
                    BoolValue = bool.Parse(valueStr);
                    break;
                case "Single":
                    FloatValue = float.Parse(valueStr);
                    break;
                case "Byte":
                case "Int16":
                case "Int32":
                    IntValue = int.Parse(valueStr);
                    break;
                //case "String":
                //    StringValue = valueStr;
                //    break;
                default:
                    StringValue = valueStr;
                    break;
            }
        }

        public bool IsGroup => m_SubCommands.Count > 0;

        public SearchCommandGroupLogicType subLogicType;
        private readonly List<SearchCommand> m_SubCommands = new List<SearchCommand>();
        public int GroupCount => m_SubCommands.Count;

        public void AddSubSearchCommand(SearchCommand sub)
        {
            m_SubCommands.Add(sub);
        }
        public SearchCommand GetSubSearchCommand(int index)
        {
            return m_SubCommands[index];
        }
        public void RemoveSubSearchCommand(SearchCommand sub)
        {
            m_SubCommands.Remove(sub);
        }
        public void RemoveSubSearchCommandAtIndex(int index)
        {
            m_SubCommands.RemoveAt(index);
        }
        public void ClearSubSearchCommand()
        {
            m_SubCommands.Clear();
        }
        public bool Check(GameObject gameObject)
        {
            if (IsGroup)
            {
                if(subLogicType == SearchCommandGroupLogicType.AND)
                {
                    //只要有一个为false，则可以判定为false
                    foreach (SearchCommand command in m_SubCommands)
                    {
                        if (!command.Check(gameObject))
                            return false;
                    }
                    return true;
                }else if (subLogicType == SearchCommandGroupLogicType.OR)
                {
                    //只要有一个为true,则可以判定为true
                    foreach (SearchCommand command in m_SubCommands)
                    {
                        if (command.Check(gameObject))
                            return true;
                    }
                }
                return false;
            }
            else
            {
                return CheckSingle(gameObject, this);
            }
        }



    }  

}
