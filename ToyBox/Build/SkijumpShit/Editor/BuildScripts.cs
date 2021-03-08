namespace HhhNetwork.Build.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal.VR;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.XR;

    public static partial class BuildScripts
    {
        public static string gameName => PlayerSettings.productName;

        public static string singlePlayerBuildName => gameName + "Singleplayer";

        public static string clientBuildName => gameName + "Client";

        public static string serverBuildName => gameName + "Server";

        public static string matchmakerBuildName => gameName + "Matchmaker";

        public static string completeBuildName => gameName + "Complete";

        private const int _defaultPort = 8080;

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

        /* SERVER */

        public static void BuildServer64(string serverOnlyScene, string buildFolder, bool devBuild, BuildTarget target, int socketPort)
        {
            BuildServer(serverOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer, devBuild, socketPort);
        }

        public static void BuildAndRunServer64(string serverOnlyScene, string buildFolder, bool devBuild, BuildTarget target, int socketPort)
        {
            BuildServer(serverOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer | BuildOptions.AutoRunPlayer, devBuild, socketPort);
        }

        public static void PlayAsServer(string serverOnlyScene, int socketPort)
        {
            SetServerNetSenderSocketAddress(serverOnlyScene, socketPort);
            Run(serverOnlyScene);
        }

        /* MATCHMAKER */

        public static void BuildMatchmaker64(string matchmakerScene, string buildFolder, bool devBuild, BuildTarget target)
        {
            BuildMatchmaker(matchmakerScene, buildFolder, target, BuildOptions.ShowBuiltPlayer, devBuild);
        }

        public static void BuildAndRunMatchmaker64(string matchmakerScene, string buildFolder, bool devBuild, BuildTarget target)
        {
            BuildMatchmaker(matchmakerScene, buildFolder, target, BuildOptions.ShowBuiltPlayer | BuildOptions.AutoRunPlayer, devBuild);
        }

        public static void PlayAsMatchmaker(string matchmakerScene)
        {
            // should just load matchmaker scene and run it...
            Run(matchmakerScene);
        }

        /* CLIENT */

        public static void BuildAndRunClientForIP(string ip, int port, string clientOnlyScene, string buildFolder, bool devBuild, BuildTarget target, bool autoConnect)
        {
            BuildClient(clientOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer | BuildOptions.AutoRunPlayer, ip, port, devBuild, autoConnect);
        }

        public static void BuildClientForIP(string ip, int port, string clientOnlyScene, string buildFolder, bool devBuild, BuildTarget target, bool autoConnect)
        {
            BuildClient(clientOnlyScene, buildFolder, target, BuildOptions.ShowBuiltPlayer, ip, port, devBuild, autoConnect);
        }

        public static void PlayAsClientForIP(string ip, int port, string clientOnlyScene, bool autoConnect)
        {
            RunClient(clientOnlyScene, ip, port, autoConnect);
        }

        /* Complete */

        public static void PlayAsComplete(string loadScene)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // load loader scene only.
                var loadSceneE = EditorSceneManager.GetSceneByName(loadScene);
                if (loadSceneE.IsValid() && loadSceneE.isLoaded)
                {
                    Debug.Log("Scene is already loaded == " + loadScene);
                }
                else
                {
                    EditorSceneManager.OpenScene(loadScene, OpenSceneMode.Single);
                }

                //SetLevelLoaderSceneName(loadScene, string.Empty, true);
                EditorApplication.isPlaying = true;
            }
        }

        public static void BuildComplete(bool alsoRun, bool devBuild, BuildTarget target, string buildFolder, string loadScenePath, string[] additiveScenes)
        {
            // Add all of the additive scenes if they aren't added already
            var temp = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Make sure that the load scene is the first scene
            var loadScene = temp.FirstOrDefault(bb => bb.path.Equals(loadScenePath, StringComparison.OrdinalIgnoreCase));
            loadScene.enabled = true;
            if (loadScene == null)
            {
                loadScene = new EditorBuildSettingsScene(loadScenePath, true);
                temp.Insert(0, loadScene);
            }
            else
            {
                var idx = temp.IndexOf(loadScene);
                // swap so the load scene is first in list.
                if (idx > 0)
                {
                    var tmp = temp[0];
                    temp[idx] = tmp;
                    temp[0] = loadScene;
                }
            }

            foreach (var add in additiveScenes)
            {
                var contained = false;
                foreach (var t in temp)
                {
                    if (t.path.Equals(add, StringComparison.OrdinalIgnoreCase))
                    {
                        contained = true;
                        break;
                    }
                }

                if (!contained)
                {
                    temp.Add(new EditorBuildSettingsScene(add, true));
                }
            }

            EditorBuildSettings.scenes = temp.ToArray();

            // Additive level loader should not additively load anything
            foreach (var scene in EditorBuildSettings.scenes)
            {
                SetLevelLoaderSceneName(scene.path, string.Empty, true);

                if (scene.path.ToLower().Contains("client"))
                {
                    SetAutoConnectOnEnable(scene.path, false);
                }
            }

            var options = BuildOptions.ShowBuiltPlayer;
            if (devBuild)
            {
                options |= BuildOptions.Development;
            }
            else if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal)
            {
                // Linux builds always headless, hardcoded - but only if it is not development build, otherwise Unity crashes (Development cannot work with Headless)
                options |= BuildOptions.EnableHeadlessMode;
            }

            if (alsoRun)
            {
                options |= BuildOptions.AutoRunPlayer;
            }

            var pathFolder = completeBuildName;
            var path = GetPath(pathFolder, buildFolder, target);
            BuildPipeline.BuildPlayer(new BuildPlayerOptions()
            {
                scenes = SelectScenes(),
                locationPathName = path,
                target = target,
                options = options
            });

            Debug.Log("BUILD:: " + pathFolder + " for " + target.ToString() + " " + (devBuild ? "(Development Build)" : string.Empty) + ", built at == " + path);
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

        private static void BuildMatchmaker(string matchmakerScene, string buildFolder, BuildTarget target, BuildOptions options, bool devBuild)
        {
            // only add matchmaker scene, and restore afterwards...?
            var backupScenes = EditorBuildSettings.scenes;
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
            Build(matchmakerScene, matchmakerBuildName, buildFolder, target, options, ForceVRSupportedModes.ForcedFalse, devBuild);
            EditorBuildSettings.scenes = backupScenes;
        }

        private static void BuildServer(string serverOnlyScene, string buildFolder, BuildTarget target, BuildOptions options, bool devBuild, int socketPort)
        {
            // backup settings. order might matter.
            var backupVrEnabledStatus = XRSettings.enabled;
            var backupVrSupportedStatus = PlayerSettings.virtualRealitySupported;
            var backupVrMode = XRSettings.supportedDevices;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            Build(serverOnlyScene, serverBuildName, buildFolder, target, options, ForceVRSupportedModes.ForcedFalse, devBuild, socketPort);
            // vr settings back to previous
            PlayerSettings.virtualRealitySupported = backupVrSupportedStatus;
            VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, backupVrMode);
            XRSettings.enabled = backupVrEnabledStatus;
        }

        private static void BuildClient(string clientOnlyScene, string buildFolder, BuildTarget target, BuildOptions options, string ip, int port, bool devBuild, bool autoConnect)
        {
            SetAutoConnectOnEnable(clientOnlyScene, autoConnect);
            Build(clientOnlyScene, clientBuildName, buildFolder, target, options, ForceVRSupportedModes.NotForced, devBuild, -1, ip, port);
        }

        private static void RunClient(string extraScenePath, string ip, int port, bool autoConnect)
        {
            AddExtraScene(extraScenePath);

            OnRun(extraScenePath);

            SetAutoConnectOnEnable(extraScenePath, autoConnect);
            SetClientNetSenderServerAddress(extraScenePath, ip, port);

            var sceneCount = EditorSceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                SetLevelLoaderSceneName(scenePath, extraScenePath);
            }

            // also do it for scenes not in the build settings! ;)
            SetLevelLoaderSceneName(EditorSceneManager.GetActiveScene().path, extraScenePath);

            EditorApplication.isPlaying = true;

            //Debug.LogWarning("BUILD:: Be aware that currently the scene: " + extraScenePath + " - exists in Editor Build Settings as well, which is not a problem, as long as it is not the 0th index scene.");
        }

        private static void Run(string extraScenePath)
        {
            AddExtraScene(extraScenePath);

            OnRun(extraScenePath);

            var sceneCount = EditorSceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                SetLevelLoaderSceneName(scenePath, extraScenePath);
            }

            // also do it for scenes not in the build settings! ;)
            SetLevelLoaderSceneName(EditorSceneManager.GetActiveScene().path, extraScenePath);

            EditorApplication.isPlaying = true;

            //Debug.LogWarning("BUILD:: Be aware that currently the scene: " + extraScenePath + " - exists in Editor Build Settings as well, which is not a problem, as long as it is not the 0th index scene.");
        }

        static partial void OnRun(string extraScenePath);

        private static void Build(string extraScenePath, string pathFolder, string buildFolder, BuildTarget target, BuildOptions options, ForceVRSupportedModes forceVRSupported, bool devBuild, int socketPort = -1, string ip = "", int port = -1)
        {
            AddExtraScene(extraScenePath);

            OnBuild(extraScenePath);

            var sceneCount = EditorSceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                SetLevelLoaderSceneName(scenePath, extraScenePath);
            }

            if (forceVRSupported != ForceVRSupportedModes.NotForced)
            {
                // force true or false based on mode
                PlayerSettings.virtualRealitySupported = forceVRSupported == ForceVRSupportedModes.ForcedTrue;
                Debug.Log("BUILD:: Virtual reality supported == " + PlayerSettings.virtualRealitySupported.ToString());
            }

            if (!string.IsNullOrEmpty(ip) && port != -1)
            {
                SetClientNetSenderServerAddress(extraScenePath, ip, port);
            }

            if (socketPort >= 0)
            {
                SetServerNetSenderSocketAddress(extraScenePath, socketPort);
            }

            if (devBuild)
            {
                options |= BuildOptions.Development;
            }
            else if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal)
            {
                // Linux builds always headless, hardcoded - but only if it is not development build, otherwise Unity crashes (Development cannot work with Headless)
                options |= BuildOptions.EnableHeadlessMode;
            }

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

        static partial void OnBuild(string extraScenePath);

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

            case BuildTarget.StandaloneLinux:
            {
                return "x86";
            }

            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
            {
                return "x86_64";
            }
            case BuildTarget.Android:
            {
                return "apk";
            }
            }

            return string.Empty;
        }

        private static void SetAutoConnectOnEnable(string scenePath, bool autoConnect)
        {
            //var modified = ModifyRootGameObject<Client.ClientNetSender>(scenePath, (clientNetSender) =>
            //{
            //    if (clientNetSender.autoConnectOnEnable != autoConnect)
            //    {
            //        clientNetSender.autoConnectOnEnable = autoConnect;
            //        return true;
            //    }

            //    return false;
            //});

            //if (!modified)
            //{
            //    Debug.LogError("BUILD:: Could not identify a ClientNetSender in any of the scenes in the Editor Build Settings currently. Thus, cannot modify AutoConnect on enable setting.");
            //}
        }

        private static void SetServerNetSenderSocketAddress(string scenePath, int socketPort)
        {
            //var modified = ModifyRootGameObject<Server.ServerNetSender>(scenePath, (serverNetSender) =>
            //{
            //    if (serverNetSender.socketPort != socketPort)
            //    {
            //        serverNetSender.socketPort = socketPort;
            //        Debug.Log("BUILD:: Server server socket port on ServerNetSender (" + serverNetSender.ToString() + ") == " + serverNetSender.socketPort.ToString());
            //        return true;
            //    }

            //    return false;
            //});

            //if (!modified)
            //{
            //    Debug.LogError("BUILD:: Could not identify a ServerNetSender in any of the scenes in the Editor Build Settings currently. Thus, server may not work! Please ensure the additively loaded 'Server Only' scene is setup and is valid.");
            //}
        }

        private static void SetClientNetSenderServerAddress(string scenePath, string serverIp, int serverPort)
        {
            //var modified = ModifyRootGameObject<Client.ClientNetSender>(scenePath, (clientNetSender) =>
            //{
            //    if (!string.Equals(clientNetSender.serverIp, serverIp) || clientNetSender.serverPort != serverPort)
            //    {
            //        clientNetSender.serverIp = serverIp;
            //        clientNetSender.serverPort = serverPort;
            //        Debug.Log("BUILD:: Set server IP and port on ClientNetSender (" + clientNetSender.ToString() + ") == " + clientNetSender.serverIp + " : " + clientNetSender.serverPort.ToString());
            //        return true;
            //    }

            //    return false;
            //});

            //if (!modified)
            //{
            //    Debug.LogError("BUILD:: Could not identify a ClientNetSender in any of the scenes in the Editor Build Settings currently. Thus, client may not work! Please ensure that the additively loaded 'Client Only' scene is setup and is valid.");
            //}
        }

        private static void SetLevelLoaderSceneName(string scenePath, string extraScenePath, bool noCreate = false)
        {
            //var sceneName = Path.GetFileNameWithoutExtension(extraScenePath);
            //var foundAdditiveLoader = ModifyRootGameObject<AdditiveLevelLoader>(scenePath, (levelLoader) =>
            //{
            //    if (levelLoader.doNotRunInThisScene)
            //    {
            //        return false;
            //    }

            //    if (!string.Equals(levelLoader.sceneName, sceneName))
            //    {
            //        levelLoader.sceneName = sceneName;
            //        Debug.Log(DebugUtils.messageColor + "BUILD:: AdditiveLevelLoader on Scene " + scenePath + ": set from " + levelLoader.sceneName + " =to= " + sceneName + DebugUtils.endColor);
            //        //Debug.Log("BUILD:: Set scene name on additive level loader (" + levelLoader.ToString() + ") == " + levelLoader.sceneName);
            //        return true;
            //    }

            //    return false;
            //});

            //if (!noCreate && !foundAdditiveLoader)
            //{
            //    // create the object in the correct scene... not just anywhere
            //    // stolen from ModifyRootGameObject<T> function
            //    var openedScene = false;
            //    var scene = EditorSceneManager.GetSceneByPath(scenePath);
            //    if (!scene.IsValid() || !scene.isLoaded)
            //    {
            //        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            //        openedScene = true;
            //    }

            //    var go = new GameObject("AdditiveLevelLoader");
            //    Undo.RegisterCreatedObjectUndo(go, "Created additive level loader");
            //    // move new GO in the correct scene
            //    var goFromTargetScene = scene.GetRootGameObjects().FirstOrDefault();
            //    if (goFromTargetScene != null)
            //    {
            //        Undo.SetTransformParent(go.transform, goFromTargetScene.transform, "Parent Additive Loader to new scene");
            //        Undo.SetTransformParent(go.transform, null, "Parent Additive Loader to root of new scene");
            //    }

            //    var levelLoader = Undo.AddComponent<AdditiveLevelLoader>(go);

            //    Undo.RecordObject(levelLoader, "Set additive level loader scene name");
            //    levelLoader.sceneName = sceneName;

            //    Undo.FlushUndoRecordObjects();
            //    EditorSceneManager.SaveOpenScenes();
            //    Debug.Log("BUILD:: Create an additive level loader & set scene name on it (" + levelLoader.ToString() + ") == " + levelLoader.sceneName);

            //    if (openedScene)
            //    {
            //        EditorSceneManager.CloseScene(scene, true);
            //    }
            //}
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