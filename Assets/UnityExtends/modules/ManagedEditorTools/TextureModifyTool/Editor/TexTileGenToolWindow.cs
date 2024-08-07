using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using AORCore.Editor;
#if FRAMEWORKDEF
using AorBaseUtility;
using Framework.Extends;
#else
using AORCore;
#endif

namespace UnityEngine.Rendering.Universal.Editor.Utility
{
    /// <summary>
    /// Texture2D Tiles分割工具
    /// 
    /// Update Date : 2021-08-21 Aorition
    /// 
    /// </summary>
    public class TexTileGenToolWindow :UnityEditor.EditorWindow
    {

        private enum ColorChannel
        {
            ChannelR,
            ChannelG,
            ChannelB,
            ChannelA
        }

        private enum ColorBit
        {
            Bit8,
            Bit16,
            Bit32
        }

        private static GUIStyle _titleStyle;
        protected static GUIStyle titleStyle
        {
            get {
                if(_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _titleStyle.fontSize = 16;
                    _titleStyle.fontStyle = FontStyle.Bold;
                }
                return _titleStyle;
            }
        }

        private static GUIStyle _sTitleStyle;
        protected static GUIStyle sTitleStyle
        {
            get {
                if(_sTitleStyle == null)
                {
                    _sTitleStyle = new GUIStyle(EditorStyles.largeLabel);
                    _sTitleStyle.fontSize = 14;
                    _sTitleStyle.fontStyle = FontStyle.Bold;
                }
                return _sTitleStyle;
            }
        }

        private static GUIStyle _sTipStyle;
        protected static GUIStyle sTipStyle
        {
            get {
                if(_sTipStyle == null)
                {
                    _sTipStyle = new GUIStyle(EditorStyles.label);
                    _sTipStyle.richText = true;
                    _sTipStyle.fontSize = 11;
                    _sTipStyle.wordWrap = true;
                }
                return _sTipStyle;
            }
        }

        //--------------------------------------------------------------

        //-------------------------------------------------------------

        private static TexTileGenToolWindow _instance;

        [MenuItem("Window/FrameworkTools/Bitmaps/Texture2D Tiles网格分割")]
        public static TexTileGenToolWindow init()
        {

            _instance = UnityEditor.EditorWindow.GetWindow<TexTileGenToolWindow>();
            _instance.minSize = new Vector2(495, 612);

            return _instance;
        }

        private static string[] _menuLabels = new string[] { "Tiles分割" };
        private static int m_menuIndex;
        private Vector2 _scrollPos = new Vector2();
        private void OnGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            {
                GUILayout.Space(15);
                _draw_toolTitle_UI();
                GUILayout.Space(15);

                m_menuIndex = GUILayout.Toolbar(m_menuIndex, _menuLabels, GUILayout.Height(28));
                switch(m_menuIndex)
                {
                    case 1:
                        {
                            //_draw_savePath_UI(true);
                            //_draw_MergeTex_UI();
                        }
                    break;
                    default:
                        {
                            _draw_savePath_UI();
                            _draw_TilesGen_UI();
                        }
                    break;
                }

                GUILayout.Space(15);
            }
            GUILayout.EndScrollView();

            //EditorPlusMethods.Draw_DebugWindowSizeUI();
        }

        //--------------------------------------

