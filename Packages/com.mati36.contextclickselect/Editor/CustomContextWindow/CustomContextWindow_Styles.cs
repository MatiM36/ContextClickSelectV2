using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class CustomContextWindow : EditorWindow
{
    const int ITEM_HEIGHT = 20;
    const int WINDOW_MIN_WIDTH = 200;
    const int WINDOW_MAX_HEIGHT = 400;

    const int RESIZE_WIDTH_MARGIN = 32;
    const int RESIZE_HEIGHT_MARGIN = 2;

    static Color borderColor = new Color(0, 0, 0, 0.4f);
    static Color bgColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    static Color selectedColor = new Color(0.05490196f, 0.427451f, 0.7960785f, 1f);
    static Color alternateColor = new Color(0, 0, 0, 0.05f);
    static RectOffset windowBorder = new RectOffset(1, 1, 1, 1);

    static Texture2D _transparentBG;
    static Texture2D TransparentBG
    {
        get
        {
            if (_transparentBG == null)
            {
                _transparentBG = new Texture2D(1, 1);
                _transparentBG.SetPixel(0, 0, Color.clear);
                _transparentBG.Apply();
            }

            return _transparentBG;
        }
    }

    static Texture2D WhiteBG { get { return EditorGUIUtility.whiteTexture; } }

    static private GUIStyle _itemStyle;
    static public GUIStyle ItemStyle
    {
        get
        {
            if (_itemStyle == null)
            {
                _itemStyle = new GUIStyle();

                _itemStyle.active.background = TransparentBG;
                _itemStyle.focused.background = TransparentBG;
                _itemStyle.hover.background = WhiteBG;
                _itemStyle.normal.background = TransparentBG;

                _itemStyle.onActive.background = TransparentBG;
                _itemStyle.onFocused.background = TransparentBG;
                _itemStyle.onHover.background = WhiteBG;

                _itemStyle.onNormal.background = TransparentBG;

                _itemStyle.hover.textColor = Color.white;
                _itemStyle.onHover.textColor = Color.white;

                _itemStyle.padding = new RectOffset(12, 4, 0, 0);

                _itemStyle.richText = false;
                _itemStyle.alignment = TextAnchor.MiddleLeft;
                _itemStyle.font = EditorStyles.standardFont;
            }
            return _itemStyle;
        }
    }
}
