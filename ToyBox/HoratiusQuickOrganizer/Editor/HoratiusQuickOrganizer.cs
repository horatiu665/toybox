using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.Linq;

public class HoratiusQuickOrganizer : EditorWindow
{
    //string myString = "Hello World";
    //bool groupEnabled;
    //bool myBool = true;
    //float myFloat = 1.23f;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/Horatiu's Quick Organizer Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        HoratiusQuickOrganizer window = (HoratiusQuickOrganizer)EditorWindow.GetWindow(typeof(HoratiusQuickOrganizer));
        window.titleContent = new GUIContent("Organizer");
        var minSize = window.minSize;
        minSize.y = 24;
        window.minSize = minSize;
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Organize Selected Assets"))
        {
            OrganizeAssets();
        }

        if (GUILayout.Button("Chaos! Flatten File Hierarchy"))
        {
            FlattenHierarchy();
        }

        // default example from http://docs.unity3d.com/ScriptReference/EditorWindow.html
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);
        // cool advanced options selector:
        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }

    private void FlattenHierarchy()
    {
        var knownAssetList = GetAllAssetsInSelectedFolders();
        if (knownAssetList.Count > 0)
        {
            // move all assets from this list into a folder sibling to the outermost folder selected.
            // create that folder first

            var outsideFolderPath = "";
            // get outmost folder path among selected folders
            //if (false) {
            //    outsideFolderPath = knownAssetList.Aggregate((hai1, hai2) =>
            //            Path.GetDirectoryName(hai1.path).Count(c => (c == Path.DirectorySeparatorChar)) +
            //            Path.GetDirectoryName(hai1.path).Count(c => (c == Path.AltDirectorySeparatorChar)) <
            //            Path.GetDirectoryName(hai2.path).Count(c => (c == Path.DirectorySeparatorChar)) +
            //            Path.GetDirectoryName(hai2.path).Count(c => (c == Path.AltDirectorySeparatorChar)) ? hai1 : hai2
            //        ).path;
            //}
            //if (false) {
            //    outsideFolderPath = "Assets/";
            //}

            // find common ancestor
            outsideFolderPath = GetCommonAncestor(knownAssetList.Select(ai => ai.path).ToList());

            var targetFolder = outsideFolderPath;
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            foreach (var a in knownAssetList)
            {
                AssetDatabase.MoveAsset(a.path, targetFolder + "/" + a.fileName);
            }

            Debug.Log(outsideFolderPath);
            AssetDatabase.Refresh();

        }
    }

    private string GetCommonAncestor(List<string> assetPathList)
    {
        var minAncestry = assetPathList.Min(path => GetAssetAncestry(path));
        if (minAncestry == 0)
            return "Assets";

        for (int i = 0; i < assetPathList.Count; i++)
        {
            while (GetAssetAncestry(assetPathList[i]) > minAncestry)
            {
                assetPathList[i] = Path.GetDirectoryName(assetPathList[i]);
            }
        }

        assetPathList = assetPathList.Distinct().ToList();

        // find folder of said ancestry
        if (assetPathList.Count() == 1)
        {
            var p = (assetPathList.First());
            if (IsPathAFolder(p))
            {
                return p;
            }
            else
            {
                return Path.GetDirectoryName(p);
            }
        }
        else
        {
            // must find common parent of those guys
            return GetCommonAncestor(assetPathList.Select(s => Path.GetDirectoryName(s)).ToList());
        }
    }

    /// <summary>
    /// Finds the amount of separator characters in the asset name and calculates the ancestry, useful for finding the common ancestor of multiple assets.
    /// </summary>
    /// <param name="path">path of the asset</param>
    /// <returns>number of slashes from the Assets folder, zero being a child of the root</returns>
    private static int GetAssetAncestry(string assetPath)
    {
        var a = assetPath.Count(c => (c == Path.DirectorySeparatorChar) || (c == Path.AltDirectorySeparatorChar));
        //Debug.Log("Asset " + assetPath + " has ancestry" + a);
        return a;
    }

    [MenuItem("Tools/Horatiu's Quick Organize Selection")]
    private static void OrganizeAssets()
    {
        // if selection is a folder, organize all assets in that folder, but not in any children folders.
        if (Selection.objects.Length == 1 && IsAssetAFolder(Selection.activeObject))
        {
            // for each asset in selected folder, organize it.

            // put all assets in folder in this list
            List<HoratiusAssetInfo> assetsWithKnownTypes = GetAllAssetsInSelectedFolders();

            foreach (var assetInfo in assetsWithKnownTypes)
            {
                OrganizeAsset(assetInfo);
            }

        }
        // else, organize selected assets only, and put them in folders within their parent folder (or the root)
        else
        {
            foreach (var objSelected in Selection.objects)
            {
                var assetPath = AssetDatabase.GetAssetPath(objSelected);
                var assetInfo = new HoratiusAssetInfo(objSelected, assetPath, objSelected.GetType());
                OrganizeAsset(assetInfo);

            }
        }
    }

    [MenuItem("Tools/Horatiu's Quick Organize Selection", validate = true)]
    private static bool OrganizeAssetsValidateFunction()
    {
        var obj = Selection.objects;
        if (obj.Any(o => !AssetDatabase.Contains(o)))
        {
            return false;
        }
        return (Selection.objects.Length >= 1);
    }

    /// <summary>
    ///  puts each asset from folder in the list
    /// </summary>
    /// <returns></returns>
    private static List<HoratiusAssetInfo> GetAllAssetsInSelectedFolders()
    {
        List<HoratiusAssetInfo> assetsWithKnownTypes = new List<HoratiusAssetInfo>();

        foreach (var objSelected in Selection.objects)
        {
            if (IsAssetAFolder(objSelected))
            {
                // taken from http://answers.unity3d.com/questions/234935/how-do-i-enumerate-the-contents-of-an-asset-folder.html

                string sAssetFolderPath = AssetDatabase.GetAssetPath(objSelected);
                // Construct the system path of the asset folder 
                string sDataPath = Application.dataPath;
                string sFolderPath = sDataPath.Substring(0, sDataPath.Length - 6) + sAssetFolderPath;
                // get the system file paths of all the files in the asset folder
                string[] aFilePaths = Directory.GetFiles(sFolderPath);
                // enumerate through the list of files loading the assets they represent and getting their type

                foreach (string sFilePath in aFilePaths)
                {
                    string sAssetPath = sFilePath.Substring(sDataPath.Length - 6);
                    //Debug.Log("Path: " + sAssetPath);

                    Object objAsset = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(Object));

                    if (objAsset == null)
                    {
                        if (sAssetPath.EndsWith(".meta"))
                        {
                            //Debug.Log("Type: Meta file");
                        }
                        else
                        {
                            //Debug.Log("Type: Null asset");
                        }
                    }
                    else
                    {
                        var hai = new HoratiusAssetInfo(objAsset, sAssetPath, objAsset.GetType());
                        assetsWithKnownTypes.Add(hai);
                        //Debug.Log("Type: " + objAsset.GetType().Name + " a.k.a. " + hai.assetType);

                    }
                }

                // add folder too
                assetsWithKnownTypes.Add(new HoratiusAssetInfo(objSelected, AssetDatabase.GetAssetPath(objSelected), objSelected.GetType()));
            }
            else
            {
                var hai = new HoratiusAssetInfo(objSelected, AssetDatabase.GetAssetPath(objSelected), objSelected.GetType());
                assetsWithKnownTypes.Add(hai);
            }
        }

        return assetsWithKnownTypes;
    }

    private static void OrganizeAsset(HoratiusAssetInfo assetInfo)
    {
        // highest level idea: put asset in a folder named according to the asset type.
        // if asset is already in such a folder, leave it, and move all other assets around it in appropriate folders, outside of the current folder.

        // is current folder of any known type?
        if (assetInfo.assetType != HoratiusAssetKnownTypes.Folder)
        {
            // using backslashes or forward slashes?

            string folderName = Path.GetDirectoryName(assetInfo.path);
            string directoryOfFolder = Path.GetDirectoryName(folderName);
            folderName = folderName.Remove(0, directoryOfFolder.Length > 0 ? directoryOfFolder.Length + 1 : 0);

            //Debug.Log(folderName);

            // if folder is named like a reserved type folder such as Materials or Prefabs
            if (AssetTypeToFolder.Values.Contains(folderName))
            {
                Debug.LogError("Folder name " + folderName + " at " + directoryOfFolder + " is reserved. Please rename!");
                // reorganize assets into sibling folders outside of the current folder, and only keep appropriate assets inside
            }
            else
            {
                // folder name is fine. move asset into folder appropriately named. if not exists, create.
                var appropriateFolderPath = Path.Combine(Path.Combine(directoryOfFolder, folderName), AssetTypeToFolder[assetInfo.assetType]);
                //Debug.Log(appropriateFolderPath);

                if (!Directory.Exists(appropriateFolderPath))
                {
                    // if directory with same name but lowercase exists, rename to correct case
                    Directory.CreateDirectory(appropriateFolderPath);
                    AssetDatabase.Refresh();
                }


                // move asset there
                AssetDatabase.MoveAsset(assetInfo.path, appropriateFolderPath + "/" + assetInfo.fileName);

            }
        }

    }

    // http://answers.unity3d.com/questions/472808/how-to-get-the-current-selected-folder-of-project.html
    private static bool IsAssetAFolder(Object obj)
    {
        string path = "";

        if (obj == null)
        {
            return false;
        }

        path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

        if (path.Length > 0)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    private static bool IsPathAFolder(string path)
    {
        if (path.Length == 0)
            return false;

        if (Path.HasExtension(path))
        {
            return false;
        }
        else
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static bool IsSameOrSubclass(System.Type potentialBase, System.Type potentialDescendant)
    {
        return potentialDescendant.IsSubclassOf(potentialBase)
               || potentialDescendant == potentialBase;
    }

    public enum HoratiusAssetKnownTypes
    {
        Unknown,
        Folder,
        Prefab,
        MonoScript,
        Material,
        SceneAsset,
        Shader,
        Texture,
        Model,
        AnimatorController,
        AnimationClip,
        TextAsset,
    }

    public static readonly Dictionary<HoratiusAssetKnownTypes, string> AssetTypeToFolder = new Dictionary<HoratiusAssetKnownTypes, string>() {
        {HoratiusAssetKnownTypes.Unknown, "Other"},
        {HoratiusAssetKnownTypes.Prefab, "Prefabs"},
        {HoratiusAssetKnownTypes.MonoScript, "Scripts"},
        {HoratiusAssetKnownTypes.Material, "Materials"},
        {HoratiusAssetKnownTypes.SceneAsset, "Scenes"},
        {HoratiusAssetKnownTypes.Shader, "Shaders"},
        {HoratiusAssetKnownTypes.Texture, "Textures"},
        {HoratiusAssetKnownTypes.Model, "Models"},
        {HoratiusAssetKnownTypes.AnimatorController, "AnimatorControllers"},
        {HoratiusAssetKnownTypes.AnimationClip, "Animations"},
        {HoratiusAssetKnownTypes.TextAsset, "Text"},
    };

    public class HoratiusAssetInfo
    {
        public Object asset;
        public HoratiusAssetKnownTypes assetType;
        public string path;
        public string fileName;

        public HoratiusAssetInfo(Object asset, string assetPath, System.Type type)
        {
            this.asset = asset;
            this.path = assetPath;
            DetermineType(assetPath, type);
            if (assetType != HoratiusAssetKnownTypes.Folder)
            {
                // using backslashes or forward slashes?
                var lastSlash = Mathf.Max(assetPath.LastIndexOf("/"), assetPath.LastIndexOf(@"\"));
                fileName = assetPath.Substring(lastSlash + 1, assetPath.Length - lastSlash - 1);
            }

        }

        void DetermineType(string assetPath, System.Type type)
        {
            if (IsAssetAFolder(asset))
            {
                assetType = HoratiusAssetKnownTypes.Folder;
            }
            else if (type == typeof(GameObject))
            {
                // could be prefab or something else that you can drag in the scene
                if (assetPath.EndsWith(".prefab"))
                {
                    assetType = HoratiusAssetKnownTypes.Prefab;
                }
                else
                {
                    assetType = HoratiusAssetKnownTypes.Model;
                }
            }
            else if (type == typeof(MonoScript))
            {
                assetType = HoratiusAssetKnownTypes.MonoScript;

            }
            else if (type == typeof(Material))
            {
                assetType = HoratiusAssetKnownTypes.Material;

            }
            else if (type == typeof(SceneAsset))
            {
                assetType = HoratiusAssetKnownTypes.SceneAsset;

            }
            else if (type == typeof(Shader))
            {
                assetType = HoratiusAssetKnownTypes.Shader;

            }
            else if (IsSameOrSubclass(typeof(Texture), type))
            {
                // maybe split textures into multiple types, such as cubemap, texture2d, sprite, etc...
                assetType = HoratiusAssetKnownTypes.Texture;

            }
            else if (type == typeof(AnimationClip))
            {
                assetType = HoratiusAssetKnownTypes.AnimationClip;

            }
            else if (type == typeof(AnimatorController))
            {
                assetType = HoratiusAssetKnownTypes.AnimatorController;

            }
            else if (type == typeof(TextAsset))
            {
                assetType = HoratiusAssetKnownTypes.TextAsset;

            }
            else
            {
                assetType = HoratiusAssetKnownTypes.Unknown;
            }

        }
    }

}
