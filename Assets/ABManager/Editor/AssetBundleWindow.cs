#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class AssetBundleWindow : EditorWindow
{
    [SerializeField] TreeViewState m_TreeViewState;
    SimpleTreeView m_SimpleTreeView;

    [MenuItem("Window/PleIQ ABManager")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleWindow>(false, "PleIQ ABManager", true);
    }

    void OnEnable()
    {
        TreeViewStateHandler();
    }

    void OnFocus()
    {
        TreeViewStateHandler();
    }

    void OnGUI()
    {     
        var names = AssetDatabase.GetAllAssetBundleNames();
        if (names.Length == 0)
        {
            EditorGUILayout.HelpBox("No se encontraron Assets Bundles.", MessageType.Info, true);
        }
        else
        {

            /*foreach (var name in names)
            {
                EditorGUILayout.LabelField("- " + name);                
            }*/
           

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (GUILayout.Button("Build para " + buildTarget.ToString(),GUILayout.Height(40)))
                CreateAssetBundles.BuildAllAssetBundle();

            EditorGUILayout.LabelField("Listado de Asset Bundles", EditorStyles.boldLabel);

            m_SimpleTreeView.OnGUI(new Rect(0, 60, position.width, position.height));
            
            //GUILayout.FlexibleSpace();

        }
    }


    void TreeViewStateHandler()
    {
        if (m_TreeViewState == null)
            m_TreeViewState = new TreeViewState();

            m_SimpleTreeView = new SimpleTreeView(m_TreeViewState);        
    }
}

class SimpleTreeView : TreeView
{
    private Dictionary<int, string> assetLocation = new Dictionary<int, string>();
    public SimpleTreeView(TreeViewState treeViewState) : base(treeViewState)
    {
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        var allItems = new List<TreeViewItem>();
        int i = 0;
        foreach (var name in names)
        {
            allItems.Add(new TreeViewItem(i, 0, "Bundle: " + name));
            foreach (var assetPathAndName in AssetDatabase.GetAssetPathsFromAssetBundle(name))
            {
                i++;
                string nameWithoutPath = assetPathAndName.Substring(assetPathAndName.LastIndexOf("/") + 1);
                string aname = nameWithoutPath.Substring(0, nameWithoutPath.LastIndexOf("."));
                assetLocation.Add(i, assetPathAndName);
                allItems.Add(new TreeViewItem(i, 1, aname));
            }
            i++;
        }
        SetupParentsAndChildrenFromDepths(root, allItems);
        return root;
    }

    protected override void DoubleClickedItem(int id)
    {
        if (assetLocation.ContainsKey(id))
        {
            Object obj = AssetDatabase.LoadAssetAtPath(assetLocation[id], typeof(UnityEngine.Object));
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
#endif
