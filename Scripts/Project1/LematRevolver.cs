using FistVR;
using UnityEngine;
using System.Collections;


namespace PrimeVrScripts
{
    public class LematRevolver : SingleActionRevolver
    {
        [Header("Cap and Ball Revolver Config")]
        public Transform ramRodLever;
        public LematLever lematLever;
        public Vector3 lowerLimit;
        public Vector3 upperLimit;

        public Vector3 touchingRot;
        public float wiggleRoom = 5f;

        public int numberOfChambersBackwardsToRam;

        private LematCylinder CapCylinder;

        private int lastChamber = -1;

        private bool isShotgunActivated = false;

        private bool isRamRodExtended = false;

        public Transform hammerSwitch;
        public float hammerSwitchUnflipped = 0;
        public float hammerSwitchFlipped = 0;
        public float hammerUncockedFlipped = 0;
        public float hammerUncockedUnflipped = 0;

        public Transform shotgunMuzzle;
        public Transform regularMuzzle;
        public Transform overrideMuzzle;

        public FVRFireArmRecoilProfile shotgunRecoilProfile;

#if!(MEATKIT ||UNITY_EDITOR||UNITY_5)
        public override void Start()
        {
            base.Start();
            Hook();
            CapCylinder = base.Cylinder as LematCylinder;

            numberOfChambersBackwardsToRam = Mathf.Abs(numberOfChambersBackwardsToRam);
        }

        public override void OnDestroy()
        {
            Unhook();
            base.OnDestroy();
        }
        private void Unhook()
        {
            On.FistVR.SingleActionRevolver.Fire -= SingleActionRevolver_Fire;
            //On.FistVR.SingleActionRevolver.EjectPrevCylinder -= SingleActionRevolver_EjectPrevCylinder;
            On.FistVR.SingleActionRevolver.UpdateCylinderRot -= SingleActionRevolver_UpdateCylinderRot;
            On.FistVR.SingleActionRevolver.AdvanceCylinder -= SingleActionRevolver_AdvanceCylinder;
        }

        private void Hook()
        {
            On.FistVR.SingleActionRevolver.Fire += SingleActionRevolver_Fire;
            //On.FistVR.SingleActionRevolver.EjectPrevCylinder += SingleActionRevolver_EjectPrevCylinder;
            On.FistVR.SingleActionRevolver.UpdateCylinderRot += SingleActionRevolver_UpdateCylinderRot;
            On.FistVR.SingleActionRevolver.AdvanceCylinder += SingleActionRevolver_AdvanceCylinder;
            FVRViveHand hand = this.m_hand;
        }

        public int RammingChamber
        {
            get
            {
                int num = this.CurChamber - numberOfChambersBackwardsToRam;
                if (num < 0)
                {
                    return this.Cylinder.NumChambers + num;
                }
                return num;
            }
        }

        public int PrevChamber3
        {
            get
            {
                int num = this.CurChamber - 3;
                if (num < 0)
                {
                    return this.Cylinder.NumChambers + num;
                }
                return num;
            }
        }
        public override void FVRUpdate()
        {
            base.FVRUpdate();


            float lerp = ExtendingVector3.InverseLerp(touchingRot, upperLimit, ramRodLever.localEulerAngles);
            if (CapCylinder.Chambers[RammingChamber] != null)
            {
                if (CapCylinder.Chambers[RammingChamber].IsFull && lerp > 0f)
                {
                    CapCylinder.RamChamber(RammingChamber, lerp);
                }
            }


            if (this.m_hand != null)
            {
                if (this.m_hammerCockLerp == 1 && this.m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    StartCoroutine(activateShotgun());
                }
            }

        }

