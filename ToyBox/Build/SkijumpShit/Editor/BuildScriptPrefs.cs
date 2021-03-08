namespace HhhNetwork.Build.Editor
{
    using UnityEditor;

    public static partial class BuildScriptPrefs
    {
        private readonly static string _projectKey = PlayerSettings.productName;

        private readonly static string _keyBuildFolder = _projectKey + "_BuildFolder";

        private readonly static string _keyServerIp = _projectKey + "_ServerIP";
        private readonly static string _keyServerPort = _projectKey + "_ServerPort";

        private readonly static string _keyMatchmakerScene = _projectKey + "_MatchmakerScene";
        private readonly static string _keyServerOnlyScene = _projectKey + "_ServerOnlyScene";
        private readonly static string _keySingleOnlyScene = _projectKey + "_SingleOnlyScene";
        private readonly static string _keyClientOnlyScene = _projectKey + "_ClientOnlyScene";
        private readonly static string _keyLoadScene = _projectKey + "_LoadScene";
        private readonly static string _keyLevelSelectScene = _projectKey + "_LevelSelect";

        private readonly static string _keyCurrentMode = _projectKey + "_CurrentMode";

        private readonly static string _keyDevBuild = _projectKey + "_DevelopmentBuild";
        private readonly static string _keyBuildAndRun = _projectKey + "_BuildAndRun";
        private readonly static string _keyUseLocal = _projectKey + "_UseLocal";
        private readonly static string _keyBuildTarget = _projectKey + "_BuildTarget";
        private readonly static string _keyUseHetzner = _projectKey + "_UseHetzner";

        private readonly static string _keySocketPort = _projectKey + "_SocketPort";

        private readonly static string _keyAutoConnectOnEnable = _projectKey + "_AutoConnectOnEnable";

        public static void SetBuildFolder(string buildFolder)
        {
            EditorPrefs.SetString(_keyBuildFolder, buildFolder);
        }

        public static string GetBuildFolder()
        {
            return EditorPrefs.GetString(_keyBuildFolder, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "VRUnicorns/bin"));
        }

        public static void SetServerIP(string serverIp)
        {
            EditorPrefs.SetString(_keyServerIp, serverIp);
        }

        public static string GetServerIP()
        {
            return EditorPrefs.GetString(_keyServerIp, "192.168.1.1");
        }

        public static void SetServerPort(int serverPort)
        {
            EditorPrefs.SetInt(_keyServerPort, serverPort);
        }

        public static int GetServerPort()
        {
            return EditorPrefs.GetInt(_keyServerPort, 8080);
        }

        public static void SetMatchmakerScene(string matchmakerScene)
        {
            EditorPrefs.SetString(_keyMatchmakerScene, matchmakerScene);
        }

        public static string GetMatchmakerScene()
        {
            return EditorPrefs.GetString(_keyMatchmakerScene, "Assets/Hhh/HhhMatchmaking/Scenes/MatchmakingServer.unity");
        }

        public static void SetServerOnlyScene(string serverOnlyScene)
        {
            EditorPrefs.SetString(_keyServerOnlyScene, serverOnlyScene);
        }

        public static string GetServerOnlyScene()
        {
            return EditorPrefs.GetString(_keyServerOnlyScene, "Assets/Scenes/Templates/Server Only.unity");
        }

        public static void SetSingleOnlyScene(string singleOnlyScene)
        {
            EditorPrefs.SetString(_keySingleOnlyScene, singleOnlyScene);
        }

        public static string GetSingleOnlyScene()
        {
            return EditorPrefs.GetString(_keySingleOnlyScene, "Assets/Scenes/Templates/Singleplayer Only.unity");
        }

        public static void SetClientOnlyScene(string clientOnlyScene)
        {
            EditorPrefs.SetString(_keyClientOnlyScene, clientOnlyScene);
        }

        public static string GetClientOnlyScene()
        {
            return EditorPrefs.GetString(_keyClientOnlyScene, "Assets/Scenes/Templates/Client Only.unity");
        }

        public static void SetLoadScene(string loadScene)
        {
            EditorPrefs.SetString(_keyLoadScene, loadScene);
        }

        public static string GetLoadScene()
        {
            return EditorPrefs.GetString(_keyLoadScene, "Assets/Scenes/Load Scene.unity");
        }

        public static void SetLevelSelectScene(string selectScene)
        {
            EditorPrefs.SetString(_keyLevelSelectScene, selectScene);
        }

        public static string GetLevelSelectScene()
        {
            return EditorPrefs.GetString(_keyLevelSelectScene, "Assets/Scenes/Scene Selection.unity");
        }

        public static void SetBuildAndRun(bool buildAndRun)
        {
            EditorPrefs.SetBool(_keyBuildAndRun, buildAndRun);
        }

        public static bool GetBuildAndRun()
        {
            return EditorPrefs.GetBool(_keyBuildAndRun, true);
        }

        public static void SetUseLocal(bool useLocal)
        {
            EditorPrefs.SetBool(_keyUseLocal, useLocal);
        }

        public static bool GetUseLocal()
        {
            return EditorPrefs.GetBool(_keyUseLocal, true);
        }

        public static void SetUseHetzner(bool useHetzner)
        {
            EditorPrefs.SetBool(_keyUseHetzner, useHetzner);
        }

        public static bool GetUseHetzner()
        {
            return EditorPrefs.GetBool(_keyUseHetzner, false);
        }

        public static void SetDevelopmentBuild(bool devBuild)
        {
            EditorPrefs.SetBool(_keyDevBuild, devBuild);
        }

        public static bool GetDevelopmentBuild()
        {
            return EditorPrefs.GetBool(_keyDevBuild, false);
        }

        public static void SetBuildTarget(BuildTarget target)
        {
            EditorPrefs.SetInt(_keyBuildTarget, (int)target);
        }

        public static BuildTarget GetBuildTarget()
        {
            return (BuildTarget)EditorPrefs.GetInt(_keyBuildTarget, (int)BuildTarget.StandaloneWindows64);
        }

        public static void SetSocketPort(int port)
        {
            EditorPrefs.SetInt(_keySocketPort, port);
        }

        public static int GetSocketPort()
        {
            return EditorPrefs.GetInt(_keySocketPort, 8080);
        }

        public static bool GetAutoConnectOnEnable()
        {
            return EditorPrefs.GetBool(_keyAutoConnectOnEnable, false);
        }

        public static void SetAutoConnectOnEnable(bool autoConnect)
        {
            EditorPrefs.SetBool(_keyAutoConnectOnEnable, autoConnect);
        }
    }
}