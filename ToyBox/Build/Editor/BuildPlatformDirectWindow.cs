using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEditorInternal.VR;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

using HhhNetwork.Build.Editor;

public class BuildPlatformDirectWindow : EditorWindow
{
    private Vector2 _scroll;

    private BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;
    private bool _buildAndRun = true;
    private bool _devBuild = false;
    private string _buildFolder = string.Empty;

    [MenuItem("Build/Build To Platform")]
    public static void OpenBuildWindow()
    {
        // try to dock next to Game window as I like it -/H
        EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        var gameWindow = windows.FirstOrDefault(e => e.titleContent.text.Contains("Game"));

        BuildPlatformDirectWindow bsw;
        if (gameWindow != null)
        {
            bsw = GetWindow<BuildPlatformDirectWindow>("Build To Platform", true, gameWindow.GetType());
        }
        else
        {
            bsw = GetWindow<BuildPlatformDirectWindow>("Build To Platform", true);
        }

        bsw.Show();
    }

    private void OnEnable()
    {
        this.minSize = new Vector2(200, 17);
    }

    private void OnGUI()
    {
        _scroll = GUILayout.BeginScrollView(_scroll);

        // Build and run GUI for each mode
        OnGUI_RenderBuildAndRunning();

        EditorGUILayout.Separator();

        GUILayout.EndScrollView();
    }

    private void OnGUI_RenderBuildAndRunning()
    {
        EditorGUILayout.LabelField(new GUIContent("Building & Running", "Building and optionally auto-running builds."), EditorStyles.centeredGreyMiniLabel);

        EditorGUI.BeginChangeCheck();

        _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(new GUIContent("Build Target", "Which platform to build for."), _buildTarget);

        if (EditorGUI.EndChangeCheck())
        {
            //BuildScriptPrefs.SetBuildTarget(_buildTarget);
        }

        EditorGUI.BeginChangeCheck();

        _devBuild = EditorGUILayout.Toggle(new GUIContent("Development Build", "Toggle to build as development build."), _devBuild);

        if (EditorGUI.EndChangeCheck())
        {
            //BuildScriptPrefs.SetDevelopmentBuild(_devBuild);
        }

        EditorGUI.BeginChangeCheck();
        _buildAndRun = EditorGUILayout.Toggle(new GUIContent("Build and Run?", "Whether to auto-run the game after building, if successful."), _buildAndRun);

        if (EditorGUI.EndChangeCheck())
        {
            //BuildScriptPrefs.SetBuildAndRun(_buildAndRun);
        }


        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Build"))
        {
            BuildButton_Singleplayer();
        }

        if (GUILayout.Button("Run"))
        {
            RunAsSingleplayer();
        }

        EditorGUILayout.EndHorizontal();


    }

    private void BuildButton_Singleplayer()
    {
        if (!Directory.Exists(_buildFolder))
        {
            SetBuildFolder();
        }

        if (_buildAndRun)
        {
            BuildScripts.BuildAndRunSingleplayer64("", _buildFolder, _devBuild, _buildTarget);
        }
        else
        {
            BuildScripts.BuildSingleplayer64("", _buildFolder, _devBuild, _buildTarget);
        }

        GUIUtility.ExitGUI();
    }

    private void RunAsSingleplayer()
    {
        BuildScripts.PlayAsSingleplayer("");
        GUIUtility.ExitGUI();
    }

    private string SetBuildFolder()
    {
        var fullPath = EditorUtility.OpenFolderPanel("Build Folder", Application.dataPath.Replace("Assets", string.Empty), "bin");
        if (!string.IsNullOrEmpty(fullPath))
        {
            _buildFolder = fullPath;
            BuildScriptPrefs.SetBuildFolder(_buildFolder);
            base.Repaint();
        }

        return fullPath;
    }

}