        IEnumerator activateShotgun()
        {
            this.isShotgunActivated = !this.isShotgunActivated;
            this.PlayAudioEvent(FirearmAudioEventType.Safety);
            if (this.isShotgunActivated)
            {
                this.hammerSwitch.localEulerAngles = new Vector3(this.hammerSwitchFlipped, 0f, 0f);
                this.Hammer_Rot_Uncocked = hammerUncockedFlipped;
            }
            else
            {
                this.hammerSwitch.localEulerAngles = new Vector3(this.hammerSwitchUnflipped, 0f, 0f);
                this.Hammer_Rot_Uncocked = hammerUncockedUnflipped;
            }
            yield return new WaitForSeconds(1f);
        }

        private void SingleActionRevolver_AdvanceCylinder(On.FistVR.SingleActionRevolver.orig_AdvanceCylinder orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                if (!lematLever.rodClosed || (!CapCylinder.ChamberRammed(RammingChamber) && CapCylinder.Chambers[RammingChamber].IsFull))
                {
                    return;
                }

                if (lastChamber == this.CurChamber)
                {
                    lastChamber--;
                }
                else
                {
                    this.CurChamber++;
                    lastChamber = this.CurChamber;
                }

                this.PlayAudioEvent(FirearmAudioEventType.FireSelector, 1f);
            }
            else orig(self);
        }