        private void _draw_toolTitle_UI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("      位图网格分割工具      ", titleStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }
            GUILayout.EndVertical();
        }

        //=============================================================================

        private bool m_useSrcTexPathDir = true;

        private string m_savePath;

        private void verifySavePath()
        {
            EditorAssetInfo info = new EditorAssetInfo(_srcTexPath);
            m_savePath = info.dirPath;
        }

        private void _draw_savePath_UI(bool focus = false)
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_subTitle_UI("------ 设置生成路径 ------");
                GUILayout.Space(5);

                if(focus)
                {
                    m_useSrcTexPathDir = false;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(24);
                        GUILayout.Label("使用源图片所在的文件夹作为生成路径");
                        GUILayout.FlexibleSpace();
                        m_useSrcTexPathDir = EditorGUILayout.Toggle(m_useSrcTexPathDir);
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }

                if(!m_useSrcTexPathDir)
                {

                    GUILayout.Space(5);

                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("设置生成路径");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        {
                            m_savePath = EditorGUILayout.TextField(m_savePath);
                            if(GUILayout.Button("UseSelection", GUILayout.Width(120)))
                            {
                                if(Selection.activeObject)
                                {
                                    string tp = AssetDatabase.GetAssetPath(Selection.activeObject);
                                    if(!string.IsNullOrEmpty(tp))
                                    {

                                        EditorAssetInfo info = new EditorAssetInfo(tp);
                                        m_savePath = info.dirPath;

                                    }
                                    else
                                    {
                                        m_savePath = "";
                                    }
                                }
                                else
                                {
                                    m_savePath = "";
                                }
                            }
                            if(GUILayout.Button("Set", GUILayout.Width(50)))
                            {
                                m_savePath = EditorUtility.SaveFolderPanel("设置保存路径", "", "");
                                m_savePath = m_savePath.Replace(Application.dataPath, "Assets");
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        //=============================================================================

        private Texture2D _srcTexture;
        private string _srcTexPath;
        private int _tileU = 2;
        private int _tileV = 2;
        private int _extU = 0;
        private int _extV = 0;
        private bool _DoubleSizeExt;

        private int _tileW = 0;
        private int _tileWe = 0;
        private int _tileH = 0;
        private int _tileHe = 0;
        private bool _isIntDivU;
        private bool _isIntDivV;

        private enum OverBorderMethods 
        { 
            UseDefaultColor,
            UseNearsetPosColor
        }

        private OverBorderMethods _overBorderMethods = OverBorderMethods.UseNearsetPosColor;

        private Color _defaultOverBorderColor = Color.black;


        private void _draw_TilesGen_UI()
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                _draw_subTitle_UI("------ 分割参数设定 ------");
                GUILayout.Space(5);

                if(!_srcTexture && !string.IsNullOrEmpty(_srcTexPath))
                {
                    tryBuildSrcTexture();
                }

                GUILayout.BeginHorizontal();
                {

                    GUILayout.Label("源图Path", GUILayout.Width(160));
                    string nSrcTexPath = EditorGUILayout.TextField(_srcTexPath);
                    if(_srcTexPath != nSrcTexPath)
                    {
                        _srcTexture = null;
                        _srcTexPath = nSrcTexPath;
                        _isExTexPath = false;
                    }
                    if(GUILayout.Button("SetFormSelection", GUILayout.Width(120)))
                    {
                        if(Selection.activeObject)
                        {
                            _srcTexture = null;
                            _srcTexPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                            _isExTexPath = false;
                            GUI.FocusControl(null);
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if(!_srcTexture && _isExTexPath)
                {
                    GUILayout.Space(5);
                    draw_ExTexParamsUI();
                }

                if(_srcTexture)
                {

                    GUILayout.Space(5);

                    draw_srcTex_previewUI();

                    GUILayout.Space(5);

                    draw_info_UI();
                }

                GUILayout.Space(5);
               
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("分割块数设置", GUILayout.Width(160));
                    Vector2Int s = new Vector2Int(_tileU, _tileV);
                    Vector2Int n = EditorGUILayout.Vector2IntField("", s);
                    if(!s.Equals(n))
                    {
                        _tileU = n.x;
                        _tileV = n.y;
                    }

                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("融合边界设置", GUILayout.Width(160));
                    Vector2Int s = new Vector2Int(_extU, _extV);
                    Vector2Int n = EditorGUILayout.Vector2IntField("", s);
                    if(!s.Equals(n))
                    {
                        _extU = n.x;
                        _extV = n.y;
                    }
                }
                GUILayout.EndHorizontal();
                if(!_extU.Equals(0) || !_extV.Equals(0))
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("启用全向融合", GUILayout.Width(160));
                        _DoubleSizeExt = EditorGUILayout.Toggle(_DoubleSizeExt, GUILayout.Width(50));
                        GUI.backgroundColor = Color.gray;
                        GUILayout.BeginVertical("box");
                        {
                            string tipStr = _DoubleSizeExt ? "全向融合<color=#ffff00>(上下左右)</color>" : "默认融合<color=#ffff00>(右上)</color>";
                            GUILayout.Label($"融合边计算方式 : {tipStr}", sTipStyle);
                        }
                        GUILayout.EndVertical();
                        GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("超出边界像素处理方式", GUILayout.Width(160));
                        _overBorderMethods = (OverBorderMethods)EditorGUILayout.EnumPopup(_overBorderMethods);
                    }
                    GUILayout.EndHorizontal();

                    if(_overBorderMethods == OverBorderMethods.UseDefaultColor)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("默认超出边界颜色", GUILayout.Width(160));
                            _defaultOverBorderColor = EditorGUILayout.ColorField(_defaultOverBorderColor);
                        }
                        GUILayout.EndHorizontal();
                    }

                }

                GUILayout.Space(5);

                draw_FileNameTmpUI();

                GUILayout.FlexibleSpace();

                if(_vaildInputData())
                {
                    GUI.color = Color.yellow;
                    if(GUILayout.Button("Start", GUILayout.Height(28)))
                    {
                        if(EditorUtility.DisplayDialog("提示", "确定开始创建分割Tiles?", "确定", "取消"))
                        {
                            tilesGenProcess();
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.gray;
                    if(GUILayout.Button("Start", GUILayout.Height(28)))
                    {
                        //do nothing ...
                    }
                    GUI.color = Color.white;
                }

            }
            GUILayout.EndVertical();

        }

        //=============================================================================

        private Vector2Int _exTexSize = new Vector2Int(1025, 1025);
        private ColorChannel _exTexChannel = ColorChannel.ChannelR;
        private ColorBit _exTexCBit = ColorBit.Bit8;

        private void draw_ExTexParamsUI()
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("RAW图像 Options ");
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Size", GUILayout.Width(160));
                    _exTexSize = EditorGUILayout.Vector2IntField("", _exTexSize);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Color Channel", GUILayout.Width(160));
                    _exTexChannel = (ColorChannel)EditorGUILayout.EnumPopup(_exTexChannel);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Color Bits", GUILayout.Width(160));
                    _exTexCBit = (ColorBit)EditorGUILayout.EnumPopup(_exTexCBit);
                }
                GUILayout.EndHorizontal();
                if(GUILayout.Button("创建Raw图像"))
                {

                    byte[] bytes = AorIO.ReadBytesFormFile(_srcTexPath);
                    if(bytes != null && bytes.Length > 0)
                    {
                        TextureFormat tf = TextureFormat.RGBA32;
                        if(!string.IsNullOrEmpty(_srcTexExt))
                        {
                            switch(_srcTexExt)
                            {
                                case ".raw":
                                    {
                                        tf = TextureFormat.R16;
                                    }
                                    break;
                                case ".r16":
                                    {
                                        tf = TextureFormat.R16;
                                    }
                                    break;
                                case ".r32":
                                    {
                                        tf = TextureFormat.RFloat;
                                    }
                                    break;
                            }
                        }
                        _srcTexture = new Texture2D(_exTexSize.x, _exTexSize.y, tf, false);
                        _srcTexture.LoadRawTextureData(bytes);
                        _srcTexture.Apply();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("提示", $"从{_srcTexPath}中读取数据失败!\n请确认源图路径是否正确?", "确定");
                    }

                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

        private void draw_info_UI()
        {
            //计算 Tile Size
            float w = (float)_srcTexture.width / _tileU;
            float w2 = Mathf.Floor(w);
            float h = (float)_srcTexture.height / _tileV;
            float h2 = Mathf.Floor(h);
            _isIntDivU = w.Equals(w2);
            _isIntDivV = h.Equals(h2);
            _tileW = (int)w2;
            _tileH = (int)h2;
            _tileWe = _tileW + (_DoubleSizeExt ? _extU * 2 : _extU);
            _tileHe = _tileH + (_DoubleSizeExt ? _extV * 2 : _extV);

            GUILayout.BeginVertical("box");
            {

                GUILayout.Space(5);
                GUILayout.Label("Info :");
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("源图格式:", sTipStyle, GUILayout.Width(160));
                    GUILayout.Label($"{_srcTexture.format.ToString()}", sTipStyle, GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(20);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("源图尺寸:", sTipStyle, GUILayout.Width(160));
                    GUILayout.Label("U", sTipStyle);
                    GUILayout.Label($"{_srcTexture.width}", sTipStyle, GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("V", sTipStyle);
                    GUILayout.Label($"{_srcTexture.height}", sTipStyle, GUILayout.Width(100));
                    GUILayout.Space(20);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Tile尺寸:", sTipStyle, GUILayout.Width(160));
                    GUILayout.Label("U", sTipStyle);
                    GUILayout.Label((_isIntDivU ? _tileWe.ToString() : $"<color=#ffaa00>{_tileWe} (像素丢弃)</color>"), sTipStyle, GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("V", sTipStyle);
                    GUILayout.Label((_isIntDivV ? _tileHe.ToString() : $"<color=#ffaa00>{_tileHe} (像素丢弃)</color>"), sTipStyle, GUILayout.Width(100));
                    GUILayout.Space(20);
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }

        private void draw_srcTex_previewUI()
        {

            GUILayout.BeginVertical("box");
            {
                GUILayout.Space(12);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("源图", GUILayout.Width(160));
                    Texture2D nSrcTexture = (Texture2D)EditorGUILayout.ObjectField("", _srcTexture, typeof(Texture2D), false);
                    if(_srcTexture != nSrcTexture)
                    {
                        _srcTexture = nSrcTexture;
                        _srcTexPath = AssetDatabase.GetAssetPath(_srcTexture);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(12);
            }
            GUILayout.EndVertical();

        }

        private bool _isExTexPath;
        private string _srcTexExt;
        private EditorAssetInfo _srcTexPathInfo;

        private void tryBuildSrcTexture()
        {
            _srcTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_srcTexPath);
            _srcTexPathInfo = new EditorAssetInfo(_srcTexPath);
            _srcTexExt = _srcTexPathInfo.extension;
            if(!_srcTexture)
            {
                switch(_srcTexPathInfo.extension)
                {
                    case ".raw":
                        {
                            _exTexCBit = ColorBit.Bit16;
                            _isExTexPath = true;
                        }
                    break;
                    case ".r16":
                        {
                            _exTexCBit = ColorBit.Bit16;
                            _isExTexPath = true;
                        }
                        break;
                    case ".r32":
                        {
                            _exTexCBit = ColorBit.Bit32;
                            _isExTexPath = true;
                        }
                        break;
                    default:
                        {
                            _isExTexPath = false;
                        }
                        break;
                }
            }
        }

        private void tilesGenProcess()
        {

            if(m_useSrcTexPathDir)
                verifySavePath();

            //检查源图可读写
            bool readableDirty = false;
            bool fiterModeDitry = false;
            FilterMode orgFilterMode = FilterMode.Bilinear;
            //
            string srcPath = AssetDatabase.GetAssetPath(_srcTexture);
            TextureImporter textureImporter = null;
            if(!string.IsNullOrEmpty(srcPath))
            {
                textureImporter = (TextureImporter)TextureImporter.GetAtPath(srcPath);
                if(!textureImporter)
                {
                    EditorUtility.DisplayDialog("错误提示", "源图不可读写且无法加载源图的TextureImporter,操作被中断.", "确定");
                    return;
                }
                if(!textureImporter.isReadable)
                {
                    textureImporter.isReadable = true;
                    readableDirty = true;
                }
                if(textureImporter.filterMode != FilterMode.Point)
                {
                    orgFilterMode = textureImporter.filterMode;
                    textureImporter.filterMode = FilterMode.Point;
                    fiterModeDitry = true;
                }

                if(readableDirty || fiterModeDitry)
                {
                    textureImporter.SaveAndReimport();
                    _srcTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
                }
            }

            //初始化文件名模板参数 
            initFileNameTmp();

            //
            int pLen = _tileV * _tileU;
            int pIdx = 1;

            for(int v = 0; v < _tileV; v++)
            {
                for(int u = 0; u < _tileU; u++)
                {

                    EditorUtility.DisplayProgressBar("正在创建...", $"正在创建Tile图 {pIdx} / {pLen}", (float)pIdx / pLen);

                    string tileName = getTileFileName(u, v);
                    string savePath = m_savePath + $"/{tileName}";

                    int x = u * _tileW;
                    int y = v * _tileH;
                    if(_DoubleSizeExt)
                    {
                        x -= _extU;
                        y -= _extV;
                    }

                    Texture2D tile = creatTex2D(ref _srcTexture, x, y, _tileWe, _tileHe,(u2, u2Min, v2, v2Min, @out)=> 
                    {
                        //超出采样范围处理
                        Color c = _defaultOverBorderColor;
                        if(_overBorderMethods == OverBorderMethods.UseNearsetPosColor)
                            c = getNearsetPosColor(ref _srcTexture, u2, v2);
                        @out.SetPixel(u2 - u2Min, v2 - v2Min, c);
                    });

                    //save 
                    tile.name = tileName;
                    byte[] bytes = null;
                    switch(_srcTexExt)
                    {
                        case ".raw":
                        case ".r16":
                        case ".r32":
                            {
                                bytes = tile.GetRawTextureData();
                            }
                        break;
                        case ".tga":
                            {
                                bytes = tile.EncodeToTGA();
                            }
                        break;
                        case ".jpg":
                            {
                                bytes = tile.EncodeToJPG();
                            }
                        break;
                        default:
                            {
                                bytes = tile.EncodeToPNG();
                            }
                        break;
                    }
                        //= tile.EncodeToPNG();
                    AorIO.SaveBytesToFile(savePath + _srcTexExt, bytes);

                    AssetDatabase.Refresh();

                    switch(_srcTexExt)
                    {
                        case ".raw":
                        case ".r16":
                        case ".r32":
                            {
                                //do nothing ...
                            }
                            break;
                        default:
                            {
                                TextureImporter subImporter = (TextureImporter)TextureImporter.GetAtPath(savePath + _srcTexExt);
                                if(subImporter)
                                {
                                    subImporter.wrapMode = TextureWrapMode.Clamp;
                                    subImporter.npotScale = TextureImporterNPOTScale.None;
                                    subImporter.SaveAndReimport();
                                }
                            }
                        break;
                    }

                    pIdx++;
                }
            }
            EditorUtility.ClearProgressBar();

            if(readableDirty || fiterModeDitry)
            {
                if(readableDirty)
                    textureImporter.isReadable = false;
                if(fiterModeDitry)
                    textureImporter.filterMode = orgFilterMode;

                textureImporter.SaveAndReimport();
            }

            AssetDatabase.Refresh();
        }

        private Texture2D creatTex2D(ref Texture2D inputTex2D, int x, int y, int outTexWidth, int outTexHeight, Action<int,int,int,int,Texture2D> onOverPosDo)
        {
            Color color = Color.black;
            Texture2D @out = new Texture2D(outTexWidth, outTexHeight, inputTex2D.format, false);

            int vMin = y;
            int vMax = vMin + outTexHeight;
            int uMin = x;
            int uMax = uMin + outTexWidth;
            for(int v = vMin; v < vMax; v++)
            {
                for(int u = uMin; u < uMax; u++)
                {
                    if(u >= 0 && u < inputTex2D.width && v >= 0 && v < inputTex2D.height)

                        @out.SetPixel(u - uMin, v - vMin, inputTex2D.GetPixel(u, v));
                    else
                        onOverPosDo(u, uMin, v, vMin, @out);
                }
            }
            @out.Apply();
            return @out;
        }

        private Color getNearsetPosColor(ref Texture2D inputTex2D, int u, int v)
        {
            Color c = new Color(0, 0, 0, 1f);

            int limit = Mathf.Max(_extU, _extV);

            for(int i = 1; i <= limit; i++)
            {
                int u2 = u + i;
                int v2 = v;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u;
                v2 = v + i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u - i;
                v2 = v;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u;
                v2 = v - i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u + i;
                v2 = v + i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u - i;
                v2 = v + i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u - i;
                v2 = v - i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);

                u2 = u + i;
                v2 = v - i;
                if(u2 >= 0 && u2 < inputTex2D.width && v2 >= 0 && v2 < inputTex2D.height)
                    return inputTex2D.GetPixel(u2, v2);
            }
            
            return c;
        }



        private bool _showFntTipUI;
        private string _fileNameTmp;

        private string _fnt_tipContents = "<size=14>输出文件命名模板使用帮助:</size>\n"
                + "\n"
                + "\t命名模板使用特定的<color=#00ffff>标签</color>指代输出文件时可用的变量.\n"
                + "\t<color=#ffff00>{x}</color>\t:\t<color=#00ffff>输出Tile的X id</color>\n"
                + "\t<color=#ffff00>{y}</color>\t:\t<color=#00ffff>输出Tile的Y id</color>\n"
                + "\t例:\n"
                + "\t\t使用命名模板:<color=#ffff00>{x}_{y}</color>,你将得到<color=#00ffff>0_1</color>这样的文件命名.\n"
                + "\t\t使用命名模板:<color=#ffff00>{x:2}_{y:3}</color>,你将得到<color=#00ffff>00_001</color>这样的文件命名.\n"
                + "\t<color=#ffff00>{n}</color> / <color=#ffff00>{name}</color>\t:\t<color=#00ffff>源图名称</color>.\n"
                + "\t<color=#ffff00>{dir}</color>\t:\t<color=#00ffff>输入路径所在的文件夹名称</color>.\n"
            ;
        private void draw_FileNameTmpUI()
        {

            if(_showFntTipUI)
            {
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Space(5);
                    GUILayout.Label(_fnt_tipContents, sTipStyle);
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if(GUILayout.Button("Close This Help Tip"))
                        {
                            _showFntTipUI = false;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("输出文件命名模板", GUILayout.Width(160));
                if(string.IsNullOrEmpty(_fileNameTmp))
                {
                    _fileNameTmp = EditorPrefs.GetString("Framework.Editor.Utility.TexTileGenToolWindow.fileNameTmp");
                    if(string.IsNullOrEmpty(_fileNameTmp))
                    {
                        _fileNameTmp = "{x}_{y}";
                        EditorPrefs.SetString("Framework.Editor.Utility.TexTileGenToolWindow.fileNameTmp", _fileNameTmp);
                    }
                }
                string n = EditorGUILayout.TextField(_fileNameTmp);
                if(n != _fileNameTmp)
                {
                    _fileNameTmp = n;
                    EditorPrefs.SetString("Framework.Editor.Utility.TexTileGenToolWindow.fileNameTmp", _fileNameTmp);
                }
                if(!_showFntTipUI)
                {
                    if(GUILayout.Button("?", GUILayout.Width(22)))
                    {
                        _showFntTipUI = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private int _fn_id_x;
        private int _fn_id_y;
        private string _fn_tmp;
        private void initFileNameTmp()
        {
            _fn_id_x = _fn_id_y = 1;
            _fn_tmp = _fileNameTmp;
            var mX = Regex.Match(_fn_tmp, @"{x:(\d+)}", RegexOptions.IgnoreCase);
            if(mX.Success)
            {
                _fn_id_x = int.Parse(mX.Groups[1].Value);
                _fn_tmp = Regex.Replace(_fn_tmp, @"{x:\d+}", "{x}");
            }
            var mY = Regex.Match(_fn_tmp, @"{y:(\d+)}", RegexOptions.IgnoreCase);
            if(mY.Success)
            {
                _fn_id_y = int.Parse(mY.Groups[1].Value);
                _fn_tmp = Regex.Replace(_fn_tmp, @"{y:\d+}", "{y}");
            }

            _fn_tmp = Regex.Replace(_fn_tmp, "{name}", _srcTexPathInfo.name, RegexOptions.IgnoreCase);
            _fn_tmp = Regex.Replace(_fn_tmp, "{n}", _srcTexPathInfo.name, RegexOptions.IgnoreCase);
            _fn_tmp = Regex.Replace(_fn_tmp, "{file}", _srcTexPathInfo.name, RegexOptions.IgnoreCase);

            EditorAssetInfo info = new EditorAssetInfo(m_savePath);
            _fn_tmp = Regex.Replace(_fn_tmp, "{dir}", info.dirName, RegexOptions.IgnoreCase);
        }

        private string getStrNum(int n, int f = 1)
        {
            string v = n.ToString();
            f = f - v.Length;
            f = Mathf.Max(0, f);
            for(int i = f; i > 0; i--)
            {
                v = "0" + v;
            }
            return v;
        }

        private string getTileFileName(int u, int v)
        {
            string tn = _fn_tmp.Replace("{x}", getStrNum(u, _fn_id_x));
            tn = tn.Replace("{y}", getStrNum(v, _fn_id_y));
            return tn;
        }

        private bool _vaildInputData()
        {

            if(!m_useSrcTexPathDir && string.IsNullOrEmpty(m_savePath))
            {
                return false;
            }
            if(!_srcTexture)
            {
                return false;
            }
            return true;
        }

        private void _draw_subTitle_UI(string label)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, sTitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

    }
}


