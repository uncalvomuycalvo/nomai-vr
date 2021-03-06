﻿using UnityEngine;

namespace NomaiVR
{
    internal class CameraMaskFix : NomaiVRModule<CameraMaskFix.Behaviour, CameraMaskFix.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private OWCamera _camera;
            private static float _farClipPlane = -1;
            public static int cullingMask = -1;
            private static Behaviour _instance;
            private bool _isPaused;

            internal void Start()
            {
                _instance = this;

                _camera = Locator.GetPlayerCamera();

                if (LoadManager.GetPreviousScene() == OWScene.TitleScreen && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
                {
                    CloseEyes();
                }
            }

            internal void Update()
            {
                _camera.postProcessingSettings.chromaticAberrationEnabled = false;
                _camera.postProcessingSettings.vignetteEnabled = false;

                if (InputHelper.IsUIInteractionMode() && !_isPaused)
                {
                    _isPaused = true;
                    cullingMask = Camera.main.cullingMask;
                    Camera.main.cullingMask = LayerMask.GetMask("UI");
                }
                if (!InputHelper.IsUIInteractionMode() && _isPaused)
                {
                    _isPaused = false;
                    Camera.main.cullingMask = cullingMask;
                }
            }

            private void CloseEyesDelayed()
            {
                Invoke(nameof(CloseEyes), 3);
            }

            private void CloseEyes()
            {
                cullingMask = Camera.main.cullingMask;
                _farClipPlane = Camera.main.farClipPlane;
                Camera.main.cullingMask = 1 << LayerMask.NameToLayer("VisibleToPlayer");
                Camera.main.farClipPlane = 5;
                Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
            }

            private void OpenEyes()
            {
                Camera.main.cullingMask = cullingMask;
                Camera.main.farClipPlane = _farClipPlane;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<Campfire>("StartFastForwarding", typeof(Patch), nameof(PostStartFastForwarding));

                    var openEyesMethod =
                        typeof(PlayerCameraEffectController)
                        .GetMethod("OpenEyes", new[] { typeof(float), typeof(AnimationCurve) });
                    NomaiVR.Helper.HarmonyHelper.AddPostfix(openEyesMethod, typeof(Patch), nameof(PostOpenEyes));

                    NomaiVR.Post<PlayerCameraEffectController>("CloseEyes", typeof(Patch), nameof(PostCloseEyes));
                }

                private static void PostStartFastForwarding()
                {
                    Locator.GetPlayerCamera().enabled = true;
                }

                private static void PostOpenEyes()
                {
                    _instance.OpenEyes();
                }

                private static void PostCloseEyes()
                {
                    _instance.CloseEyesDelayed();
                }
            }
        }
    }
}