        private void SingleActionRevolver_UpdateCylinderRot(On.FistVR.SingleActionRevolver.orig_UpdateCylinderRot orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                if (this.m_isStateToggled)
                {
                    int num = this.PrevChamber;
                    if (this.IsAccessTwoChambersBack)
                        num = this.PrevChamber2;
                    for (int index = 0; index < this.CapCylinder.Chambers.Length - 1; ++index)
                    {
                        this.CapCylinder.Chambers[index].IsAccessible = index == num;
                        this.CapCylinder.capNipples[index].IsAccessible = index == num;

                        if (lastChamber == this.CurChamber)
                        {
                            if (!this.IsAccessTwoChambersBack)
                            {
                                this.CapCylinder.Chambers[index].IsAccessible = index == this.PrevChamber2;
                                this.CapCylinder.capNipples[index].IsAccessible = index == this.PrevChamber2;
                            }
                            else
                            {
                                this.CapCylinder.Chambers[index].IsAccessible = index == this.PrevChamber3;
                                this.CapCylinder.capNipples[index].IsAccessible = index == this.PrevChamber3;
                            }
                        }
                    }
                    if (this.DoesHalfCockHalfRotCylinder)
                    {
                        int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                        this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0.65f);

                        if (lastChamber == this.CurChamber) this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0f);
                    }
                    else
                    {
                        int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                        this.CapCylinder.transform.localRotation = this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber);
                        if (lastChamber == this.CurChamber)
                        {
                            this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0.25f);
                        }
                    }
                    if (this.DoesCylinderTranslateForward)
                        this.CapCylinder.transform.localPosition = this.CylinderBackPos;

                }
                else
                {
                    for (int index = 0; index < this.CapCylinder.Chambers.Length - 1; ++index)
                    {
                        this.CapCylinder.Chambers[index].IsAccessible = false;
                        this.CapCylinder.capNipples[index].IsAccessible = false;
                    }
                    this.m_tarChamberLerp = !this.m_isHammerCocking ? 0.0f : this.m_hammerCockLerp;
                    this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);
                    int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                    this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), this.m_curChamberLerp);

                    if (this.DoesCylinderTranslateForward)
                        this.CapCylinder.transform.localPosition = Vector3.Lerp(this.CylinderBackPos, this.CylinderFrontPos, this.m_hammerCockLerp);


                    return;
                }
            }
            else orig(self);
        }

        private void SingleActionRevolver_EjectPrevCylinder(On.FistVR.SingleActionRevolver.orig_EjectPrevCylinder orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                /*if (!this.m_isStateToggled)
                    return;
                int index = this.PrevChamber;
                if (this.IsAccessTwoChambersBack)
                    index = this.PrevChamber2;
                FVRFireArmChamber chamber = this.CapCylinder.Chambers[index];
                if (chamber.IsFull)
                    this.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound);
                chamber.EjectRound(chamber.transform.position + chamber.transform.forward * (1f / 400f), chamber.transform.forward, Vector3.zero);*/
            }
            else
            {
                orig(self);
            }
        }

        private void SingleActionRevolver_Fire(On.FistVR.SingleActionRevolver.orig_Fire orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                this.PlayAudioEvent(FirearmAudioEventType.HammerHit);
                bool capFired = this.isShotgunActivated ? this.CapCylinder.capNipples[9].Fire() : this.CapCylinder.capNipples[this.CurChamber].Fire();

                if (capFired)
                {
                        this.PlayAudioEvent(FirearmAudioEventType.Shots_LowPressure);
                }
                if (this.isShotgunActivated)
                {
                    if (!capFired || !this.CapCylinder.Chambers[9].Fire())
                        return;
                }
                else
                {
                    if (!capFired || !this.CapCylinder.ChamberRammed(this.CurChamber) || !this.CapCylinder.Chambers[this.CurChamber].Fire())
                        return;
                }
                FVRFireArmChamber chamber;
                Transform muzzle;
                if (this.isShotgunActivated)
                {
                    chamber = this.CapCylinder.Chambers[9];
                    muzzle = this.shotgunMuzzle;
                    this.overrideMuzzle.position = this.shotgunMuzzle.position;
                    this.MuzzleEffects[0].OverridePoint = this.shotgunMuzzle;
                    this.GasOutEffects[2].MaxGasRate = 12;
                    this.GasOutEffects[1].MaxGasRate = 0;
                    this.m_isSuppressed = true;
                    this.HasActiveShoulderStock = true;

                }
                else
                {
                    chamber = this.CapCylinder.Chambers[this.CurChamber];
                    muzzle = this.regularMuzzle;
                    this.overrideMuzzle.position = this.regularMuzzle.position;
                    this.MuzzleEffects[0].OverridePoint = null;
                    this.GasOutEffects[2].MaxGasRate = 0;
                    this.GasOutEffects[1].MaxGasRate = 12;
                    this.m_isSuppressed = false;
                    this.HasActiveShoulderStock = false;

                }
                this.Fire(chamber, muzzle, true);
                this.FireMuzzleSmoke();
                if (this.isShotgunActivated)
                {
                    this.Recoil(this.IsTwoHandStabilized(), (Object)this.AltGrip != (Object)null, this.IsShoulderStabilized(), shotgunRecoilProfile);
                }
                else
                {
                    this.Recoil(this.IsTwoHandStabilized(), (Object)this.AltGrip != (Object)null, this.IsShoulderStabilized());
                }
                this.PlayAudioGunShot(chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment());

                if (GM.CurrentSceneSettings.IsAmmoInfinite && GM.CurrentPlayerBody.IsInfiniteAmmo)
                {
                    chamber.IsSpent = false;
                    this.CapCylinder.capNipples[this.CurChamber].IsSpent = false;

                    chamber.UpdateProxyDisplay();
                }
                else
                {
                    chamber.SetRound(null);
                    if (!this.isShotgunActivated)
                        this.CapCylinder.ChamberRammed(this.CurChamber, true, false);
                }
            }
            else orig(self);
        }
#endif
    }
#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
    public static class ExtendingVector3
    {
        public static bool IsGreaterOrEqual(this Vector3 local, Vector3 other)
        {
            if (local.x >= other.x && local.y >= other.y && local.z >= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsLesserOrEqual(this Vector3 local, Vector3 other)
        {
            if (local.x <= other.x && local.y <= other.y && local.z <= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            /*
            float lerpx = Mathf.InverseLerp(a.x, b.x, value.x);
            float lerpy = Mathf.InverseLerp(a.y, b.y, value.y);
            float lerpz = Mathf.InverseLerp(a.z, b.z, value.z);

            Vector3 lerp = new Vector3(lerpx, lerpy, lerpz);
            return lerp.magnitude;
            */

            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
        }
    }
#endif
}
