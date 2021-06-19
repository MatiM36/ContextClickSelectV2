using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
public partial class CustomContextWindow : EditorWindow
{
    public struct ContextMenuItem
    {
        public Transform obj;
        public string label;
        public bool isPrefab;
    }

    static CustomContextWindow window;
    static ContextMenuItem noneItem = new ContextMenuItem() { label = "None" };

    private List<ContextMenuItem> items;

    public event Action<CustomContextWindow, ContextMenuItem, int> OnItemSelected;

    private bool pendingHorizontalResize;
    private Vector2 scrollPos;

    private int scrollViewHeight = -1;

    static public CustomContextWindow OpenSearchWindow(Vector2 screenPos, IEnumerable<ContextMenuItem> items)
    {
        if (window != null)
        {
            window.Close();
            window.OnItemSelected = null;
        }
        window = CreateInstance<CustomContextWindow>();

        window.items = items.ToList();

        window.maxSize = new Vector2(WINDOW_MIN_WIDTH, Mathf.Min(Mathf.Max(ITEM_HEIGHT * window.items.Count, ITEM_HEIGHT) + RESIZE_HEIGHT_MARGIN, WINDOW_MAX_HEIGHT));
        window.minSize = window.maxSize;

        window.position = new Rect(screenPos, window.maxSize);

        window.pendingHorizontalResize = true;

        window.ShowPopup();

        return window;
    }


    private void OnLostFocus()
    {
        if (window != null)
        {
            window.Close();
            window.OnItemSelected = null;
        }
    }

    private void OnGUI()
    {
        ResizeWindow();

        var windowSize = window.position.size;
        var fullRect = new Rect(Vector2.zero, windowSize);

        var prevColor = GUI.color;
        GUI.color = borderColor;
        GUI.DrawTexture(fullRect, EditorGUIUtility.whiteTexture); //BG

        fullRect.xMin += windowBorder.left;
        fullRect.xMax -= windowBorder.right;
        fullRect.yMin += windowBorder.top;
        fullRect.yMax -= windowBorder.bottom;

        GUI.color = bgColor;
        GUI.DrawTexture(fullRect, EditorGUIUtility.whiteTexture); //BG
        GUI.color = prevColor;

        var botRect = new Rect(fullRect);
        if (scrollViewHeight == -1)
            scrollViewHeight = (int)botRect.height;

        //
        //SCROLL BAR
        //
        var color = GUI.backgroundColor;
        bool scrollBarShown = scrollViewHeight > botRect.height;
        float itemWidth = scrollBarShown ? botRect.width - 16 : botRect.width;
        scrollPos = GUI.BeginScrollView(botRect, scrollPos, new Rect(botRect.position, new Vector2(itemWidth, scrollViewHeight)));

        var itemRect = new Rect(0, botRect.y, itemWidth, ITEM_HEIGHT);

        scrollViewHeight = 0;

        GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);

        if (items.Count == 0)
        {
            scrollViewHeight += ITEM_HEIGHT;
            if (DrawItem(ref itemRect, 0, noneItem))
                SelectAsset(noneItem, -1);
        }
        else
        {
            //Draw Items
            int i = 0;
            foreach (var item in items)
            {
                scrollViewHeight += ITEM_HEIGHT;

                if (item.isPrefab)
                {
                    if (DrawPrefabItem(ref itemRect, i, item))
                        SelectAsset(item, i);
                }
                else
                {
                    if (DrawItem(ref itemRect, i, item))
                        SelectAsset(item, i);
                }

                i++;
            }
        }
        GUI.EndScrollView();
    }

    private void ResizeWindow()
    {
        if (pendingHorizontalResize)
        {
            Rect maxWidthRect = new Rect(0, 0, 0, 0);
            foreach (var item in items)
            {
                var name = item.label;
                var rect = GUILayoutUtility.GetRect(new GUIContent(name), ItemStyle, GUILayout.ExpandWidth(false));
                if (rect.width > maxWidthRect.width)
                    maxWidthRect = rect;
            }

            maxWidthRect.width += RESIZE_WIDTH_MARGIN;
            if (maxWidthRect.width > window.maxSize.x)
            {
                window.maxSize = new Vector2(Mathf.Max(maxWidthRect.width, WINDOW_MIN_WIDTH), window.maxSize.y);
                window.minSize = window.maxSize;
                pendingHorizontalResize = true;
            }
        }
    }

    private void Update()
    {
        Repaint(); //For immediatly applying hover/click feedback
    }


    private bool DrawItem(ref Rect rect, int index, ContextMenuItem item)
    {
        if (index % 2 != 0)
            EditorGUI.DrawRect(rect, alternateColor);

        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = selectedColor;
        bool pressed = GUI.Button(rect, item.label, ItemStyle);
        GUI.backgroundColor = prevColor;

        rect.y += ITEM_HEIGHT;
        return pressed;
    }

    private bool DrawPrefabItem(ref Rect rect, int index, ContextMenuItem item)
    {
        if (index % 2 != 0)
            EditorGUI.DrawRect(rect, alternateColor);

        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = selectedColor;
        bool pressed = GUI.Button(rect, item.label, ItemStyle);
        GUI.backgroundColor = prevColor;

        var iconRect = new Rect(rect);
        iconRect.xMin = 0;
        iconRect.xMax = 6;
        EditorGUI.DrawRect(iconRect, selectedColor);

        rect.y += ITEM_HEIGHT;
        return pressed;
    }

    private void SelectAsset(ContextMenuItem item, int index)
    {
        OnItemSelected?.Invoke(this, item, index);
        window.Close();
    }
}
