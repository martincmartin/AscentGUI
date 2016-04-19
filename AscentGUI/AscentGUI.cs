using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AscentGUI
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AscentGUI : MonoBehaviour
    {
        public KeyCode key = KeyCode.J;
        public bool useModifier = true;
        public bool winterOwlModeOff = true;
        public static bool guiEnabled = false;
        public static Rect windowPos = new Rect(200, 100, 0, 0);

        public void Start()
        {
            enabled = true;
            // Some day we might add settings, like AeroGUI
            // ConfigNode settings = GameDatabase.Instance.GetConfigNodes("AEROGUI")[0];
        }

        public void OnGUI()
        {
            print("In OnGUI!");

            if (guiEnabled)
            {
                windowPos = GUILayout.Window("AscentGUI".GetHashCode(), windowPos, DrawWindow, "AscentGUI");
            }
        }

        public void Update()
        {
            if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(key))
            {
                guiEnabled = !guiEnabled;
            }
        }

        public double SpeedAtApoasisFromOrbitalEnergy(double specificOrbitalEnergy, double ApA, CelestialBody body)
        {
            double specificKineticEnergyAtAp = specificOrbitalEnergy - -body.gravParameter / (ApA + body.Radius);
            return Math.Sqrt(2 * specificKineticEnergyAtAp);
        }

        public double SpeedAtApoapsisFromApAPeA(double ApA, double PeA, CelestialBody body)
        {
            return SpeedAtApoasisFromOrbitalEnergy(- body.gravParameter / (ApA + PeA + 2 * body.Radius), ApA, body);
        }

        public double SpeedAtApoapsisFromCurrentSpeedAndAltitude(double speed, double altitude, double ApA, CelestialBody body)
        {
            return SpeedAtApoasisFromOrbitalEnergy(speed * speed / 2 - body.gravParameter / (altitude + body.Radius), ApA, body);
        }

        // cosRV is the cosine of the angle between the current position relative to the body we're orbiting,
        // and our velocity.  Because we're buring prograde, the direction of V won't change, so this is an input.
        // Can be calculated as orbit.h.magnitude / orbit.vel.magnitude / orbit.radius;
        //
        // Could also take cosRV * R, which would save this routine a multiply and probably
        // the caller three multiplies, a sqrt and a divide.  Performance gain is probably not worth it.
        public double SpeedToReachApoapsis(double desiredApA, double altitude, double cosRV, CelestialBody body)
        {
            double r = altitude + body.Radius;
            double ra = desiredApA + body.Radius;
            double vSqr = 2 * body.gravParameter * ra * (ra - r) / (r * (ra * ra - cosRV * cosRV * r * r));
            return Math.Sqrt(vSqr);
        }

        /*
        public void ApAAndPeA(double speed, double altitude, double cosRV, CelestialBody body, out double ApA, out double PeA)
        {
            double r = altitude + body.Radius;
            double h = r * speed * cosRV;
            double E = speed * speed / 2 - body.gravParameter / r;
//            double p = h * h / body.gravParameter;
            double e = Math.Sqrt(1 + 2 * E * h * h / (body.gravParameter * body.gravParameter));
            double a = -body.gravParameter / (2 * E);
            double PeR = a * (1 - e);
            double ApR = a * (1 + e);
        }
        */

        public void AddLabel(string text, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, style);
            GUILayout.EndHorizontal();
        }

        public void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();

            // Enable closing of the window with "x"
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(5, 5, 3, 0);
            buttonStyle.margin = new RectOffset(1, 1, 1, 1);
            buttonStyle.stretchWidth = false;
            buttonStyle.stretchHeight = false;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.wordWrap = false;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X", buttonStyle))
            {
                guiEnabled = false;
            }
            GUILayout.EndHorizontal();

            Orbit orbit = FlightGlobals.ActiveVessel.orbit;

            double desiredApA = orbit.referenceBody.atmosphereDepth * 1.1;
            double desiredPeA = desiredApA;
            AddLabel("Current Ap: " + orbit.ApA.ToString("N0") + " / " + desiredApA.ToString("N0") + " m", labelStyle);
            double cosRV = orbit.h.magnitude / orbit.vel.magnitude / orbit.radius;
            AddLabel("Angle between prograde and vertical: " + Math.Acos(cosRV).ToString("N3"), labelStyle);
            AddLabel("Speed current: " + orbit.vel.magnitude.ToString("N3")+" m/s", labelStyle);
            double speedForAp;
            if (orbit.ApA >= desiredApA)
            {
                desiredApA = orbit.ApA;
                speedForAp = orbit.vel.magnitude;
            } else {
                speedForAp = SpeedToReachApoapsis(desiredApA, orbit.altitude, cosRV, orbit.referenceBody);
            }
            AddLabel("Speed to reach desired Ap: " + speedForAp.ToString("N3")+" m/s", labelStyle);
            double deltaVToAp = speedForAp - orbit.vel.magnitude;
            AddLabel("Delta V to Ap: " + deltaVToAp.ToString("N3"), labelStyle);            

            AddLabel("-----------------------------", labelStyle);

            // Energy at surface with no velocity.
//            double surfaceEnergyDensity = - orbit.referenceBody.gravParameter / orbit.referenceBody.Radius;
//            double specificGravityEnergy = -orbit.referenceBody.gravParameter / orbit.radius - surfaceEnergyDensity;
//            double specificKineticEnergy = 0.5 * orbit.vel.sqrMagnitude;
            double speedAtDesiredApA = SpeedAtApoapsisFromCurrentSpeedAndAltitude(speedForAp, orbit.altitude, desiredApA, orbit.referenceBody);
//            AddLabel("Altitude: " + orbit.altitude.ToString("N0") + " m", labelStyle);
//            AddLabel("Specific gravity energy: " + (specificGravityEnergy/1000).ToString("N3") +" k", labelStyle);
//            AddLabel("Specific kinetic energy: " + (specificKineticEnergy/1000).ToString("N3")+" k", labelStyle);
//            AddLabel("Specific orbital energy: " + ((specificGravityEnergy + specificKineticEnergy)/1000).ToString("N3")+" k", labelStyle);
            AddLabel("Periapsis: " + orbit.PeA.ToString("N0") + "/" + desiredPeA.ToString("N0") + " m", labelStyle);
            AddLabel("Desired Ap : " + desiredApA.ToString("N3") + " m", labelStyle);
            AddLabel("Speed at desired Ap : " + speedAtDesiredApA.ToString("N3") + " m/s", labelStyle);
            //            AddLabel("Speed at Ap from Orbit: " + orbit.getOrbitalSpeedAtDistance(orbit.ApR).ToString("N3"), labelStyle);
            double desiredSpeedAtDesiredApA = SpeedAtApoapsisFromApAPeA(desiredApA, orbit.referenceBody.atmosphereDepth * 1.1, orbit.referenceBody);
            AddLabel("Desired Speed at desired Ap: " + desiredSpeedAtDesiredApA.ToString("N3"), labelStyle);
            double deltaVToRaisePe = Math.Max(0, desiredSpeedAtDesiredApA - speedAtDesiredApA);
            AddLabel("Delta V to raise Pe: " + deltaVToRaisePe.ToString("N3"), labelStyle);
            AddLabel("-----------------------------", labelStyle);
            AddLabel("Total Delta V: " + (Math.Max(0, deltaVToRaisePe) + Math.Max(0, deltaVToAp)).ToString("N3"), labelStyle);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
