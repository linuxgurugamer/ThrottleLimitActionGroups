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

        [GameParameters.CustomParameterUI("Enable Throttle Limits")]
        public bool throttleLimits = true;

        [GameParameters.CustomParameterUI("Enable Global Thrust Limit Window")]
        public bool thrustLimitWindow = true;

        [GameParameters.CustomParameterUI("Use alternative skin")]
        public bool useAlternativeSkin = false;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
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
