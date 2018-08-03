using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSP___ActionGroupEngines.Main
{
    class EngineAGThrottleModule : PartModule
    {

        private enum ChangeModes
        {
            INCREASE = 0,
            DECREASE = 1,
            SET = 2
        }

        [KSPAction("Throttle: 100%")]
        public void throttle100(KSPActionParam p)
        {
            setLimit(ChangeModes.SET, 100f, p);
        }

        [KSPAction("Throttle: +10%")]
        public void throttleP10(KSPActionParam p)
        {
            setLimit(ChangeModes.INCREASE, 10f, p);
        }

        [KSPAction("Throttle: +5%")]
        public void throttleP5(KSPActionParam p)
        {
            setLimit(ChangeModes.INCREASE, 5f, p);
        }

        [KSPAction("Throttle: +1%")]
        public void throttleP1(KSPActionParam p)
        {
            setLimit(ChangeModes.INCREASE, 1f, p);
        }

        [KSPAction("Throttle: 50%")]
        public void throttle50(KSPActionParam p)
        {
            setLimit(ChangeModes.SET, 50f, p);
        }

        [KSPAction("Throttle: -10%")]
        public void throttleM10(KSPActionParam p)
        {
            setLimit(ChangeModes.DECREASE, 10f, p);
        }

        [KSPAction("Throttle: -5%")]
        public void throttleM5(KSPActionParam p)
        {
            setLimit(ChangeModes.DECREASE, 5f, p);
        }

        [KSPAction("Throttle: -1%")]
        public void throttleM1(KSPActionParam p)
        {
            setLimit(ChangeModes.DECREASE, 1f, p);
        }

        [KSPAction("Throttle: 0%")]
        public void throttle0(KSPActionParam p)
        {
            setLimit(ChangeModes.SET, 0f, p);
        }

        public void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().throttleLimits)
            {
                Actions["throttle100"].active = false;
                Actions["throttleP10"].active = false;
                Actions["throttleP5"].active = false;
                Actions["throttleP1"].active = false;
                Actions["throttle50"].active = false;
                Actions["throttleM10"].active = false;
                Actions["throttleM5"].active = false;
                Actions["throttle0"].active = false;
            }
        }

        private void setLimit(ChangeModes c, float f, KSPActionParam p)
        {
            foreach (PartModule m in this.part.Modules)
                if (m is ModuleEngines)
                {
                    ModuleEngines me = (ModuleEngines)m;
                    if (!me.isOperational)
                        continue;
                    if (c == ChangeModes.DECREASE && me.thrustPercentage == 0f || c == ChangeModes.INCREASE && me.thrustPercentage == 100f) // 1.0.1: Fix for engines going >100 or <0
                        continue;

                    if (c == ChangeModes.DECREASE)
                        me.thrustPercentage -= f;
                    else if (c == ChangeModes.INCREASE)
                        me.thrustPercentage += f;
                    else
                        me.thrustPercentage = f;
                }
                else if (m is ModuleEnginesFX && m.isEnabled) // Squad, y u have separate module for NASA engines? :c
                {
                    ModuleEnginesFX me = (ModuleEnginesFX)m;
                    if (!me.isOperational)
                        continue;
                    if (c == ChangeModes.DECREASE && me.thrustPercentage == 0f || c == ChangeModes.INCREASE && me.thrustPercentage == 100f) // 1.0.1: Fix for engines going >100 or <0
                        continue;

                    if (c == ChangeModes.DECREASE)
                        me.thrustPercentage -= f;
                    else if (c == ChangeModes.INCREASE)
                        me.thrustPercentage += f;
                    else
                        me.thrustPercentage = f;
                }

        }

    }
}
