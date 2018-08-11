using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using KSP.UI.Screens;


using ClickThroughFix;
using ToolbarControl_NS;

namespace KSP___ActionGroupEngines
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(ChangeThrustLimiter.MODID, ChangeThrustLimiter.MODNAME);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class ChangeThrustLimiter : MonoBehaviour
    {
        internal const string MODID = "ThrottleLimitExtended_NS";
        internal const string MODNAME = "Throttle Limit";

        ToolbarControl toolbarControl;
        bool WindowVisible = false;
        private bool m_UIHidden = false;

        const float SEQ_WIDTH = 100;
        const float SEQ_HEIGHT = 200;
        protected Rect windowPos;

        internal static GUIStyle yellowLabel = null;     

        public class EngineInfo
        {
            public string engineType;
            public bool update;

            public EngineInfo()
            {
                engineType = "";
                update = false;
            }
        }

        List<EngineInfo> engineTypes;

        void GetEngineType( PartModule m , ref string engineType)
        {
            if (m is ModuleEngines)
            {
                ModuleEngines me = (ModuleEngines)m;
 
                engineType = me.GetEngineType() + ": ";
                foreach (var p in me.propellants.OrderBy(n => n.displayName))
                {
                    if (engineType.Substring(engineType.Length - 2) != ": ")

                        engineType += ", ";
                    engineType += p.displayName;
                }
            }
            if (m is ModuleEnginesFX && m.isEnabled) // Squad, y u have separate module for NASA engines? :c
            {
                ModuleEnginesFX me = (ModuleEnginesFX)m;
        
                engineType = me.GetEngineType() + ": ";
                foreach (var p in me.propellants.OrderBy(n => n.displayName))
                {
                    if (engineType.Substring(engineType.Length - 2) != ": " )
                        engineType += ", ";
                    engineType += p.displayName;
                }
            }
            if (engineType != "")
                Log.Info("GetEnginetype, engineType: " + engineType);
        }

        void GetEngineTypes()
        {
            engineTypes = new List<EngineInfo>();

            foreach (var part in FlightGlobals.ActiveVessel.Parts)
            {
               
                foreach (PartModule m in part.Modules)
                {
                    EngineInfo engineInfo = new EngineInfo();
                    Log.Info("part: " + part.partInfo.title);
                    GetEngineType(m, ref engineInfo.engineType);


                    if (engineInfo.engineType != "")
                    {
                        Log.Info("engineType: [" + engineInfo + "]");
                        bool found = false;
                        foreach (var et in engineTypes)
                        {
                            Log.Info("et.engineType: " + et.engineType);
                            if (et.engineType == engineInfo.engineType)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            engineTypes.Add(engineInfo);
                    }
                }
            }
            engineTypes.Sort((x, y) => string.Compare(x.engineType, y.engineType));
        }

        public void Start()
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().thrustLimitWindow)
            {
                CreateButtonIcon();

                GameEvents.onShowUI.Add(OnShowUI);
                GameEvents.onHideUI.Add(OnHideUI);
                GameEvents.onGUILock.Add(OnHideUI);
                GameEvents.onGUIUnlock.Add(OnShowUI);
                GameEvents.onGamePause.Add(OnHideUI);
                GameEvents.onGameUnpause.Add(OnShowUI);
            }

            LoadWindowPositions();
        }

        string SETTINGSNAME = "ThrottleLimitExtended";
        string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/ThrottleLimitExtended/PluginData/ThrottleLimitExtended.cfg";
        string winName = "ThrustLimitWin_";

        internal void SaveWinPos(Rect win)
        {
            ConfigNode settingsFile = new ConfigNode();
            ConfigNode settings = new ConfigNode();
            settingsFile.SetNode(SETTINGSNAME, settings, true);

            settings.SetValue(winName + "X", (win.x + 1).ToString(), true);
            settings.SetValue(winName + "Y", (win.y + 1).ToString(), true);

            settingsFile.Save(PLUGINDATA);
        }

        static public string SafeLoad(string value, double oldvalue)
        {
            if (value == null)
                return oldvalue.ToString();
            return value;
        }

        Rect GetWinPos(ConfigNode settings, string winName, float width, float height, bool initialScaled = true)
        {
            double x = (Screen.width - width) / 2;
            double y = (Screen.height - height) / 2;
            Rect r;
            Log.Info("GetWinPos, winName: " + winName);
            if (settings != null && settings.HasValue(winName + "X"))
            {
                var x1 = Double.Parse(SafeLoad(settings.GetValue(winName + "X"), x));
                var y1 = Double.Parse(SafeLoad(settings.GetValue(winName + "Y"), y));
                if (x1 > 0) x = x1 - 1;
                if (y1 > 0) y = y1 - 1;

                r = new Rect((float)x, (float)y, (float)width, (float)height);
            }
            else
            {
                Log.Info("GetWinPos,centering window");
                r = GUIUtil.ScreenCenteredRect(width, height);
            }

            return r;
        }

        public void LoadWindowPositions()
        {
            Log.Info("LoadWindowPositions");

            ConfigNode settingsFile;
            ConfigNode settings = new ConfigNode();
            
            settingsFile = ConfigNode.Load(PLUGINDATA);
            if (settingsFile != null)
            {
                settings = settingsFile.GetNode(SETTINGSNAME);

                windowPos = GetWinPos(settings, winName, SEQ_WIDTH, SEQ_HEIGHT, false);
            }
        }

        void OnDestroy()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);

            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onGUILock.Remove(OnHideUI);
            GameEvents.onGUIUnlock.Remove(OnShowUI);
            GameEvents.onGamePause.Remove(OnHideUI);
            GameEvents.onGameUnpause.Remove(OnShowUI);
            SaveWinPos(windowPos);
        }

        private void CreateButtonIcon()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleWin, ToggleWin,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                MODID,
                "TLE_Button",
                "ThrottleLimitExtended/PluginData/Textures/throttle-38",
                "ThrottleLimitExtended/PluginData/Textures/throttle-24",
                MODNAME
            );
        }

        private void OnShowUI()
        {
            m_UIHidden = false;
        }

        private void OnHideUI()
        {
            m_UIHidden = true;
        }

        void ToggleWin()
        {

            WindowVisible = !WindowVisible;
            if (WindowVisible)
                GetEngineTypes();
            else
                SaveWinPos(windowPos);
        }


        private void OnGUI()
        {
            if (yellowLabel == null)
            {
                yellowLabel = new GUIStyle(GUI.skin.label);
                yellowLabel.normal.textColor = Color.yellow;
                yellowLabel.fontStyle = FontStyle.Bold;


            }
            if (WindowVisible && !m_UIHidden)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().useAlternativeSkin)
                    GUI.skin = HighLogic.Skin;
                windowPos = ClickThruBlocker.GUILayoutWindow(43578591, windowPos, WindowGUI, "Change Thrust Limits", GUILayout.MinWidth(300));

            }
        }

        bool allActiveEngines = false;
        bool allInactiveEngines = false;
        bool allRCS = false;

        float throttleLimit = 100f;
        float origThrottleLimit = 100f;

        bool apply = false;

        private void WindowGUI(int windowID)
        {
            GUILayout.BeginHorizontal();
            allActiveEngines = GUILayout.Toggle(allActiveEngines, "Apply to all active engines");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            allInactiveEngines = GUILayout.Toggle(allInactiveEngines, "Apply to all inactive engines");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            allRCS = GUILayout.Toggle(allRCS, "Apply to all RCS");
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Apply to the following Engine Types:");
            GUILayout.EndHorizontal();
            foreach (var et in engineTypes)
            {
                    GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                    et.update = GUILayout.Toggle(et.update, et.engineType);
                    GUILayout.EndHorizontal();

            }
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUIStyle s;
            if (throttleLimit != origThrottleLimit && yellowLabel != null)
            {
                s = yellowLabel;
            }
            else
                s = new GUIStyle(GUI.skin.label);
            GUILayout.Label("Thrust Limit (" + throttleLimit.ToString("F1") + "): ", s);
            GUILayout.FlexibleSpace();
            throttleLimit = GUILayout.HorizontalSlider(throttleLimit, 0, 100, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Instant Presets", yellowLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set all to " + HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetOne + "% "))
            {
                throttleLimit = HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetOne;
                apply = true;
            }

            if (GUILayout.Button("Set all to " + HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetTwo + "%"))
            {
                throttleLimit = HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetTwo;
                apply = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set all to " + HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetThree + "%"))
            {
                throttleLimit = HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetThree;
                apply = true;
            }

            if (GUILayout.Button("Set all to " + HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetFour + "%"))
            {
                throttleLimit = HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().presetFour;
                apply = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (throttleLimit == origThrottleLimit)
                GUI.enabled = false;
            if (GUILayout.Button("Apply"))
            {
                apply = true;
            }
            if (apply)
            {
                apply = false;
                throttleLimit = (float)Math.Round(throttleLimit, 1);
                origThrottleLimit = throttleLimit;
                setThrustLimit(throttleLimit);
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                toolbarControl.SetFalse(true);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void setThrustLimit(float limit)
        {
            foreach (var part in FlightGlobals.ActiveVessel.Parts)
            {
                foreach (PartModule m in part.Modules)
                {

                    // Do the "all active" and "all inactive" here
                    if (m is ModuleEngines)
                    {
                        ModuleEngines me = (ModuleEngines)m;
                        if ((me.EngineIgnited && allActiveEngines) ||
                            (!me.EngineIgnited && allInactiveEngines))
                            me.thrustPercentage = limit;
                    }
                    if (m is ModuleEnginesFX && m.isEnabled) // Squad, y u have separate module for NASA engines? :c
                    {
                        ModuleEnginesFX me = (ModuleEnginesFX)m;
                        if ((me.EngineIgnited && allActiveEngines) ||
                             (!me.EngineIgnited && allInactiveEngines))
                            me.thrustPercentage = limit;
                    }

                    // Do the RCS here
                    if (m is ModuleRCS)
                    {
                        ModuleRCS me = (ModuleRCS)m;

                        me.thrustPercentage = limit;
                    }
                    if (m is ModuleRCSFX && m.isEnabled) // Squad, y u have separate module for NASA engines? :c
                    {
                        ModuleRCSFX me = (ModuleRCSFX)m;

                        me.thrustPercentage = limit;
                    }

                    // Do the enginetype here

                    string engineType = "";
                    GetEngineType(m, ref engineType);
                    foreach (var et in engineTypes)
                    {
                        if (et.update)
                        {
                            if(engineType == et.engineType)
                            {
                                if (m is ModuleEngines)
                                {
                                    ModuleEngines me = (ModuleEngines)m;
                                    me.thrustPercentage = limit;
                                }
                                if (m is ModuleEnginesFX)
                                {
                                    ModuleEnginesFX me = (ModuleEnginesFX)m;
                                    me.thrustPercentage = limit;
                                }
                            }
                        }
                        
                    }
                }
            }
        }
    }
}