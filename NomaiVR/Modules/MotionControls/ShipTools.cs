﻿using UnityEngine;

namespace NomaiVR {
    class ShipTools: MonoBehaviour {
        bool _wasHoldingInteract;
        ReferenceFrameTracker _referenceFrameTracker;

        void Awake () {
            NomaiVR.Log("Start Ship Tools");

            _referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
        }

        void Update () {
            if (_referenceFrameTracker.GetReferenceFrame() == null && _referenceFrameTracker.GetPossibleReferenceFrame() == null) {
                return;
            }
            if (OWInput.IsNewlyHeld(InputLibrary.interact)) {
                ControllerInput.SimulateInput(XboxAxis.dPadY, 1);
                _wasHoldingInteract = true;
            }
            if (OWInput.IsNewlyReleased(InputLibrary.interact)) {
                if (_wasHoldingInteract) {
                    ControllerInput.SimulateInput(XboxAxis.dPadY, 0);
                    _wasHoldingInteract = false;
                } else {
                    ControllerInput.SimulateInput(XboxButton.LeftStickClick, 1);
                    Invoke("ResetLeftStickClick", 0.5f);
                }
            }
        }

        void ResetLeftStickClick () {
            ControllerInput.SimulateInput(XboxButton.LeftStickClick, 0);
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ShipBody>("Start", typeof(Patches), nameof(Patches.ShipStart));
                NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patches), nameof(Patches.PreFindReferenceFrameInLineOfSight));
                NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patches), nameof(Patches.PostFindReferenceFrameInLineOfSight));
            }

            static void ShipStart (ShipBody __instance) {
                var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                var probe = cockpitUI.Find("ProbeScreen/ProbeScreenPivot/ProbeScreen").gameObject.AddComponent<ButtonInteraction>();
                probe.button = XboxButton.RightBumper;
                probe.text = UITextType.ScoutModePrompt;

                var signalscope = cockpitUI.Find("SignalScreen/SignalScreenPivot/SignalScopeScreenFrame_geo").gameObject.AddComponent<ButtonInteraction>();
                signalscope.button = XboxButton.Y;
                signalscope.text = UITextType.SignalscopePrompt;

                var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                var landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ButtonInteraction>();
                landingCam.button = XboxButton.DPadDown;
                landingCam.text = UITextType.ShipLandingPrompt;
            }


            static Vector3 _cameraPosition;
            static Quaternion _cameraRotation;

            static void PreFindReferenceFrameInLineOfSight (
                OWCamera ____activeCam,
                OWCamera ____landingCam,
                bool ____isLandingView
            ) {
                var camera = ____isLandingView ? ____landingCam : ____activeCam;
                _cameraPosition = camera.transform.position;
                _cameraRotation = camera.transform.rotation;

                camera.transform.position = LaserPointer.Laser.position;
                camera.transform.rotation = LaserPointer.Laser.rotation;
            }

            static void PostFindReferenceFrameInLineOfSight (
                OWCamera ____activeCam,
                OWCamera ____landingCam,
                bool ____isLandingView
            ) {
                var camera = ____isLandingView ? ____landingCam : ____activeCam;

                camera.transform.position = _cameraPosition;
                camera.transform.rotation = _cameraRotation;
            }
        }
    }
}
