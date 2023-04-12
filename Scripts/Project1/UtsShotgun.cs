using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class UtsShotgun : TubeFedShotgun
    {
        [Header("UTS-15 Specifics")]
        public int tubeSwitchPosition = 0;
        public UtsMagazine leftTubeMagazine;
        public UtsMagazine rightTubeMagazine;
        public int flashlightLaserPosition = 0;
        [Header("UTS-15 Flashlight")]
        public GameObject FlashLightParts;
        public AudioEvent FlashLightLaser_ToggleClip;
        public Light FlashlightLight;
        [Header("UTS-15 Laser")]
        private RaycastHit m_hit;
        public GameObject BeamEffect;
        public GameObject BeamHitPoint;
        public Transform Aperture;
        public GameObject LaserGlow;
        public bool isLaserOn = false;
        public LayerMask LM;
        public int gatesOpen = 0;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            base.Awake();
            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        private void Unhook()
        {
            On.FistVR.TubeFedShotgun.EjectExtractedRound -= this.TubeFedShotgun_EjectExtractedRound;
            On.FistVR.TubeFedShotgun.UpdateInputAndAnimate -= this.TubeFedShotgun_UpdateInputAndAnimate;
        }

        private void Hook()
        {
            On.FistVR.TubeFedShotgun.EjectExtractedRound += this.TubeFedShotgun_EjectExtractedRound;
            On.FistVR.TubeFedShotgun.UpdateInputAndAnimate += this.TubeFedShotgun_UpdateInputAndAnimate;
        }

        private void TubeFedShotgun_UpdateInputAndAnimate(On.FistVR.TubeFedShotgun.orig_UpdateInputAndAnimate orig, TubeFedShotgun self, FVRViveHand hand)
        {
            if (self == this)
            {
                this.IsSlideReleaseButtonHeld = false;
                if (this.IsAltHeld)
                    return;
                this.m_triggerFloat = !this.m_hasTriggeredUpSinceBegin ? 0.0f : hand.Input.TriggerFloat;
                if (!this.m_hasTriggerReset && (double)this.m_triggerFloat <= (double)this.TriggerResetThreshold)
                {
                    this.m_hasTriggerReset = true;
                    this.PlayAudioEvent(FirearmAudioEventType.TriggerReset);
                }
                Vector2 touchpadAxes = hand.Input.TouchpadAxes;
                if (hand.IsInStreamlinedMode)
                {
                    if (hand.Input.BYButtonDown)
                        this.ToggleSafety();
                    if (hand.Input.AXButtonPressed)
                    {
                        this.IsSlideReleaseButtonHeld = true;
                        if (this.HasHandle && this.Mode == TubeFedShotgun.ShotgunMode.PumpMode)
                            this.Handle.UnlockHandle();
                    }
                }
                else
                {
                    if (hand.Input.TouchpadDown && (double)touchpadAxes.magnitude > 0.20000000298023224)
                    {
                        if ((double)Vector2.Angle(touchpadAxes, Vector2.left) <= 45.0)
                            this.ToggleSafety();
                        else if ((double)Vector2.Angle(touchpadAxes, Vector2.up) <= 45.0 && this.Mode == TubeFedShotgun.ShotgunMode.Automatic)
                            this.Bolt.ReleaseBolt();
                    }
                    if (hand.Input.TouchpadPressed && (double)touchpadAxes.magnitude > 0.20000000298023224 && (double)Vector2.Angle(touchpadAxes, Vector2.up) <= 45.0)
                    {
                        this.IsSlideReleaseButtonHeld = true;
                        if (this.HasHandle && this.Mode == TubeFedShotgun.ShotgunMode.PumpMode)
                            this.Handle.UnlockHandle();
                    }
                }
                if ((double)this.m_triggerFloat < (double)this.TriggerBreakThreshold || !this.m_isHammerCocked || this.m_isSafetyEngaged || this.gatesOpen > 0)
                    return;
                if (this.m_hasTriggerReset || this.UsesSlamFireTrigger)
                    this.ReleaseHammer();
                this.m_hasTriggerReset = false;
            } else
            {
                orig(self, hand);
            }
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            Vector2 touchpadAxes = hand.Input.TouchpadAxes;
            if (hand.IsInStreamlinedMode)
            {

            }
            else
            {
                if (hand.Input.TouchpadDown && (double)touchpadAxes.magnitude > 0.20000000298023224)
                {
                    if ((double)Vector2.Angle(touchpadAxes, Vector2.right) <= 45.0)
                        this.ToggleFlashlightLaser();
                    else if ((double)Vector2.Angle(touchpadAxes, Vector2.up) <= 45.0 && this.Mode == TubeFedShotgun.ShotgunMode.Automatic)
                        this.Bolt.ReleaseBolt();
                }
            }

        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            if (!this.isLaserOn)
                return;
            Vector3 vector3 = this.Aperture.position + this.Aperture.forward * 1000f;
            float num1 = 1000f;
            if (Physics.Raycast(this.Aperture.position, this.Aperture.forward, out this.m_hit, 1000f, (int)this.LM, QueryTriggerInteraction.Ignore))
            {
                vector3 = this.m_hit.point;
                num1 = this.m_hit.distance;
            }
            float num2 = Mathf.Lerp(0.01f, 0.2f, num1 * 0.01f);
            this.BeamHitPoint.transform.position = vector3;
            this.BeamHitPoint.transform.localScale = new Vector3(num2, num2, num2);
        }

        public void ToggleFlashlightLaser() {
            SM.PlayCoreSound(FVRPooledAudioType.GenericClose, this.FlashLightLaser_ToggleClip, this.transform.position);
            flashlightLaserPosition++;
            if (flashlightLaserPosition == 4)
                flashlightLaserPosition = 0;
            this.activateLaser(flashlightLaserPosition == 1 || flashlightLaserPosition == 3);
            this.activateFlashlight(flashlightLaserPosition == 2 || flashlightLaserPosition == 3);
        }

        private void activateLaser(bool active)
        {
            if (active)
            {
                this.BeamHitPoint.SetActive(true);
                this.BeamEffect.SetActive(true);
                this.LaserGlow.SetActive(true);
                this.isLaserOn = true;
            } else
            {
                this.BeamHitPoint.SetActive(false);
                this.BeamEffect.SetActive(false);
                this.LaserGlow.SetActive(false);
                this.isLaserOn = false;
            }
        }
        
        private void activateFlashlight(bool active)
        {
            if (active)
            {
                this.FlashLightParts.SetActive(true);
                this.FlashlightLight.intensity = !GM.CurrentSceneSettings.IsSceneLowLight ? 0.75f : 2.5f;
            } else
            {
                this.FlashLightParts.SetActive(false);
            }
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);
            string key = "TubeSelector";
            string value = string.Empty;
            if (f.ContainsKey(key))
            {
                value = f[key];
                this.tubeSwitchPosition = int.Parse(value);
            }
            if (leftTubeMagazine != null && leftTubeMagazine is UtsMagazine leftMag)
            {
                key = "LeftMagazine";
                value = string.Empty;

                string[] roundClassStrings;
                if (f.ContainsKey(key))
                {
                    value = f[key];

                    roundClassStrings = value.Split(';');
                    leftMag.loadingGate.isOpen = false;
                    leftMag.loadingGate.SimpleInteraction(this.m_hand);
                    foreach (string roundClassString in roundClassStrings)
                    {
                        leftMag.AddRound((FireArmRoundClass)Enum.Parse(typeof(FireArmRoundClass), roundClassString), false, false);
                    }
                    leftMag.loadingGate.SimpleInteraction(this.m_hand);

                    leftMag.UpdateBulletDisplay();
                }
            }
            if (rightTubeMagazine != null && rightTubeMagazine is UtsMagazine rightMag)
            {
                key = "RightMagazine";
                value = string.Empty;

                string[] roundClassStrings;
                if (f.ContainsKey(key))
                {
                    value = f[key];

                    roundClassStrings = value.Split(';');
                    rightMag.loadingGate.isOpen = false;
                    rightMag.loadingGate.SimpleInteraction(this.m_hand);
                    foreach (string roundClassString in roundClassStrings)
                    {
                        rightMag.AddRound((FireArmRoundClass)Enum.Parse(typeof(FireArmRoundClass), roundClassString), false, false);
                    }
                    rightMag.loadingGate.SimpleInteraction(this.m_hand);

                    rightMag.UpdateBulletDisplay();
                }
            }
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();
            string key = "TubeSelector";
            string value = this.tubeSwitchPosition.ToString();
            flagDic.Add(key, value);
            if (leftTubeMagazine != null && leftTubeMagazine is UtsMagazine leftMag)
            {
                key = "LeftMagazine";
                value = string.Empty;

                if (leftMag.HasARound())
                {
                    value += leftMag.LoadedRounds[0].LR_Class.ToString();

                    for (int i = 1; i < leftMag.m_numRounds; i++)
                    {
                        value += ";" + leftMag.LoadedRounds[i].LR_Class.ToString();
                    }

                    flagDic.Add(key, value);
                }
            }
            if (rightTubeMagazine != null && rightTubeMagazine is UtsMagazine rightMag)
            {
                key = "RightMagazine";
                value = string.Empty;

                if (rightMag.HasARound())
                {
                    value += rightMag.LoadedRounds[0].LR_Class.ToString();

                    for (int i = 1; i < rightMag.m_numRounds; i++)
                    {
                        value += ";" + rightMag.LoadedRounds[i].LR_Class.ToString();
                    }

                    flagDic.Add(key, value);
                }
            }
            return flagDic;
        }

        private void TubeFedShotgun_EjectExtractedRound(On.FistVR.TubeFedShotgun.orig_EjectExtractedRound orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                if (!this.leftTubeMagazine.IsFull())
                {
                    this.leftTubeMagazine.DisplayRoundsOrigin.transform.localPosition = this.leftTubeMagazine.DisplayRoundsOriginDePressedTransform;
                }
                if (!this.rightTubeMagazine.IsFull())
                {
                    this.rightTubeMagazine.DisplayRoundsOrigin.transform.localPosition = this.rightTubeMagazine.DisplayRoundsOriginDePressedTransform;
                }

                if (this.tubeSwitchPosition == 1)
                {
                    if(leftTubeMagazine == this.Magazine)
                    {
                        if (rightTubeMagazine.HasARound())
                        {
                            this.Magazine = rightTubeMagazine;
                        }
                    }
                    else
                    {
                        if (leftTubeMagazine.HasARound())
                        {
                            this.Magazine = leftTubeMagazine;
                        }
                    }
                }
                this.Magazine.IsDropInLoadable = true;
                this.leftTubeMagazine.IsDropInLoadable = true;
                this.rightTubeMagazine.IsDropInLoadable = true;
            }
        }
#endif
    }
}
