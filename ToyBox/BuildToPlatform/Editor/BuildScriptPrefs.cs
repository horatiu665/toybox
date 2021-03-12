namespace ToyBoxHHH.Build.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A window that can help build a project to a new platform, bypassing the extra click for "Switch Platforms".
    /// It can be also used as a reference for how to do build scripts, and if you clone it you can extend with your own pre-build operations,
    ///     like disabling some game objects, generating stuff before build, enabling scenes, or whatever.
    ///     
    /// made by @horatiu665 and Rami Ahmed Bock cca 2018
    /// </summary>
    public static partial class BuildScriptPrefs
    {
        private readonly static string _projectKey = PlayerSettings.productName;

        private readonly static string _keyBuildFolder = _projectKey + "_BuildFolder";

        private readonly static string _keyDevBuild = _projectKey + "_DevelopmentBuild";
        private readonly static string _keyBuildAndRun = _projectKey + "_BuildAndRun";
        private readonly static string _keyBuildTarget = _projectKey + "_BuildTarget";

        public static void SetBuildFolder(string buildFolder)
        {
            EditorPrefs.SetString(_keyBuildFolder, buildFolder);
        }
        public static string GetBuildFolder()
        {
            var defaultBuildFolder = System.IO.Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets/".Length), "bin");
            return EditorPrefs.GetString(_keyBuildFolder, defaultBuildFolder);
        }

        public static void SetBuildAndRun(bool buildAndRun)
        {
            EditorPrefs.SetBool(_keyBuildAndRun, buildAndRun);
        }
        public static bool GetBuildAndRun()
        {
            return EditorPrefs.GetBool(_keyBuildAndRun, true);
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

    }
}