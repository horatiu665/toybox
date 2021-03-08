// MAKE THIS TRUE to preview the script with all its glorious bugs
#if false

namespace HhhNetwork.Build.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal;
    using UnityEditorInternal.VR;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.XR;

    public sealed partial class BuildScriptWindow : EditorWindow
    {
        //private BuildMode _currentMode = BuildMode.Singleplayer;

        private string _matchmakerScene = string.Empty;
        private string _serverOnlyScene = string.Empty;
        private string _clientOnlyScene = string.Empty;
        private string _singleOnlyScene = string.Empty;
        private string _loadScene = string.Empty;
        private string _levelSelectScene = string.Empty;

        private BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;
        private string _buildFolder = string.Empty;
        private string _serverIp = string.Empty;
        private bool _buildAndRun = true;
        private bool _devBuild = false;
        private int _serverPort = -1;
        private bool _useLocal = true;
        private bool _useHetzner = false;
        private int _socketPort = -1;
        private bool _autoConnectOnEnable = false;

        private Vector2 _scroll;
        private ReorderableList _reorderableList;

#region SCENE ENABLED/DISABLED STATUSES PER GAME MODE
        public enum SceneEnabledState : byte
        {
            None,
            ForcedActive,
            ForcedInactive,
        }

        private const string SCENE_STATUSES_KEY = "scene_statuses_ASDFSFREFRD";
        [SerializeField] private List<string> sceneStatusesPathCache = new List<string>();
        [SerializeField] private SceneEnabledState[] sceneStatusesMatchmaker = new SceneEnabledState[0];
        [SerializeField] private SceneEnabledState[] sceneStatusesServer = new SceneEnabledState[0];
        [SerializeField] private SceneEnabledState[] sceneStatusesClient = new SceneEnabledState[0];
        [SerializeField] private SceneEnabledState[] sceneStatusesSingleplayer = new SceneEnabledState[0];
        [SerializeField] private SceneEnabledState[] sceneStatusesComplete = new SceneEnabledState[0];

        private void SaveSceneStatusesFromPrefs()
        {
            var s = "";
            for (int i = 0; i < sceneStatusesPathCache.Count; i++)
            {
                s += sceneStatusesPathCache[i] + ",";
            }
            s += ";";
            for (int i = 0; i < sceneStatusesMatchmaker.Length; i++)
            {
                s += (int)sceneStatusesMatchmaker[i] + ",";
            }
            s += ";";
            for (int i = 0; i < sceneStatusesServer.Length; i++)
            {
                s += (int)sceneStatusesServer[i] + ",";
            }
            s += ";";
            for (int i = 0; i < sceneStatusesClient.Length; i++)
            {
                s += (int)sceneStatusesClient[i] + ",";
            }
            s += ";";
            for (int i = 0; i < sceneStatusesSingleplayer.Length; i++)
            {
                s += (int)sceneStatusesSingleplayer[i] + ",";
            }
            s += ";";
            for (int i = 0; i < sceneStatusesComplete.Length; i++)
            {
                s += (int)sceneStatusesComplete[i] + ",";
            }
            EditorPrefs.SetString(SCENE_STATUSES_KEY, s);
        }

        private void LoadSceneStatusesFromPrefs()
        {
            var s = EditorPrefs.GetString(SCENE_STATUSES_KEY);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var parts = s.Split(';');
                if (parts.Length == 6)
                {
                    // scene status pathes
                    var paths = parts[0];
                    sceneStatusesPathCache.Clear();
                    foreach (var path in paths.Split(','))
                    {
                        sceneStatusesPathCache.Add(path);
                    }

                    sceneStatusesMatchmaker = new SceneEnabledState[sceneStatusesPathCache.Count];
                    sceneStatusesServer = new SceneEnabledState[sceneStatusesPathCache.Count];
                    sceneStatusesClient = new SceneEnabledState[sceneStatusesPathCache.Count];
                    sceneStatusesSingleplayer = new SceneEnabledState[sceneStatusesPathCache.Count];
                    sceneStatusesComplete = new SceneEnabledState[sceneStatusesPathCache.Count];

                    var i = 1;

                    sceneStatusesMatchmaker = parts[i++].Split(',').Select(sta => { int val; int.TryParse(sta, out val); return (SceneEnabledState)val; }).ToArray();
                    sceneStatusesServer = parts[i++].Split(',').Select(sta => { int val; int.TryParse(sta, out val); return (SceneEnabledState)val; }).ToArray();
                    sceneStatusesClient = parts[i++].Split(',').Select(sta => { int val; int.TryParse(sta, out val); return (SceneEnabledState)val; }).ToArray();
                    sceneStatusesSingleplayer = parts[i++].Split(',').Select(sta => { int val; int.TryParse(sta, out val); return (SceneEnabledState)val; }).ToArray();
                    sceneStatusesComplete = parts[i++].Split(',').Select(sta => { int val; int.TryParse(sta, out val); return (SceneEnabledState)val; }).ToArray();
                }
            }
        }

        private void ValidateSceneStatuses_BuildSceneCounts()
        {
            // load from player prefs...

            // if any scenes in the cache are not the same as the ones in the build settings...
            if (sceneStatusesPathCache == null || sceneStatusesPathCache.Count != EditorBuildSettings.scenes.Length
                || sceneStatusesComplete.Length == 0
                || sceneStatusesSingleplayer.Length != sceneStatusesComplete.Length
                || sceneStatusesClient.Length != sceneStatusesSingleplayer.Length
                || sceneStatusesServer.Length != sceneStatusesClient.Length
                || sceneStatusesMatchmaker.Length != sceneStatusesServer.Length
                )
            {
                // reset the cache list, but also reorder all scene enabled states, so the same enabled scenes are kept between reorderings no matter what other scenes have changed.
                // example: ServerOnly true should remain even if the scenes are reordered or added/removed...
                var newPathCache = EditorBuildSettings.scenes.Select(ebss => ebss.path).ToList();
                var newSceneStatusesMatchmaker = newPathCache.Select(ebss => SceneEnabledState.None).ToArray();
                var newSceneStatusesServer = newSceneStatusesMatchmaker.ToArray();
                var newSceneStatusesClient = newSceneStatusesMatchmaker.ToArray();
                var newSceneStatusesSingleplayer = newSceneStatusesMatchmaker.ToArray();
                var newSceneStatusesComplete = newSceneStatusesMatchmaker.ToArray();
                // set all new (reordered) ids to the old values at old indices.
                for (int i = 0; i < newPathCache.Count; i++)
                {
                    var indexOld = sceneStatusesPathCache.IndexOf(newPathCache[i]);
                    if (indexOld >= 0 && indexOld < sceneStatusesPathCache.Count)
                    {
                        newSceneStatusesMatchmaker[i] = sceneStatusesMatchmaker[indexOld];
                        newSceneStatusesServer[i] = sceneStatusesServer[indexOld];
                        newSceneStatusesClient[i] = sceneStatusesClient[indexOld];
                        newSceneStatusesSingleplayer[i] = sceneStatusesSingleplayer[indexOld];
                        newSceneStatusesComplete[i] = sceneStatusesComplete[indexOld];
                    }
                }
                sceneStatusesPathCache = newPathCache;
                sceneStatusesMatchmaker = newSceneStatusesMatchmaker;
                sceneStatusesServer = newSceneStatusesServer;
                sceneStatusesClient = newSceneStatusesClient;
                sceneStatusesSingleplayer = newSceneStatusesSingleplayer;
                sceneStatusesComplete = newSceneStatusesComplete;
            }

            // save to player prefs...

        }

        private void SetSceneStatuses()
        {
            SceneEnabledState[] statuses = sceneStatusesSingleplayer;
            switch (_currentMode)
            {
                case BuildMode.Singleplayer:
                    statuses = sceneStatusesSingleplayer;
                    break;
                case BuildMode.Client:
                    statuses = sceneStatusesClient;
                    break;
                case BuildMode.Server:
                    statuses = sceneStatusesServer;
                    break;
                case BuildMode.MatchmakingServer:
                    statuses = sceneStatusesMatchmaker;
                    break;
                case BuildMode.Complete:
                    statuses = sceneStatusesComplete;
                    break;
            }
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var pp = EditorBuildSettings.scenes[i].path;
                var index = sceneStatusesPathCache.IndexOf(pp);
                if (index >= 0 && index < statuses.Length)
                {
                    if (statuses[index] == SceneEnabledState.ForcedActive)
                    {
                        EditorBuildSettings.scenes[i].enabled = true;
                    }
                    else if (statuses[index] == SceneEnabledState.ForcedInactive)
                    {
                        EditorBuildSettings.scenes[i].enabled = false;
                    }
                }
            }

            var s = "Set scene enabled status from mode-specific setting. Final flags look like: ";
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                s += (EditorBuildSettings.scenes[i].enabled ? 1 : 0).ToString();
            }
            Debug.Log(s);

        }
