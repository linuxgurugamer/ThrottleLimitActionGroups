using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP___ActionGroupEngines.Main
{

    public class ThrottleLimiterModule : PartModule
    {
        enum SetThrustValues { none, off, decrease, increase, auto };

        [KSPField]
        public float minThrottle = 0;

        public override string GetModuleDisplayName()
        {
            return "Throttle Limit";
        }
        public override string GetInfo()
        {
            return "Minimum throttle: " + (100f * minThrottle).ToString("n0") + "%";         
        }

#if DEBUG
        void Start()
        {
            Log.Info("ThrottleLimiterModule:  part; " + part.partInfo.title + ",   minThrottle: " + minThrottle);
        }
#endif


        //void CheckThrust()
        void SetMinThrust(SetThrustValues stv)
        {
#if DEBUG
            Log.Info("ThrottleLimiterModule.CheckThrust:  part; " + part.partInfo.title + ",   stv: " + stv.ToString());
#endif
            foreach (PartModule m in this.part.Modules)
                if (m is ModuleEngines)
                {
                    ModuleEngines engine = m as ModuleEngines;
                    if (engine == null || !engine.isOperational || engine.engineType == EngineType.SolidBooster || engine.throttleLocked)
                        continue;

                    if (stv != SetThrustValues.off)
                    {
                        if (stv == SetThrustValues.decrease && engine.thrustPercentage <= 0.0f)
                            continue;

                        engine.throttleMin = minThrottle;
                    }
                    else
                        engine.throttleMin = 0;
                }
                else if (m is ModuleEnginesFX && m.isEnabled)
                {
                    ModuleEnginesFX engineFx = m as ModuleEnginesFX;
                    if (engineFx == null || !engineFx.isOperational || engineFx.engineType == EngineType.SolidBooster || engineFx.throttleLocked)
                        continue;

                    if (stv != SetThrustValues.off)
                    {
                        if (stv == SetThrustValues.decrease && engineFx.thrustPercentage <- 0.0f)
                            continue;
                        engineFx.throttleMin = minThrottle;
                    }
                    else
                        engineFx.throttleMin = 0;
                }
        }


        void Update()
        {
            if (FlightGlobals.ActiveVessel == this.vessel && HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().throttleLimits)
            {
                if (GameSettings.THROTTLE_UP.GetKey() ||
                     (Input.GetKey(KeyCode.LeftAlt) && GameSettings.WHEEL_THROTTLE_UP.GetKey())
                     )
                    SetMinThrust(SetThrustValues.increase);


                if (GameSettings.THROTTLE_DOWN.GetKey() ||
                     (Input.GetKey(KeyCode.LeftAlt) && GameSettings.WHEEL_THROTTLE_DOWN.GetKey())
                    )
                    SetMinThrust(SetThrustValues.decrease);

                if (GameSettings.THROTTLE_CUTOFF.GetKey())
                    SetMinThrust(SetThrustValues.off);

                if (GameSettings.THROTTLE_FULL.GetKey())
                    SetMinThrust(SetThrustValues.increase);
            }
        }
    }
}
