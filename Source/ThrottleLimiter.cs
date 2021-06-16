using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP___ActionGroupEngines.Main
{

    public class ThrottleLimiterModule : PartModule
    {
        enum SetThrustValues { none, init, off, decrease, increase, auto };

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
            SetMinThrust(SetThrustValues.init);

        }
#endif


        //void CheckThrust()
        void SetMinThrust(SetThrustValues stv)
        {
#if DEBUG
            Log.Info("ThrottleLimiterModule.CheckThrust:  part; " + part.partInfo.title + ",   stv: " + stv.ToString());
#endif
            foreach (PartModule m in this.part.Modules)
            {
                if (m.isEnabled || stv == SetThrustValues.init)
                {
                    if (m is ModuleEngines)
                    {
                        ModuleEngines engine = m as ModuleEngines;
                        if (engine == null || (stv != SetThrustValues.init && (!engine.isOperational || engine.engineType == EngineType.SolidBooster || engine.throttleLocked)))
                            continue;

                        Log.Info("SetMinThrust before change, engine.throttleMin: " + engine.throttleMin + ", engine.minFuelFlow: " + engine.minFuelFlow + ", engine.maxFuelFlow: " + engine.maxFuelFlow + ", engine.minThrust: " + engine.minThrust);

                        if (stv != SetThrustValues.off)
                        {
                            if (stv == SetThrustValues.decrease && engine.thrustPercentage <= 0.0f)
                                continue;

                            engine.throttleMin = minThrottle;
                            // following added for both GT & MJ
                            engine.minFuelFlow = minThrottle * engine.maxFuelFlow;
                            engine.minThrust = engine.maxThrust * minThrottle;
                        }
                        else
                        {
                            engine.throttleMin = 0;
                            engine.minFuelFlow = minThrottle * engine.maxFuelFlow;
                            engine.minThrust = 0;
                        }
                        Log.Info("SetMinThrust after change, engine.throttleMin: " + engine.throttleMin + ", engine.minFuelFlow: " + engine.minFuelFlow + ", engine.maxFuelFlow: " + engine.maxFuelFlow + ", engine.minThrust: " + engine.minThrust);
                    }
                    else if (m is ModuleEnginesFX)
                    {
                        ModuleEnginesFX engineFx = m as ModuleEnginesFX;
                        if (engineFx == null || (stv != SetThrustValues.init && (!engineFx.isOperational || engineFx.engineType == EngineType.SolidBooster || engineFx.throttleLocked)))
                            continue;
                        Log.Info("SetMinThrust before change, engineFx.throttleMin: " + engineFx.throttleMin + ", engineFx.minFuelFlow: " + engineFx.minFuelFlow + ", engineFx.maxFuelFlow: " + engineFx.maxFuelFlow + ", engineFx.minThrust: " + engineFx.minThrust);

                        if (stv != SetThrustValues.off)
                        {
                            if (stv == SetThrustValues.decrease && engineFx.thrustPercentage < -0.0f)
                                continue;
                            engineFx.throttleMin = minThrottle;
                            // following added for both GT & MJ
                            engineFx.minFuelFlow = minThrottle * engineFx.maxFuelFlow;
                            engineFx.minThrust = engineFx.maxThrust * minThrottle;
                        }
                        else
                        {
                            engineFx.throttleMin = 0;
                            engineFx.minFuelFlow = minThrottle * engineFx.maxFuelFlow;
                            engineFx.minThrust = 0;
                        }
                        Log.Info("SetMinThrust after change, engineFx.throttleMin: " + engineFx.throttleMin + ", engineFx.minFuelFlow: " + engineFx.minFuelFlow + ", engineFx.minThrust: " + ", engineFx.maxFuelFlow: " + engineFx.maxFuelFlow + engineFx.minThrust);
                    }

                }
            }
        }

        float lastThrottle = -1;
        void SetZeroThrottle()
        {
            lastThrottle = FlightInputHandler.state.mainThrottle;
            if (FlightInputHandler.state.mainThrottle == 0)
            {
                foreach (PartModule m in this.part.Modules)
                {
                    if (m.isEnabled)
                    {
                        if (m is ModuleEngines)
                        {
                            ModuleEngines engine = m as ModuleEngines;
                            if (engine == null || ((!engine.isOperational || engine.engineType == EngineType.SolidBooster || engine.throttleLocked)))
                                continue;

                            Log.Info("SetZeroThrottle before change, engine.throttleMin: " + engine.throttleMin + ", engine.minFuelFlow: " + engine.minFuelFlow + ", engine.maxFuelFlow: " + engine.maxFuelFlow + ", engine.minThrust: " + engine.minThrust);


                            {
                                engine.throttleMin = 0;
                                engine.minFuelFlow = 0;
                                engine.minThrust = 0;
                            }
                            Log.Info("SetZeroThrottle after change, engine.throttleMin: " + engine.throttleMin + ", engine.minFuelFlow: " + engine.minFuelFlow + ", engine.maxFuelFlow: " + engine.maxFuelFlow + ", engine.minThrust: " + engine.minThrust);
                        }
                        else if (m is ModuleEnginesFX)
                        {
                            ModuleEnginesFX engineFx = m as ModuleEnginesFX;
                            if (engineFx == null || ((!engineFx.isOperational || engineFx.engineType == EngineType.SolidBooster || engineFx.throttleLocked)))
                                continue;
                            Log.Info("SetZeroThrottle before change, engineFx.throttleMin: " + engineFx.throttleMin + ", engineFx.minFuelFlow: " + engineFx.minFuelFlow + ", engineFx.maxFuelFlow: " + engineFx.maxFuelFlow + ", engineFx.minThrust: " + engineFx.minThrust);


                            {
                                engineFx.throttleMin = 0;
                                engineFx.minFuelFlow = 0;
                                engineFx.minThrust = 0;
                            }
                            Log.Info("SetZeroThrottle after change, engineFx.throttleMin: " + engineFx.throttleMin + ", engineFx.minFuelFlow: " + engineFx.minFuelFlow + ", engineFx.minThrust: " + ", engineFx.maxFuelFlow: " + engineFx.maxFuelFlow + engineFx.minThrust);
                        }

                    }
                }
            }
            else
                SetMinThrust(SetThrustValues.init);
        }

        void LateUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel == this.vessel && HighLogic.CurrentGame.Parameters.CustomParams<TLE_Settings>().throttleLimits)
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
                if (FlightInputHandler.state.mainThrottle != lastThrottle)
                    SetZeroThrottle();
            }
        }
    }
}