#endregion


        [MenuItem("Build/Open Build Window")]
        public static void OpenBuildWindow()
        {
            // try to dock next to Game window as I like it -/H
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            var gameWindow = windows.FirstOrDefault(e => e.titleContent.text.Contains("Game"));

            BuildScriptWindow bsw;
            if (gameWindow != null)
            {
                bsw = GetWindow<BuildScriptWindow>("Build Window", true, gameWindow.GetType());
            }
            else
            {
                bsw = GetWindow<BuildScriptWindow>("Build Window", true);
            }

            bsw.Show();
        }

        private void OnEnable()
        {
            LoadPrefs();

            this.minSize = new Vector2(200, 17);

            this.wantsMouseMove = true;
        }

        private void OnFocus()
        {
            LoadPrefs();
        }

        public void LoadPrefs()
        {
            if (string.IsNullOrWhiteSpace(_matchmakerScene))
            {
                _matchmakerScene = BuildScriptPrefs.GetMatchmakerScene();
            }

            if (string.IsNullOrWhiteSpace(_serverOnlyScene))
            {
                _serverOnlyScene = BuildScriptPrefs.GetServerOnlyScene();
            }

            if (string.IsNullOrWhiteSpace(_clientOnlyScene))
            {
                _clientOnlyScene = BuildScriptPrefs.GetClientOnlyScene();
            }

            if (string.IsNullOrWhiteSpace(_singleOnlyScene))
            {
                _singleOnlyScene = BuildScriptPrefs.GetSingleOnlyScene();
            }

            if (string.IsNullOrWhiteSpace(_loadScene))
            {
                _loadScene = BuildScriptPrefs.GetLoadScene();
            }

            if (string.IsNullOrWhiteSpace(_levelSelectScene))
            {
                _levelSelectScene = BuildScriptPrefs.GetLevelSelectScene();
            }

            if (string.IsNullOrWhiteSpace(_buildFolder))
            {
                _buildFolder = BuildScriptPrefs.GetBuildFolder();
            }

            if (string.IsNullOrWhiteSpace(_serverIp))
            {
                _serverIp = BuildScriptPrefs.GetServerIP();
            }

            if (_serverPort == -1)
            {
                _serverPort = BuildScriptPrefs.GetServerPort();
            }

            if (_socketPort == -1)
            {
                _socketPort = BuildScriptPrefs.GetSocketPort();
            }

            _currentMode = BuildScriptPrefs.GetCurrentMode();
            _buildAndRun = BuildScriptPrefs.GetBuildAndRun();
            _useLocal = BuildScriptPrefs.GetUseLocal();
            _useHetzner = BuildScriptPrefs.GetUseHetzner();
            _devBuild = BuildScriptPrefs.GetDevelopmentBuild();
            _buildTarget = BuildScriptPrefs.GetBuildTarget();
            _autoConnectOnEnable = BuildScriptPrefs.GetAutoConnectOnEnable();

            LoadSceneStatusesFromPrefs();

            OnLoad();
        }

        partial void OnLoad();

        private void RepaintWhenMouseover(Rect rect)
        {
            // hack discovered at https://forum.unity.com/threads/gui-button-hover-change-text-color-solved.262440/
            // and tweaked a bit for performance
            // mouseOverWindow throws errors on multi monitors. does funny stuff sometimes. anyway this seems to work
            try
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (mouseOverWindow.GetType() == (typeof(BuildScriptWindow)))
                    {
                        Repaint();
                    }
                }
            }
            catch { }
        }

        private void OnGUI()
        {
            _scroll = GUILayout.BeginScrollView(_scroll);

            // Tabs => choosing mode
            RenderModeToolbar();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // Build and run GUI for each mode
            RenderBuildAndRunning();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // Additively loaded scene settings
            RenderAdditivelyLoadedScene();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // regular build settings scene list. none of this buggy shit.
            RenderBuildSettings_Array();
            // Build settings scene setup => reorderable list
            //RenderBuildSettings_ReorderableList();

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // Build folder
            RenderBuildFolder();

            // Server Deployment
            if (_currentMode == BuildMode.Server)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                RenderDeployment();
            }

            OnDrawGUI();

            GUILayout.EndScrollView();
        }

        partial void OnDrawGUI();

        private void RenderModeToolbar()
        {
            bool alternativeButtons = Event.current.shift;

            var tempGuiColor = GUI.color;
            Color quickBuildColor = Color.red;

            // rects for the additive scene buttons just below the toolbar.
            Rect rectButton_loader;
            Rect rectButton_singleplayer;
            Rect rectButton_client;
            Rect rectButton_server;
            Rect rectButton_matchmaker;
            Rect curButRect; // used when drawing the actual buttons
            Rect fullToolbarRect;

            EditorGUILayout.BeginHorizontal();
            var pressed = false;
            var newPressed = false;
            var buttonText = "";
            var buttonTooltip = "";

            // TODO: make these 5 semi-duplicate things into a single clever function called 5 times, or a loop

            // rects for the buttons
            curButRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.miniButtonLeft);
            fullToolbarRect = curButRect;
            rectButton_loader = curButRect;
            curButRect.width *= 0.8f;
            rectButton_loader.width *= 0.2f;
            rectButton_loader.x += curButRect.width;

            pressed = _currentMode == BuildMode.Complete;
            newPressed = pressed;
            buttonText = "Complete";
            buttonTooltip = "Switch to building a complete build using the load scene";
            if (pressed && alternativeButtons)
            {
                GUI.color = quickBuildColor;
                buttonText = "QUICK BUILD";
            }
            newPressed = GUI.Toggle(curButRect, pressed, new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButtonLeft);
            if (newPressed != pressed)
            {
                if (alternativeButtons)
                {
                    BuildButton_Complete();
                }
                else
                {
                    SwitchMode(BuildMode.Complete);
                }
            }
            GUI.color = tempGuiColor;

            // rects for the buttons
            curButRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.miniButtonLeft);
            fullToolbarRect.width += curButRect.width;
            rectButton_singleplayer = curButRect;
            curButRect.width *= 0.8f;
            rectButton_singleplayer.width *= 0.2f;
            rectButton_singleplayer.x += curButRect.width;

            pressed = _currentMode == BuildMode.Singleplayer;
            newPressed = pressed;
            buttonText = "Singleplayer";
            buttonTooltip = "Switch to building for Singleplayer mode.";
            if (pressed && alternativeButtons)
            {
                GUI.color = quickBuildColor;
                buttonText = "QUICK BUILD";
            }
            newPressed = GUI.Toggle(curButRect, pressed, new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButtonLeft);
            if (newPressed != pressed)
            {
                if (alternativeButtons)
                {
                    BuildButton_Singleplayer();
                }
                else
                {
                    SwitchMode(BuildMode.Singleplayer);
                }
            }
            GUI.color = tempGuiColor;

            // rects for the buttons
            curButRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.miniButtonLeft);
            fullToolbarRect.width += curButRect.width;
            rectButton_client = curButRect;
            curButRect.width *= 0.8f;
            rectButton_client.width *= 0.2f;
            rectButton_client.x += curButRect.width;

            pressed = _currentMode == BuildMode.Client;
            newPressed = pressed;
            buttonText = "Client";
            buttonTooltip = "Switch to building for Client mode.";
            if (pressed && alternativeButtons)
            {
                GUI.color = quickBuildColor;
                buttonText = "QUICK BUILD";
            }
            newPressed = GUI.Toggle(curButRect, pressed, new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButtonLeft);
            if (newPressed != pressed)
            {
                if (alternativeButtons)
                {
                    BuildButton_Client();
                }
                else
                {
                    SwitchMode(BuildMode.Client);
                }
            }
            GUI.color = tempGuiColor;

            // rects for the buttons
            curButRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.miniButtonLeft);
            fullToolbarRect.width += curButRect.width;
            rectButton_server = curButRect;
            curButRect.width *= 0.8f;
            rectButton_server.width *= 0.2f;
            rectButton_server.x += curButRect.width;

            pressed = _currentMode == BuildMode.Server;
            newPressed = pressed;
            buttonText = "Server";
            buttonTooltip = "Switch to building for Server mode.";
            if (pressed && alternativeButtons)
            {
                GUI.color = quickBuildColor;
                buttonText = "QUICK BUILD";
            }
            newPressed = GUI.Toggle(curButRect, pressed, new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButtonLeft);
            if (newPressed != pressed)
            {
                if (alternativeButtons)
                {
                    BuildButton_Server();
                }
                else
                {
                    SwitchMode(BuildMode.Server);
                }
            }
            GUI.color = tempGuiColor;

            // rects for the buttons
            curButRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.miniButtonLeft);
            fullToolbarRect.width += curButRect.width;
            rectButton_matchmaker = curButRect;
            curButRect.width *= 0.8f;
            rectButton_matchmaker.width *= 0.2f;
            rectButton_matchmaker.x += curButRect.width;

            pressed = _currentMode == BuildMode.MatchmakingServer;
            newPressed = pressed;
            buttonText = "Matchmaker";
            buttonTooltip = "Switch to building for Matchmaking server mode.";
            if (pressed && alternativeButtons)
            {
                GUI.color = quickBuildColor;
                buttonText = "QUICK BUILD";
            }
            newPressed = GUI.Toggle(curButRect, pressed, new GUIContent(buttonText, buttonTooltip), EditorStyles.miniButtonLeft);
            if (newPressed != pressed)
            {
                if (alternativeButtons)
                {
                    BuildButton_Matchmaker();
                }
                else
                {
                    SwitchMode(BuildMode.MatchmakingServer);
                }
            }
            GUI.color = tempGuiColor;

            EditorGUILayout.EndHorizontal();

            RepaintWhenMouseover(fullToolbarRect);

            if (alternativeButtons)
            {
                GUI.color = Color.green;

                DrawQuickRun(rectButton_loader, _loadScene, RunAsComplete);
                DrawQuickRun(rectButton_singleplayer, _singleOnlyScene, RunAsSingleplayer);
                DrawQuickRun(rectButton_client, _clientOnlyScene, RunAsClient);
                DrawQuickRun(rectButton_server, _serverOnlyScene, RunAsServer);
                DrawQuickRun(rectButton_matchmaker, _matchmakerScene, RunAsMatchmaker);

                GUI.color = tempGuiColor;
            }
            else
            {
                DrawLoadUnloadSceneSmall(rectButton_loader, _loadScene);
                DrawLoadUnloadSceneSmall(rectButton_singleplayer, _singleOnlyScene);
                DrawLoadUnloadSceneSmall(rectButton_client, _clientOnlyScene);
                DrawLoadUnloadSceneSmall(rectButton_server, _serverOnlyScene);
                DrawLoadUnloadSceneSmall(rectButton_matchmaker, _matchmakerScene);
            }
        }

        private void DrawQuickRun(Rect rect, string sceneNotToUnload, System.Action runCallback)
        {
            if (GUI.Button(rect, new GUIContent(">", "Quick Run"), EditorStyles.miniButtonRight))
            {
                // additively UNLOAD all other scenes
                //...
                UnloadAllLoadedScenesExcept(sceneNotToUnload);

                runCallback();
            }
        }

        private void UnloadAllLoadedScenesExcept(string sceneNotToUnload)
        {
            if (sceneNotToUnload != _loadScene)
                CloseSceneIfLoaded(_loadScene);
            if (sceneNotToUnload != _levelSelectScene)
                CloseSceneIfLoaded(_levelSelectScene);
            if (sceneNotToUnload != _singleOnlyScene)
                CloseSceneIfLoaded(_singleOnlyScene);
            if (sceneNotToUnload != _clientOnlyScene)
                CloseSceneIfLoaded(_clientOnlyScene);
            if (sceneNotToUnload != _serverOnlyScene)
                CloseSceneIfLoaded(_serverOnlyScene);
            if (sceneNotToUnload != _matchmakerScene)
                CloseSceneIfLoaded(_matchmakerScene);
        }

        private void CloseSceneIfLoaded(string scenePath)
        {
            Scene existingScene = SceneManager.GetSceneByPath(scenePath);
            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                CloseSceneWithDialog(existingScene);
            }
        }

        private void RenderAdditivelyLoadedScene()
        {
            EditorGUILayout.LabelField(new GUIContent("Additively Loaded Scene Path", "In order to facilitate different modes of play (Multiplayer Client & Server + Singleplayer), a scene is additively loaded to the 'base' scene with the specifics for that mode of play. The path for that scene is found underneath."), EditorStyles.centeredGreyMiniLabel);

            if (_currentMode == BuildMode.Singleplayer)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _singleOnlyScene = EditorGUILayout.TextField(new GUIContent("Singleplayer Only Scene:", "The path for the additively loaded scene used for the Singleplayer mode of play."), _singleOnlyScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetSingleOnlyScene(_singleOnlyScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _singleOnlyScene = sceneBrowse;
                        BuildScriptPrefs.SetSingleOnlyScene(_singleOnlyScene);
                    }

                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();

                DrawLoadUnloadScene(_singleOnlyScene);
            }
            else if (_currentMode == BuildMode.Client)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _clientOnlyScene = EditorGUILayout.TextField(new GUIContent("Client Only Scene:", "The path for the additively loaded scene used for the Client mode of play."), _clientOnlyScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetClientOnlyScene(_clientOnlyScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _clientOnlyScene = sceneBrowse;
                        BuildScriptPrefs.SetClientOnlyScene(_clientOnlyScene);
                    }

                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();

                DrawLoadUnloadScene(_clientOnlyScene);
            }
            else if (_currentMode == BuildMode.Server)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _serverOnlyScene = EditorGUILayout.TextField(new GUIContent("Server Only Scene:", "The path for the additively loaded scene used for the Server mode of play."), _serverOnlyScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetServerOnlyScene(_serverOnlyScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _serverOnlyScene = sceneBrowse;
                        BuildScriptPrefs.SetServerOnlyScene(_serverOnlyScene);
                    }

                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();

                DrawLoadUnloadScene(_serverOnlyScene);
            }
            else if (_currentMode == BuildMode.MatchmakingServer)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _matchmakerScene = EditorGUILayout.TextField(new GUIContent("Matchmaker Scene:", "The path to the matchmaker scene"), _matchmakerScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetMatchmakerScene(_matchmakerScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _matchmakerScene = sceneBrowse;
                        BuildScriptPrefs.SetMatchmakerScene(_matchmakerScene);
                    }

                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();

                DrawLoadUnloadScene(_matchmakerScene);
            }
            else if (_currentMode == BuildMode.Complete)
            {
                // load button width
                var loadButtonWidth = 60f;

                // load scene
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _loadScene = EditorGUILayout.TextField(new GUIContent("Load Scene:", "The path for the load scene."), _loadScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetLoadScene(_loadScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _loadScene = sceneBrowse;
                        BuildScriptPrefs.SetLoadScene(_loadScene);
                    }

                    GUIUtility.ExitGUI();
                }

                DrawLoadUnloadScene(_loadScene, loadButtonWidth);

                EditorGUILayout.EndHorizontal();

                // level select scene
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _levelSelectScene = EditorGUILayout.TextField(new GUIContent("Level Selection Scene:", "The path for the scene where you select levels"), _levelSelectScene, EditorStyles.textField);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetLevelSelectScene(_levelSelectScene);
                }

                if (GUILayout.Button(new GUIContent("Browse", "Browse for scenes in the Unity project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    var sceneBrowse = BrowseScene();
                    if (!string.IsNullOrEmpty(sceneBrowse))
                    {
                        _levelSelectScene = sceneBrowse;
                        BuildScriptPrefs.SetLevelSelectScene(_levelSelectScene);
                    }

                    GUIUtility.ExitGUI();
                }

                DrawLoadUnloadScene(_levelSelectScene, loadButtonWidth);

                EditorGUILayout.EndHorizontal();


            }
        }

        private void DrawLoadUnloadSceneSmall(Rect rect, string scenePath)
        {
            bool isPlaying = (Application.isPlaying);
            var tempGuiColor = GUI.color;
            if (isPlaying)
            {
                GUI.enabled = false;
            }

            var existingScene = SceneManager.GetSceneByPath(scenePath);
            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                // color shows the scene is currently loaded.
                GUI.color = Color.green;
                var butText = "-";
                if (existingScene.isDirty)
                {
                    butText = "*";
                }
                if (GUI.Button(rect, new GUIContent(butText, string.Concat("Unload Scene\n\"", existingScene.name, "\"\nRight click to save and close")), EditorStyles.miniButtonRight))
                {
                    CloseSceneWithDialog(existingScene);
                }
            }
            else
            {
                // remove all "/" or "\\" and the ".unity" at the end.
                var sceneNameFromPath = scenePath.Substring(Mathf.Max(scenePath.LastIndexOf("\\"), scenePath.LastIndexOf("/")) + 1);
                sceneNameFromPath = sceneNameFromPath.Substring(0, sceneNameFromPath.Length - 6);
                if (GUI.Button(rect, new GUIContent("+", string.Concat("Additively Load Scene\n\"", sceneNameFromPath, "\"")), EditorStyles.miniButtonRight))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    GUIUtility.ExitGUI();
                    base.Repaint();
                }
            }

            GUI.enabled = true;
            GUI.color = tempGuiColor;
        }

        private void DrawLoadUnloadScene(string scenePath, float width = -1)
        {
            bool isPlaying = (Application.isPlaying);
            var tempGuiColor = GUI.color;
            if (isPlaying)
            {
                GUI.enabled = false;
            }

            var existingScene = SceneManager.GetSceneByPath(scenePath);
            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                GUI.color = Color.green;
                var unloadGuiContent = new GUIContent(string.Concat("Unload"), "Press the button to unload the additively loaded scene now in the editor. Right click to save and unload.");
                if (width == -1 ? GUILayout.Button(unloadGuiContent) : GUILayout.Button(unloadGuiContent, GUILayout.Width(width)))
                {
                    CloseSceneWithDialog(existingScene);
                }
            }
            else
            {
                var loadGuiContent = new GUIContent("Load", "Press the button to additively load the set scene now in the editor.");
                if (width == -1 ? GUILayout.Button(loadGuiContent) : GUILayout.Button(loadGuiContent, GUILayout.Width(width)))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    GUIUtility.ExitGUI();
                }
            }

            GUI.enabled = true;
            GUI.color = tempGuiColor;
        }

        private static void CloseSceneWithDialog(Scene existingScene)
        {
            // if dirty scene (unsaved), asks user for close scene confirmation
            bool closeScene = true;

            // only save scene if dirty, or if user clicked with right mouse button (for quick operation), or if user wants it 
            bool saveScene = false;

            bool usedRightMouseButton = Event.current.button == 1;
            if (existingScene.isDirty)
            {
                saveScene = true;
                if (!usedRightMouseButton)
                {
                    int result = EditorUtility.DisplayDialogComplex("Unsaved scene", "Scene\n\"" + existingScene.name + "\"\nhas unsaved changes. What to do?", "Save and close", "Close &without saving", "Cancel");
                    if (result == 0)
                    {
                        closeScene = true;
                        saveScene = true;
                    }
                    else if (result == 1)
                    {
                        closeScene = true;
                        saveScene = false;
                    }
                    else
                    {
                        closeScene = false;
                        saveScene = false;
                    }
                }
                else
                {
                    saveScene = true;
                    closeScene = true;
                }
            }
            if (saveScene)
            {
                EditorSceneManager.SaveScene(existingScene);
            }
            if (closeScene)
            {
                EditorSceneManager.CloseScene(existingScene, true);
                GUIUtility.ExitGUI();
            }
        }

        private void RenderBuildSettings_Array()
        {
            EditorGUILayout.LabelField(new GUIContent("Build Settings Scenes", "Same as the build settings scene list"), EditorStyles.centeredGreyMiniLabel);

            var unityScenes = EditorBuildSettings.scenes;

            // check if current scene is in build settings or not.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var curScene = SceneManager.GetSceneAt(i);
                if (unityScenes.Any(ebss => ebss.path == curScene.path))
                {

                }
                else
                {
                    var preCol = GUI.color;
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(new GUIContent("WARNING: Scene \"" + curScene.name + "\" is not in BuildSettings. Remember to add it before building!"), EditorStyles.miniBoldLabel);
                    GUI.color = preCol;
                }
            }

            ValidateSceneStatuses_BuildSceneCounts();
            SceneEnabledState[] sceneEnabledStatus = sceneStatusesSingleplayer;
            switch (_currentMode)
            {
                case BuildMode.Singleplayer:
                    sceneEnabledStatus = sceneStatusesSingleplayer;
                    break;
                case BuildMode.Client:
                    sceneEnabledStatus = sceneStatusesClient;
                    break;
                case BuildMode.Server:
                    sceneEnabledStatus = sceneStatusesServer;
                    break;
                case BuildMode.MatchmakingServer:
                    sceneEnabledStatus = sceneStatusesMatchmaker;
                    break;
                case BuildMode.Complete:
                    sceneEnabledStatus = sceneStatusesComplete;
                    break;
                default:
                    break;
            }

            // NOTE: cannot reorder scenes right now. can only enable/disable. consider making a separate window/mode for reordering.... or a smart implementation...

            var activeSceneCounter = 0;
            var changed = false;
            if (unityScenes.Length == 0)
            {
                EditorGUILayout.LabelField("no scenes");
            }
            for (int i = 0; i < unityScenes.Length; i++)
            {
                var scene = unityScenes[i];

                // SCENE DRAWING ROW
                using (var sceneRow = new EditorGUILayout.HorizontalScope())
                {
                    string scenePath;
                    if (scene != null)
                    {
                        scenePath = scene.path;
                        if (scenePath.Length >= 7)
                            scenePath = scenePath.Substring("Assets/".Length);

                        // forced scene enabled toggle, per current mode
                        {
                            var curSceneStatus = sceneEnabledStatus[i];

                            var toggleState = curSceneStatus != SceneEnabledState.None;
                            GUI.color = curSceneStatus == SceneEnabledState.ForcedActive ? Color.green : curSceneStatus == SceneEnabledState.ForcedInactive ? Color.red : Color.white;
                            var newSceneToggle = EditorGUILayout.ToggleLeft(GUIContent.none, toggleState, GUILayout.Width(12), GUILayout.Height(12));
                            if (newSceneToggle != toggleState)
                            {
                                curSceneStatus = (SceneEnabledState)(((int)curSceneStatus + 1) % 3);
                                sceneEnabledStatus[i] = curSceneStatus;
                                SaveSceneStatusesFromPrefs();
                            }
                            GUI.color = Color.white;
                        }

                        // regular scene enabled toggle
                        var newEnabled = EditorGUILayout.ToggleLeft(GUIContent.none, scene.enabled, GUILayout.Width(12), GUILayout.Height(12));
                        if (newEnabled != scene.enabled)
                        {
                            scene.enabled = newEnabled;
                            changed = true;
                        }
                    }
                    else
                    {
                        scenePath = "null";
                    }

                    if (GUILayout.Button("Open", GUILayout.Width(40), GUILayout.Height(16)))
                    {
                        EditorSceneManager.OpenScene(Path.Combine("Assets", scenePath), OpenSceneMode.Single);
                    }

                    if (GUILayout.Button(new GUIContent(" ", "Select & Ping the scene in editor"), GUILayout.Width(20f), GUILayout.Height(16f)))
                    {
                        var path = Path.Combine("Assets", scenePath);
                        var a = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                        if (a == null)
                        {
                            Debug.LogError($"Could not find a scene asset at path: {path}");
                        }
                        else
                        {
                            Selection.activeObject = a;
                            EditorGUIUtility.PingObject(a);
                        }
                    }

                    EditorGUILayout.SelectableLabel(scenePath, GUILayout.Height(17));

                    // delete button
                    var prevColor = GUI.color;
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(16)))
                    {
                        unityScenes = unityScenes.RemoveAt(i);
                        changed = true;
                    }
                    GUI.color = prevColor;

                    if (scene != null)
                    {
                        // active scene order
                        var styleCalc = new GUIStyle(GUI.skin.label);
                        var counterString = activeSceneCounter.ToString();
                        var actualPrint = scene.enabled ? counterString : "";
                        EditorGUILayout.LabelField(actualPrint, GUILayout.Width(styleCalc.CalcSize(new GUIContent(counterString)).x));
                        if (scene.enabled)
                        {
                            activeSceneCounter++;
                        }
                    }
                }
            }

            // add open scenes button on the right...
            using (var buttonsBottmRekt = new EditorGUILayout.HorizontalScope())
            {
                //var r = buttonsBottmRekt.rect;
                //r.width = 100;
                //r.x = buttonsBottmRekt.rect.width - r.width;
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open BS"))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
                }

                if (GUILayout.Button("Clear All"))
                {
                    // first disable all, then remove them. this helps unwanted clickery
                    if (unityScenes.Any(u => u.enabled))
                    {
                        if (EditorUtility.DisplayDialog("Clear all scenes", "Are you sure? Deactivate all scenes in build menu?", "Yes", "No"))
                        {
                            for (int i = 0; i < unityScenes.Length; i++)
                            {
                                unityScenes[i].enabled = false;
                            }
                            changed = true;
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Clear all scenes", "Are you sure? DELETE all scenes from build menu?", "Yes", "No"))
                        {
                            unityScenes = new EditorBuildSettingsScene[0];
                            changed = true;
                        }
                    }
                }

                var addOpenScenesContent = new GUIContent("Add Open Scenes");
                if (GUILayout.Button(addOpenScenesContent))
                {
                    var extraScenes = new List<EditorBuildSettingsScene>(SceneManager.sceneCount);
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        var newScenePath = SceneManager.GetSceneAt(i).path;
                        // if not already in build settings
                        if (!EditorBuildSettings.scenes.Any(s => s.path == newScenePath))
                        {
                            extraScenes.Add(new EditorBuildSettingsScene(newScenePath, true));
                        }
                    }
                    unityScenes = unityScenes.Concat(extraScenes).ToArray();
                    changed = true;
                }
            }

            // apply changes the ghetto way
            if (changed)
            {
                EditorBuildSettings.scenes = unityScenes;
            }
        }

        private void RenderBuildAndRunning()
        {
            EditorGUILayout.LabelField(new GUIContent("Building & Running (" + _currentMode.ToString() + ")", "Building and optionally auto-running builds."), EditorStyles.centeredGreyMiniLabel);

            EditorGUI.BeginChangeCheck();

            _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(new GUIContent("Build Target", "Which platform to build for."), _buildTarget);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetBuildTarget(_buildTarget);
            }

            EditorGUI.BeginChangeCheck();

            _devBuild = EditorGUILayout.Toggle(new GUIContent("Development Build", "Toggle to build as development build."), _devBuild);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetDevelopmentBuild(_devBuild);
            }

            EditorGUI.BeginChangeCheck();
            _buildAndRun = EditorGUILayout.Toggle(new GUIContent("Build and Run?", "Whether to auto-run the game after building, if successful."), _buildAndRun);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetBuildAndRun(_buildAndRun);
            }

            // Actual mode pages
            switch (_currentMode)
            {
                case BuildMode.Singleplayer:
                {
                    OnSingleplayerGUI();
                    break;
                }

                case BuildMode.Client:
                {
                    OnClientGUI();
                    break;
                }

                case BuildMode.Server:
                {
                    OnServerGUI();
                    break;
                }

                case BuildMode.MatchmakingServer:
                {
                    OnMatchmakingServerGUI();
                    break;
                }

                case BuildMode.Complete:
                {
                    OnCompleteGUI();
                    break;
                }
            }

            // vr mode
            using (var vrrow = new EditorGUILayout.HorizontalScope())
            {
                // buttons for VR platform definition - easier to access than player settings

                var devices = XRSettings.supportedDevices;
                var goodColor = Color.green;
                var badColor = Color.red;

                var none = "None";
                var openVR = "OpenVR";
                var oculus = "Oculus";

                EditorGUIUtility.labelWidth = 24;
                GUI.enabled = !Application.isPlaying;
                var val = EditorGUILayout.Toggle("VR", PlayerSettings.virtualRealitySupported);
                if (!Application.isPlaying && val != PlayerSettings.virtualRealitySupported)
                {
                    PlayerSettings.virtualRealitySupported = val;
                }
                GUI.enabled = true;
                EditorGUIUtility.labelWidth = 0; // default

                if (GUILayout.Button("Menu", GUILayout.Width(50f)))
                {
                    // must disable menu first and then select.... for it to update.... STUPID UNITY!!!!
                    EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
                }

                GUI.color = devices.Contains(none) ? goodColor : badColor;
                if (GUILayout.Button(none + " (" + devices.IndexOf(none) + ")"))
                {
                    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_buildTarget);
                    List<string> newDevices;
                    if (devices.Contains(none))
                    {
                        newDevices = devices.ToList();
                        newDevices.Remove(none);
                    }
                    else // devices no contain
                    {
                        newDevices = devices.ToList();
                        newDevices.Insert(0, none);
                    }
                    if (newDevices.Count > 0)
                    {
                        PlayerSettings.virtualRealitySupported = true;
                        XRSettings.enabled = true;
                    }
                    else
                    {
                        PlayerSettings.virtualRealitySupported = false;
                        XRSettings.enabled = false;

                    }

                    VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, newDevices.ToArray());
                }

                GUI.color = devices.Contains(openVR) ? goodColor : badColor;
                if (GUILayout.Button(openVR + " (" + devices.IndexOf(openVR) + ")"))
                {
                    PlayerSettings.virtualRealitySupported = true;
                    XRSettings.enabled = true;
                    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_buildTarget);
                    List<string> newDevices;
                    if (devices.Contains(openVR))
                    {
                        newDevices = devices.ToList();
                        newDevices.Remove(openVR);
                    }
                    else // devices no  contain
                    {
                        newDevices = devices.ToList();
                        newDevices.Insert(0, openVR);
                    }
                    VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, newDevices.ToArray());
                }

                GUI.color = devices.Contains(oculus) ? goodColor : badColor;
                if (GUILayout.Button(oculus + " (" + devices.IndexOf(oculus) + ")"))
                {
                    PlayerSettings.virtualRealitySupported = true;
                    XRSettings.enabled = true;
                    var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_buildTarget);
                    List<string> newDevices;
                    if (devices.Contains(oculus))
                    {
                        newDevices = devices.ToList();
                        newDevices.Remove(oculus);
                    }
                    else // devices no contain
                    {
                        newDevices = devices.ToList();
                        newDevices.Insert(0, oculus);
                    }
                    VREditor.SetVREnabledDevicesOnTargetGroup(buildTargetGroup, newDevices.ToArray());
                }

                GUI.color = Color.white;
            }
        }

        private void RenderBuildFolder()
        {
            EditorGUILayout.LabelField(new GUIContent("Build Folder", "The folder to place builds in."), EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _buildFolder = EditorGUILayout.TextField(new GUIContent("Build Folder:", "The folder to place builds in."), _buildFolder);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetBuildFolder(_buildFolder);
            }

            if (GUILayout.Button(new GUIContent("Browse", "Browse for folders in the project."), EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.fieldWidth)))
            {
                SetBuildFolder();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RenderDeployment()
        {
            EditorGUILayout.LabelField(new GUIContent("Server Deployment", "Automatic deploment to Hetzner server using WinSCP."), EditorStyles.centeredGreyMiniLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset Online Server"))
            {
                DeployScripts.ResetServers();
                GUIUtility.ExitGUI();
            }

            if (DeployScripts.DeployServersValidation())
            {
                if (GUILayout.Button("Deploy to Hetzner"))
                {
                    DeployScripts.DeployServers();
                    GUIUtility.ExitGUI();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please make sure that you have built a server. Expecting to find server executable at path = " + DeployScripts.GetServerFilePath(), MessageType.Error);
            }

            if (GUILayout.Button("Build and Deploy"))
            {
                BuildScripts.BuildServer64(_serverOnlyScene, _buildFolder, false, BuildTarget.StandaloneLinux64, _socketPort);
                DeployScripts.DeployServers();
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Link to deployment google doc"))
            {
                Application.OpenURL(@"https://docs.google.com/document/d/1Ecy0n1JQWaICvW68l0zxcmgtMhjvU3e5twXNqi2jj_Q");
            }
        }

        private void SwitchMode(BuildMode mode)
        {
            _currentMode = mode;
            BuildScriptPrefs.SetCurrentMode(_currentMode);

            base.Repaint();
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

        private string BrowseScene()
        {
            var fullPath = EditorUtility.OpenFilePanelWithFilters("Browse Scenes", Path.Combine(Application.dataPath, "Scenes"), new string[] { "Scene Files", "unity" });
            var path = fullPath.Substring(fullPath.IndexOf("Assets"));
            if (!string.IsNullOrEmpty(path))
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene != null)
                {
                    if (!Path.HasExtension(path))
                    {
                        path += ".unity";
                    }

                    return path;
                }
                else
                {
                    Debug.LogWarning(this.ToString() + " please make sure you select a valid scene!");
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private void OnSingleplayerGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build for Singleplayer"))
            {
                BuildButton_Singleplayer();
            }

            if (GUILayout.Button("Run as Singleplayer"))
            {
                RunAsSingleplayer();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildButton_Singleplayer()
        {
            SetSceneStatuses();

            if (!Directory.Exists(_buildFolder))
            {
                SetBuildFolder();
            }

            if (_buildAndRun)
            {
                BuildScripts.BuildAndRunSingleplayer64(_singleOnlyScene, _buildFolder, _devBuild, _buildTarget);
            }
            else
            {
                BuildScripts.BuildSingleplayer64(_singleOnlyScene, _buildFolder, _devBuild, _buildTarget);
            }

            GUIUtility.ExitGUI();
        }

        private void RunAsSingleplayer()
        {
            SetSceneStatuses();
            BuildScripts.PlayAsSingleplayer(_singleOnlyScene);
            GUIUtility.ExitGUI();
        }

        private void OnClientGUI()
        {
            EditorGUI.BeginChangeCheck();
            _autoConnectOnEnable = EditorGUILayout.Toggle(new GUIContent("Auto Connect On Enable?", "Whether the ClientNetSender automatically connects on enable to the set IP and port."), _autoConnectOnEnable);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetAutoConnectOnEnable(_autoConnectOnEnable);
            }

            if (_autoConnectOnEnable)
            {
                EditorGUI.BeginChangeCheck();
                _useLocal = EditorGUILayout.Toggle(new GUIContent("Connect to Localhost?", "Whether the client should connect to a localhost (127.0.0.1), or another custom IP address."), _useLocal);

                if (EditorGUI.EndChangeCheck())
                {
                    if (_useHetzner && _useLocal)
                    {
                        _useHetzner = false;
                        BuildScriptPrefs.SetUseHetzner(_useHetzner);
                    }

                    BuildScriptPrefs.SetUseLocal(_useLocal);
                }

                EditorGUI.BeginChangeCheck();
                _useHetzner = EditorGUILayout.Toggle(new GUIContent("Connect to Hetzner Server?", "Whether the client should connect to the hetzner remote server (88.198.75.133), or another custom IP address."), _useHetzner);

                if (EditorGUI.EndChangeCheck())
                {
                    if (_useLocal && _useHetzner)
                    {
                        _useLocal = false;
                        BuildScriptPrefs.SetUseLocal(_useLocal);
                    }

                    BuildScriptPrefs.SetUseHetzner(_useHetzner);
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(new GUIContent("Server Address:", "The IP + port address of the server, which the client should connect to."), GUILayout.Width(EditorGUIUtility.labelWidth));

                if (!_useLocal && !_useHetzner)
                {
                    EditorGUI.BeginChangeCheck();
                    _serverIp = EditorGUILayout.TextField(_serverIp);

                    if (EditorGUI.EndChangeCheck())
                    {
                        BuildScriptPrefs.SetServerIP(_serverIp);
                    }
                }
                else if (_useLocal)
                {
                    EditorGUILayout.SelectableLabel(BuildConstants.localhost);
                }
                else if (_useHetzner)
                {
                    EditorGUILayout.SelectableLabel(BuildConstants.hetznerIp);
                }

                EditorGUI.BeginChangeCheck();
                _serverPort = EditorGUILayout.IntField(_serverPort);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetServerPort(_serverPort);
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _serverPort = EditorGUILayout.IntField(new GUIContent("Server Port: ", "The server port to connect to. Should be the same port as on the server ;)"), _serverPort);

                if (EditorGUI.EndChangeCheck())
                {
                    BuildScriptPrefs.SetServerPort(_serverPort);
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build for Client"))
            {
                BuildButton_Client();
            }

            if (GUILayout.Button("Run as Client"))
            {
                RunAsClient();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildButton_Client()
        {
            SetSceneStatuses();

            if (!Directory.Exists(_buildFolder))
            {
                SetBuildFolder();
            }

            if (_useLocal)
            {
                if (_buildAndRun)
                {
                    BuildScripts.BuildAndRunClientLocal(_clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _serverPort, _autoConnectOnEnable);
                }
                else
                {
                    BuildScripts.BuildClientLocal(_clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _serverPort, _autoConnectOnEnable);
                }
            }
            else if (_useHetzner)
            {
                if (_buildAndRun)
                {
                    BuildScripts.BuildAndRunForHetzner(_clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _serverPort, _autoConnectOnEnable);
                }
                else
                {
                    BuildScripts.BuildClientForHetzner(_clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _serverPort, _autoConnectOnEnable);
                }
            }
            else
            {
                if (_buildAndRun)
                {
                    BuildScripts.BuildAndRunClientForIP(_serverIp, _serverPort, _clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _autoConnectOnEnable);
                }
                else
                {
                    BuildScripts.BuildClientForIP(_serverIp, _serverPort, _clientOnlyScene, _buildFolder, _devBuild, _buildTarget, _autoConnectOnEnable);
                }
            }

            GUIUtility.ExitGUI();
        }

        private void RunAsClient()
        {
            SetSceneStatuses();
            if (_useLocal)
            {
                BuildScripts.PlayAsClientLocal(_serverPort, _clientOnlyScene, _autoConnectOnEnable);
            }
            else if (_useHetzner)
            {
                BuildScripts.PlayAsHetzner(_serverPort, _clientOnlyScene, _autoConnectOnEnable);
            }
            else
            {
                BuildScripts.PlayAsClientForIP(_serverIp, _serverPort, _clientOnlyScene, _autoConnectOnEnable);
            }

            GUIUtility.ExitGUI();
        }

        private void OnServerGUI()
        {
            EditorGUI.BeginChangeCheck();
            _socketPort = EditorGUILayout.IntField(new GUIContent("Socket Port", "The socket port used to host this server through."), _socketPort);

            if (EditorGUI.EndChangeCheck())
            {
                BuildScriptPrefs.SetSocketPort(_socketPort);
            }

            EditorGUILayout.Separator();

            var ip = IPUtils.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            if (!string.IsNullOrEmpty(ip))
            {
                EditorGUILayout.DelayedTextField(new GUIContent("Ethernet IP v4 Address:", "Automatically identified local IP V4 address for Ethernet (Read Only)."), ip);
            }

            ip = IPUtils.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (!string.IsNullOrEmpty(ip))
            {
                EditorGUILayout.DelayedTextField(new GUIContent("Wireless IP v4 Address:", "Automatically identified local IP V4 address for Wireless (Read Only)."), ip);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build for Server"))
            {
                BuildButton_Server();
            }

            if (GUILayout.Button("Run as Server"))
            {
                RunAsServer();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildButton_Server()
        {
            SetSceneStatuses();

            if (!Directory.Exists(_buildFolder))
            {
                SetBuildFolder();
            }

            if (_buildAndRun)
            {
                BuildScripts.BuildAndRunServer64(_serverOnlyScene, _buildFolder, _devBuild, _buildTarget, _socketPort);
            }
            else
            {
                BuildScripts.BuildServer64(_serverOnlyScene, _buildFolder, _devBuild, _buildTarget, _socketPort);
            }

            GUIUtility.ExitGUI();
        }

        private void RunAsServer()
        {
            SetSceneStatuses();
            BuildScripts.PlayAsServer(_serverOnlyScene, _socketPort);
            GUIUtility.ExitGUI();
        }

        private void OnMatchmakingServerGUI()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build Matchmaker"))
            {
                BuildButton_Matchmaker();
            }

            if (GUILayout.Button("Run Matchmaker"))
            {
                RunAsMatchmaker();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildButton_Matchmaker()
        {
            SetSceneStatuses();

            if (!Directory.Exists(_buildFolder))
            {
                SetBuildFolder();
            }

            if (_buildAndRun)
            {
                BuildScripts.BuildAndRunMatchmaker64(_matchmakerScene, _buildFolder, _devBuild, _buildTarget);
            }
            else
            {
                BuildScripts.BuildMatchmaker64(_matchmakerScene, _buildFolder, _devBuild, _buildTarget);
            }

            GUIUtility.ExitGUI();
        }

        private void RunAsMatchmaker()
        {
            SetSceneStatuses();
            BuildScripts.PlayAsMatchmaker(_matchmakerScene);
            GUIUtility.ExitGUI();
        }

        private void OnCompleteGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox("The Load Scene must be the first scene in the build settings (this is automatically ensured), and the second scene should be the actual game scene. Additively loaded scenes for singleplayer, client, server and matchmaker are automatically added if not set.", MessageType.Info);

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build Complete"))
            {
                BuildButton_Complete();
            }

            if (GUILayout.Button("Run Complete"))
            {
                RunAsComplete();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildButton_Complete()
        {
            SetSceneStatuses();

            if (!Directory.Exists(_buildFolder))
            {
                SetBuildFolder();
            }

            if (_buildAndRun)
            {
                BuildScripts.BuildComplete(true, _devBuild, _buildTarget, _buildFolder, _loadScene, new string[] { _singleOnlyScene, _clientOnlyScene, _serverOnlyScene, _matchmakerScene });
            }
            else
            {
                BuildScripts.BuildComplete(false, _devBuild, _buildTarget, _buildFolder, _loadScene, new string[] { _singleOnlyScene, _clientOnlyScene, _serverOnlyScene, _matchmakerScene });
            }

            GUIUtility.ExitGUI();
        }

        private void RunAsComplete()
        {
            SetSceneStatuses();
            BuildScripts.PlayAsComplete(_loadScene);
            GUIUtility.ExitGUI();
        }
    }
}

#endif