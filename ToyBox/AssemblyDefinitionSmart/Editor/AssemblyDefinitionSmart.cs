namespace ToyBoxHHH.AssemblyDefinitionSmart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using SimpleJSON;

    /// <summary>
    /// Use this to automatically create Assembly Definition files in your project. Makes separate assemblies in Editor/ folders, and references upwards.
    /// 
    /// Note: it is not THAT smart that it could identify cross-referencing assemblies. For that you must identify and reference the dependencies by hand.
    /// However, this does save some time when you have a complex structure with a lot of editor folders inside other folders.
    /// 
    /// made by @horatiu665
    /// </summary>
    public static class AssemblyDefinitionSmart
    {
        private const string TAG = "[AssemblyDefinitionSmart] ";

        [MenuItem("Assets/Create/Assembly Definition (Smart)", false, 99)]
        public static void CreateAssemblyDefinition()
        {
            var path = "";
            var activeObject = Selection.activeObject;
            if (activeObject == null)
            {
                Debug.Log(TAG + "Error: selection is null");
                return;
            }
            else
            {
                path = AssetDatabase.GetAssetPath(activeObject.GetInstanceID());
            }

            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    // is folder
                    CreateADInFolder(path);
                }
                else
                {
                    // is not a folder
                    Debug.Log(TAG + "Error: selection is not a folder");
                }
            }
            else
            {
                Debug.Log(TAG + "Error: selection not in assets folder");
            }

        }

        public static List<FileInfo> GetAllAsmdefFilesInProject()
        {
            var list = new List<FileInfo>();
            var allPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allPaths.Length; i++)
            {
                if (allPaths[i].Substring(0, 6) == "Assets")
                {
                    var fi = new FileInfo(allPaths[i]);
                    if (fi.Extension == ".asmdef")
                    {
                        list.Add(fi);
                    }
                }
            }
            return list;
        }

        private static void CreateADInFolder(string path)
        {
            // GRAND STRATEGY
            //      first create an assembly definition in the folder
            //      then navigate the children folders, find potential EDITOR folders, and PLUGINS folders, and PLUGINS/EDITOR folders, and create ASMDEF for each of those, with dependency to the main one created above

            var allAsmdefs = GetAllAsmdefFilesInProject();

            // find the root asmdef file name (default)
            string mainAsmFileName = GetAsmdefName(path, allAsmdefs);

            // if an asmdef exists in this folder, do not make it again.
            bool foundAsmdefInRoot = false;
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();
            if (files.Any(f => f.Extension == ".asmdef"))
            {
                var fileWithAsmdefExtension = files.First(f => f.Extension == ".asmdef");
                // update the file name, to use as reference for all new children created asmdefs.
                mainAsmFileName = fileWithAsmdefExtension.Name.Substring(0, fileWithAsmdefExtension.Name.Length - ".asmdef".Length);

                // remove the first bit of the path = the path to the game folder.
                var asmdefPathFromGameFolder = "Assets/" + fileWithAsmdefExtension.FullName.Substring(Application.dataPath.Length);
                var contextObj = AssetDatabase.LoadAssetAtPath(asmdefPathFromGameFolder, typeof(UnityEngine.Object));
                Debug.Log(TAG + " Note: AssemblyDefinition " + fileWithAsmdefExtension.Name + " already exists for " + path, contextObj);

                // used to return but now it should just try to make asmdefs for the subfolders 
                //// why doesn't this work? no idea.
                //EditorGUIUtility.PingObject(contextObj);
                //return;
                foundAsmdefInRoot = true;
            }

            if (!foundAsmdefInRoot)
            {
                // create json structure
                var mainAsmFile = new JSONClass();
                // add name
                mainAsmFile.Add("name", new JSONData(mainAsmFileName));
                // add build target thingies (default to all)
                // add dependencies to other assembly files?...

                var finalPath = GetAsmdefFinalPath(path, mainAsmFileName);
                SaveContentsToFile(mainAsmFile.ToString(), finalPath);

                allAsmdefs.Add(new FileInfo(finalPath));
            }

            // navigate children folders, do this for all editor folders, except when an ASMDEF file was found in the folder - then do not continue checking
            CreateEditorAssembliesForChildFoldersRecursive(path, mainAsmFileName, allAsmdefs);

            // refresh database in unity
            AssetDatabase.Refresh();
        }

        private static void CreateEditorAssembliesForChildFoldersRecursive(string path, string mainAsmFileName, List<FileInfo> allAsmdefs)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dir);

                // if this folder already contains an asmdef, do not touch
                var files = dirInfo.GetFiles();
                if (files.Any(f => f.Extension == ".asmdef"))
                {
                    continue;
                }

                // if the folder is an editor folder
                if (dirInfo.Name == "Editor")
                {
                    // generate an editor asmdef and make it ref the main asmdef
                    var editorAsmFile = new JSONClass();

                    // name
                    var editorAsmFileName = GetAsmdefName(dir, allAsmdefs);
                    editorAsmFile.Add("name", new JSONData(editorAsmFileName));

                    // ref to the main one
                    var refs = new JSONArray();
                    refs.Add(new JSONData(mainAsmFileName));
                    editorAsmFile.Add("references", refs);

                    // only editor
                    editorAsmFile.Add("includePlatforms", new JSONArray() { new JSONData("Editor") });

                    // empty array
                    editorAsmFile.Add("excludePlatforms", new JSONArray());

                    var finalPath = GetAsmdefFinalPath(dir, editorAsmFileName);
                    SaveContentsToFile(editorAsmFile.ToString(), finalPath);

                    allAsmdefs.Add(new FileInfo(finalPath));

                }
                else // if this folder is not an editor
                {
                    // create asmdefs for the children ;)
                    CreateEditorAssembliesForChildFoldersRecursive(dir, mainAsmFileName, allAsmdefs);
                }

            }
        }

        private static string GetAsmdefFinalPath(string path, string asmName)
        {
            return Path.Combine(path, asmName + ".asmdef");
        }

        private static string GetAsmdefName(string dirPath, List<FileInfo> allAsmdefs)
        {
            DirectoryInfo dirInfo;
            string aName = "";
            if (Directory.Exists(dirPath))
            {
                dirInfo = new DirectoryInfo(dirPath);
                //aName = dirInfo.Name + "Asm";
            }
            else
            {
                //aName = new FileInfo(dirPath).DirectoryName + "Asm";
                dirInfo = new FileInfo(dirPath).Directory;
            }

            // prefix name with all the dirs since Assets/ (excluding Assets/ and the first /)
            var allFoldersSinceAssets = dirInfo.FullName.Substring(Application.dataPath.Length);
            var folders = allFoldersSinceAssets.Split('/', '\\');
            aName = "";
            for (int i = 0; i < folders.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(folders[i]))
                {
                    aName += folders[i] + ".";
                }
            }
            aName += folders[folders.Length - 1];

            // this will probably never be necessary now, since there cannot be duplicates when we add all folder names...
            var suffix = "";
            int suffixCounter = 0;
            // find unique name for Asm file.
            while (allAsmdefs.Any(f => f.Name == (aName + suffix) + ".asmdef"))
            {
                suffixCounter++;
                suffix = suffixCounter.ToString();
            }

            return aName + suffix;
        }

        private static void SaveContentsToFile(string asmFileContents, string finalPath)
        {
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(finalPath)).Directory.FullName);
            StreamWriter sw = new StreamWriter(finalPath, false);
            sw.Write(asmFileContents);
            sw.Close();
        }
    }
}