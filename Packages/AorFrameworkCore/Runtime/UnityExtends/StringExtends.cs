using System;
using UnityEngine;
using System.Text.RegularExpressions;

public static class StringExtends
{

    #region 路径相关方法是

    /// <summary>
    /// 根据Unity路径规则将当前路径转换为完整(Full)路径
    /// </summary>
    public static string ToNormalizedFullPath(this string path, string suffix = null)
    {
        string fullPath = path.Replace("\\", "/");
        if (fullPath.StartsWith("/"))
            fullPath = fullPath.Substring(1);
        if (!fullPath.Contains(":/"))
        {
            if (fullPath.StartsWith("Assets/"))
                fullPath = $"{Application.dataPath.Replace("Assets", "")}{fullPath}";
            else
                fullPath = $"{Application.dataPath}{"/Resources/"}{fullPath}";
        }
        if(!string.IsNullOrEmpty(suffix) && !Regex.IsMatch(fullPath, @"\..+"))
        {
            fullPath += suffix;
        }
        return fullPath;
    }

    /// <summary>
    /// 根据Unity路径规则将当前路径转换为资产(Asset)路径
    /// </summary>
    public static string ToNormalizedAssetPath(this string path, string suffix = null)
    {
        string assetPath = path.Replace("\\", "/");
        if (assetPath.StartsWith("/"))
            assetPath = assetPath.Substring(1);

        if (assetPath.StartsWith("Assets/"))
            return assetPath;

        if (assetPath.Contains(":/"))
        {
            assetPath = assetPath.Replace($"{Application.dataPath}", "Assets");
        }
        if (!string.IsNullOrEmpty(suffix) && !Regex.IsMatch(assetPath, @"\..+"))
        {
            assetPath += suffix;
        }
        return assetPath;
    }

    /// <summary>
    /// 根据Unity路径规则将当前路径转换为资源(Resources)路径
    /// </summary>
    public static string ToNormalizedResPath(this string path)
    {
        string resPath = path.Replace("\\", "/");
        if (resPath.StartsWith("/"))
            resPath = resPath.Substring(1);

        if (resPath.Contains(":/"))
            resPath = resPath.Replace($"{Application.dataPath}/Resources/", "");
        else if (resPath.StartsWith("Assets/"))
            resPath = resPath.Replace("Assets/Resources/", "");

        if (Regex.IsMatch(resPath, @"\..+"))
            Regex.Replace(resPath, @"\..+", "");
        return resPath;
    }

    #endregion

}
