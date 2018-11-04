using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KSP___ActionGroupEngines
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class TLE_Settings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } } // column heading
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Throttle Limit Extended"; } }
        public override string DisplaySection { get { return "Throttle Limit Extended"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomParameterUI("Enable Throttle Limits",
            toolTip = "Enables the throttle limits on the engines, cannot be used with the Global Thrust Limit Window")]
        public bool throttleLimits = true;

        [GameParameters.CustomParameterUI("Enable Global Thrust Limit Window",
            toolTip = "Enables the button for the Global Thrust Limit Window, cannot be used with the Throttle Limits")]
        public bool thrustLimitWindow = false;

        [GameParameters.CustomParameterUI("Use alternative skin")]
        public bool useAlternativeSkin = false;

        bool initted = false;
        bool oldThrustLimit;
        bool oldThrottleLimit;

        [GameParameters.CustomFloatParameterUI("First preset button", minValue = 0, maxValue = 100f, stepCount = 101,
         toolTip = "Clicking the first preset button will immediately set the throttle limit to this value")]
        public float presetOne = 25f;
        [GameParameters.CustomFloatParameterUI("Second preset button", minValue = 0, maxValue = 100f,
                toolTip = "Clicking the second preset button will immediately set the throttle limit to this value")]
        public float presetTwo = 50f;
        [GameParameters.CustomFloatParameterUI("Third preset button", minValue = 0, maxValue = 100f, stepCount = 101,
                toolTip = "Clicking the third preset button will immediately set the throttle limit to this value")]
        public float presetThree = 75f;
        [GameParameters.CustomFloatParameterUI("Fourth preset button", minValue = 0, maxValue = 100f, stepCount = 101,
                toolTip = "Clicking the fourth preset button will immediately set the throttle limit to this value")]
        public float presetFour = 100f;



        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (initted)
            {

                if (oldThrottleLimit != throttleLimits)
                {
                    if (throttleLimits)
                    {
                        thrustLimitWindow = false;
                    }
                }
                else if (oldThrustLimit != thrustLimitWindow)
                {
                    if (thrustLimitWindow)
                        throttleLimits = false;
                }
            }
            initted = true;
            oldThrustLimit = thrustLimitWindow;
            oldThrottleLimit = throttleLimits;

            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
            //            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
