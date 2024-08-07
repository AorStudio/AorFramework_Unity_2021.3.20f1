using System;
namespace CommonSerializeObjectGUI
{

    /// 公版 可序列化数据对象 字段显示标签
    /// 
    /// Author : Aorition
    /// Update : 2024-08-06
    /// ---------------------------------------------------

    /// <summary>
    /// 标记当前字段不在OnDrawInspectorGUI方法中被绘制
    /// </summary>
    public class CSOIgnoreFieldOnDrawInspectorGUIAttribute : Attribute
    {
        public CSOIgnoreFieldOnDrawInspectorGUIAttribute()
        {
        }
    }

    /// <summary>
    /// 设置当前字段的显示标签
    /// </summary>
    public class CSOFieldLabelAttribute : Attribute
    {
        public string Label;
        public CSOFieldLabelAttribute(string label)
        {
            Label = label;
        }
    }

    /// <summary>
    /// 设置当前字段强制显示(忽略Unity原生序列化规则)
    /// </summary>
    public class CSOForceFieldAttribute : Attribute
    {
        public CSOForceFieldAttribute()
        {
        }
    }

    /// <summary>
    /// 标识路径字符串处理方式
    /// </summary>
    public enum CSOPathTag
    {
        /// <summary>
        /// 以Assets开头的路径格式(适用于Unity编辑器)
        /// </summary>
        AssetPath,
        /// <summary>
        /// Resource标准路径格式(无后缀名，适用于Resources加载)
        /// </summary>
        ResourcesPath,
        /// <summary>
        /// 完整路径
        /// </summary>
        FullPath,
        /// <summary>
        /// 继承之前的设定
        /// </summary>
        Inherit
    }
    
    /// <summary>
    /// 可设置数组/List/子类对象用滚动区域显示
    /// </summary>
    public class CSOScrollAttribute : Attribute
    {
        public bool IsHorizontal = false;
        public float MinSize = -1;
        public float MaxSize = -1;

        public CSOScrollAttribute()
        {
        }

        public CSOScrollAttribute(bool isHorizontal)
        {
            IsHorizontal = isHorizontal;
        }

        public CSOScrollAttribute(float maxSize)
        {
            MaxSize = maxSize;
        }
        public CSOScrollAttribute(float maxSize, bool isHorizontal)
        {
            MaxSize = maxSize;
            IsHorizontal = isHorizontal;
        }
        public CSOScrollAttribute(float minSize, float maxSize, bool isHorizontal)
        {
            IsHorizontal = isHorizontal;
            MinSize = minSize;
            MaxSize = maxSize;
        }
    }

    /// <summary>
    /// 可设置数组/List用翻页区域显示
    /// </summary>
    public class CSOTurnPageAttribute : Attribute
    {
        public bool IsHorizontal = false;
        public int LimitPerPage;
        /// <summary>
        /// 设置数组/List用翻页区域方式显示
        /// </summary>
        /// <param name="limitPerPage">每页最大显示数据条数</param>
        public CSOTurnPageAttribute(int limitPerPage = 50)
        {
            LimitPerPage = limitPerPage;
        }
    }

    /// <summary>
    /// 为表示Path的String字段增加开一个按钮可快速指定到路径
    /// </summary>
    public class CSOFilePathSelectToolAttribute : Attribute
    {
        public CSOPathTag PathTag = CSOPathTag.Inherit;
        public CSOFilePathSelectToolAttribute(){}
        public CSOFilePathSelectToolAttribute(CSOPathTag tag)
        {
            PathTag = tag;
        }
    }

    /// <summary>
    /// 将表示Path的String字段使用Unity.Object Field UI方式显示(包括文件夹)
    /// * 注意此标签中将强制使用CSOPathTag.AssetPath的路径规则来处理获取的路径字段
    /// </summary>
    public class CSOPathToUObjFeildAttribute : Attribute
    {
        public string Label;
        public CSOPathTag PathTag = CSOPathTag.Inherit;
        public CSOPathToUObjFeildAttribute() { }
        public CSOPathToUObjFeildAttribute(string label)
        {
            Label = label;
        }
        public CSOPathToUObjFeildAttribute(CSOPathTag tag)
        {
            PathTag = tag;
        }
        public CSOPathToUObjFeildAttribute(string label, CSOPathTag tag)
        {
            Label = label;
            PathTag = tag;
        }
    }

    /// <summary>
    /// 为表示文件夹Path的String字段增加开一个按钮可快速指定到文件夹路径
    /// </summary>
    public class CSODirPathSelectToolAttribute : Attribute
    {

        public CSOPathTag PathTag = CSOPathTag.Inherit;
        public CSODirPathSelectToolAttribute(){}
        public CSODirPathSelectToolAttribute(CSOPathTag tag)
        {
            PathTag = tag;
        }

    }

    /// <summary>
    /// 为路径String字段配置一个按钮可通过Explorer打开相关路径
    /// </summary>
    public class CSOPathOpenDirInExplorerToolAttribute : Attribute
    {

        public CSOPathTag PathTag = CSOPathTag.Inherit;
        public CSOPathOpenDirInExplorerToolAttribute()
        {
        }
        public CSOPathOpenDirInExplorerToolAttribute(CSOPathTag tag)
        {
            PathTag = tag;
        }

    }

}


