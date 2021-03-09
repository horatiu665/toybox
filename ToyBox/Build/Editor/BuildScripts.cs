namespace ToyBox.Build.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public static partial class BuildScripts
    {
        public static string gameName => PlayerSettings.productName;

        public static string singlePlayerBuildName => gameName;

        /* SINGLEPLAYER */

        public static void BuildSingleplayer64(string singleOnlyScene, string buildFolder, bool devBuild, BuildTarget target)
        {
            BuildSingleplayer(singleOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer, devBuild);
        }

        public static void BuildAndRunSingleplayer64(string singleOnlyScene, string buildFolder, bool devBuild, BuildTarget target)
        {
            BuildSingleplayer(singleOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer | BuildOptions.AutoRunPlayer, devBuild);
        }

        public static void PlayAsSingleplayer(string singleOnlyScene)
        {
            Run(singleOnlyScene);
        }

        /* HELPERS */

        private static string[] SelectScenes()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        }

        private static void BuildSingleplayer(string singleOnlyScene, string buildFolder, BuildTarget target, BuildOptions options, bool devBuild)
        {
            Build(singleOnlyScene, singlePlayerBuildName, buildFolder, target, options, ForceVRSupportedModes.NotForced, devBuild);
        }

        private static void Run(string extraScenePath)
        {
            AddExtraScene(extraScenePath);

            EditorApplication.isPlaying = true;

        }

        private static void Build(string extraScenePath, string pathFolder, string buildFolder, BuildTarget target, BuildOptions options, ForceVRSupportedModes forceVRSupported, bool devBuild)
        {
            AddExtraScene(extraScenePath);

            // loader scene setup, when that was a thing
            //var sceneCount = EditorSceneManager.sceneCountInBuildSettings;
            //for (int i = 0; i < sceneCount; i++)
            //{
            //    var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            //    SetLevelLoaderSceneName(scenePath, extraScenePath);
            //}

#pragma warning disable 0618
            if (forceVRSupported != ForceVRSupportedModes.NotForced)
            {
                // force true or false based on mode
                PlayerSettings.virtualRealitySupported = forceVRSupported == ForceVRSupportedModes.ForcedTrue;
                Debug.Log("BUILD:: Virtual reality supported == " + PlayerSettings.virtualRealitySupported.ToString());
            }
#pragma warning restore 0618

            if (devBuild)
            {
                options |= BuildOptions.Development;
            }
#pragma warning disable 0618
            else if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal)
            {
                // Linux builds always headless, hardcoded - but only if it is not development build, otherwise Unity crashes (Development cannot work with Headless)
                options |= BuildOptions.EnableHeadlessMode;
            }
#pragma warning restore 0618

            var path = GetPath(pathFolder, buildFolder, target);
            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = SelectScenes(),
                locationPathName = path,
                target = target,
                options = options
            });

            // don't remove scenes from build settings menu, it's easier to always have them in there.
            //RemoveScene(extraScenePath);
            Debug.Log("BUILD:: " + pathFolder + " for " + target.ToString() + " " + (devBuild ? "(Development Build)" : string.Empty) + ", built at == " + path);
        }

        private static void AddExtraScene(string extraScenePath)
        {
            if (extraScenePath == "")
            {
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (string.Equals(scenes[i].path, extraScenePath))
                {
                    // scene already exists in build settings
                    return;
                }
            }

            var extraScene = new EditorBuildSettingsScene()
            {
                path = extraScenePath,
                enabled = true
            };

            EditorBuildSettings.scenes = (EditorBuildSettingsScene[])scenes.Concat(new EditorBuildSettingsScene[] { extraScene });
            Debug.Log("BUILD:: Added new scene to editor build settings, scene path == " + extraScenePath);
        }

        private static void RemoveScene(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            var newScenes = new List<EditorBuildSettingsScene>(scenes.Length);
            for (int i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!string.Equals(scene.path, scenePath))
                {
                    // only add scenes that do not match the removed scene
                    newScenes.Add(scene);
                }
            }

            EditorBuildSettings.scenes = newScenes.ToArray();
        }

        private static string GetPath(string serverOrClient, string buildFolder, BuildTarget buildTarget)
        {
            // format:
            // buildFolder/platform/clientOrServer/build.exe
            var platform = buildTarget.ToString();
            var fileWithExtension = string.Concat(serverOrClient, ".", GetExtension(buildTarget));
            return Path.Combine(buildFolder, Path.Combine(platform, Path.Combine(serverOrClient, fileWithExtension)));
        }

        private static string GetExtension(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            {
                return "exe";
            }

#pragma warning disable 0618
                case BuildTarget.StandaloneLinux:
            {
                return "x86";
            }

            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
            {
                return "x86_64";
            }
#pragma warning restore 0618
            case BuildTarget.Android:
            {
                return "apk";
            }
            }

            return string.Empty;
        }


        /// <summary>
        /// Modification runs on the scene at scenePath. If modification is successful, returns true.
        /// Modification function returns true or false based on if we should save undo or not - simply an optimization, has nothing to do with the first true/false in this description.
        /// </summary>
        /// <typeparam name="T">component to run modifications on, in the target scene</typeparam>
        /// <param name="scenePath">target scene, where to search for objects to modify</param>
        /// <param name="modification_returnTrueIfWeShouldSaveUndo">function that executes the custom modifications. returns true if the scene should be marked dirty/saved.</param>
        /// <returns>true if the modification was executed, regardless if it changed anything</returns>
        private static bool ModifyRootGameObject<T>(string scenePath, Func<T, bool> modification_returnTrueIfWeShouldSaveUndo) where T : Component
        {
            var success = false;
            var openedScene = false;
            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedScene = true;
            }

            var root = scene.GetRootGameObjects();
            foreach (var go in root)
            {
                var component = go.GetComponent<T>();
                if (component != null)
                {
                    Undo.RecordObject(component, "Modified root game object as part of BuildScripts");
                    if (modification_returnTrueIfWeShouldSaveUndo(component))
                    {
                        Undo.FlushUndoRecordObjects();

                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }

                    success = true;
                    break;
                }
            }

            if (openedScene)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            return success;
        }

        private enum ForceVRSupportedModes
        {
            NotForced,
            ForcedTrue,
            ForcedFalse
        }
    }
}