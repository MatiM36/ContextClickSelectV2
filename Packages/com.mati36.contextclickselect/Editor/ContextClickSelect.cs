using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static CustomContextWindow;

[InitializeOnLoad]
static public class ContextClickSelect
{
    const int MAX_OBJ_FOUND = 100;
    const string LEVEL_SEPARATOR = "          ";
    const string PREFAB_TAG = ""; //🔲🔳🔷❒♦🐱‍👤📦

    static bool mouseBtnPressed = false;
    static Vector2 mouseDownPos;
    static Dictionary<Transform, List<Transform>> parentChildsDict = new Dictionary<Transform, List<Transform>>();

    static MethodInfo Internal_PickClosestGO;

    static ContextClickSelect()
    {
        if (EditorApplication.isPlaying) return;
        SceneView.beforeSceneGui -= OnSceneGUI;
        SceneView.beforeSceneGui += OnSceneGUI;
        CacheMethods();
    }


    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            mouseDownPos = e.mousePosition;
            mouseBtnPressed = true;
        }
        else if (e.type == EventType.MouseUp && e.button == 1 && mouseBtnPressed)
        {
            mouseBtnPressed = false;
            if (mouseDownPos == e.mousePosition)
            {
                ShowOverlappingGameobjects(e.mousePosition, sceneView);
                //e.Use();
            }
        }

        //FOR DEBUG:
        //Handles.BeginGUI();
        //var rect = new Rect(e.mousePosition, new Vector2(300, 20));
        //EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.8f));
        //GUI.Label(rect, $"  windowPos {e.mousePosition} / w: {sceneView.position.width} h: {sceneView.position.height}");
        //var rect2 = new Rect(e.mousePosition + new Vector2(0, 30), new Vector2(300, 20));
        //EditorGUI.DrawRect(rect2, new Color(0.5f, 0.5f, 0.5f, 0.8f));
        //GUI.Label(rect2, $"  cameraPos {ScreenToCameraMousePos(e.mousePosition, sceneView)} / w: {sceneView.camera.pixelWidth} h: {sceneView.camera.pixelHeight}");
        //Handles.EndGUI();
    }

    static void ShowOverlappingGameobjects(Vector2 mousePos, SceneView sceneView)
    {
        //The mouse pos from the event is relative to the window, but the one we need for the pick method is relative to the camera
        //Why the camera and the window aren't the same pixel size? idk
        var mousePosCameraRelative = ScreenToCameraMousePos(mousePos, sceneView);

        parentChildsDict.Clear();
        GameObject objFound = null;
        GameObject[] ignore = null;

        for (int i = 0; i <= MAX_OBJ_FOUND; i++)
        {
            if (parentChildsDict.Count > 0)
                ignore = parentChildsDict.Select(obj => obj.Key.gameObject).ToArray();

            objFound = PickObjectOnPos(sceneView.camera, ~0, mousePosCameraRelative, ignore, null);
            if (objFound != null)
            {
                parentChildsDict[objFound.transform] = new List<Transform>();

                var currentParent = objFound.transform.parent;
                var lastParent = objFound.transform;
                while (currentParent != null)
                {
                    if (parentChildsDict.TryGetValue(currentParent, out var currentChilds))
                    {
                        currentChilds.Add(lastParent);
                        break;
                    }
                    else
                        parentChildsDict.Add(currentParent, new List<Transform>() { lastParent });

                    lastParent = currentParent;
                    currentParent = currentParent.parent;
                }
            }
            else
                break;
        }

        var mouseScreenPos =  GUIUtility.GUIToScreenPoint(mousePos);

        Transform selected = Selection.activeTransform;
        bool hasSelection = selected != null;

        var window = CustomContextWindow.OpenSearchWindow(
            mouseScreenPos,
            parentChildsDict.Keys
            .Where(t => t.parent == null)
            .SelectMany(t => EnumerateObjectsAsTree(t, string.Empty, parentChildsDict))
            );
        window.OnItemSelected += OnItemSelected;
    }

    static IEnumerable<ContextMenuItem> EnumerateObjectsAsTree(Transform current, string currentPath, Dictionary<Transform, List<Transform>> parentChilds)
    {        yield return new ContextMenuItem() { obj = current, label = currentPath + current.name, isPrefab = IsPrefab(current.gameObject) };

        if (!parentChilds.TryGetValue(current, out var childs)) yield break;
        if (childs == null) yield break;
        for (int i = 0; i < childs.Count; i++)
        {
            foreach (var element in EnumerateObjectsAsTree(childs[i], currentPath + LEVEL_SEPARATOR, parentChilds))
                yield return element;
        }
    }

    private static void OnItemSelected(CustomContextWindow window, ContextMenuItem item, int index)
    {
        if (item.obj != null)
            Selection.activeTransform = item.obj;
    }

    static bool IsPrefab(GameObject obj)
    {
        var prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
        return prefab != null && prefab == obj;
    }

    static Vector2 ScreenToCameraMousePos(Vector2 mousePos, SceneView sceneView)
    {
        return new Vector2((mousePos.x / sceneView.position.width) * sceneView.camera.pixelWidth, sceneView.camera.pixelHeight - (mousePos.y / sceneView.position.height * sceneView.camera.pixelHeight));
    }

    static GameObject PickObjectOnPos(Camera cam, int layers, Vector2 position, GameObject[] ignore, GameObject[] filter)
    {
        return (GameObject)Internal_PickClosestGO.Invoke(null, new object[] { cam, layers, position, ignore, filter, -1 });
    }

    static void CacheMethods()
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        System.Type handleUtilityType = editorAssembly.GetType("UnityEditor.HandleUtility");
        Internal_PickClosestGO = handleUtilityType.GetMethod("Internal_PickClosestGO", BindingFlags.Static | BindingFlags.NonPublic);
    }
}
