using FistVR;
using UnityEngine;
using System.Collections.Generic;


namespace PrimeVrScripts
{
    public class StreetSweeper : FVRFireArm
    {
        [Header("Single Action Revolver")]
        public bool AllowsSuppressor;
        public Transform Hammer;
        public Transform LoadingGate;
        public Transform Trigger;
        public Transform EjectorRod;
        public SingleActionRevolverCylinder Cylinder;
        public Transform HammerFanDir;
        private int m_curChamber;
        private float m_curChamberLerp;
        public float m_tarChamberLerp;
        [Header("Component Movement Params")]
        public float Hammer_Rot_Uncocked;
        public float Hammer_Rot_Halfcocked;
        public float Hammer_Rot_Cocked;
        public float LoadingGate_Rot_Closed;
        public float LoadingGate_Rot_Open;
        public float Trigger_Rot_Forward;
        public float Trigger_Rot_Rearward;
        public Vector3 EjectorRod_Pos_Forward;
        public Vector3 EjectorRod_Pos_Rearward;
        public bool DoesCylinderTranslateForward;
        public bool IsAccessTwoChambersBack;
        public Vector3 CylinderBackPos;
        public Vector3 CylinderFrontPos;
        [Header("Spinning Config")]
        public Transform PoseSpinHolder;
        public bool CanSpin = true;
        private bool m_isSpinning;
        [Header("StateToggling")]
        public bool StateToggles = true;
        private bool m_isStateToggled;
        public Transform Pose_Main;
        public Transform Pose_Toggled;
        public float TriggerThreshold = 0.9f;
        private float m_triggerFloat;
        private bool m_isHammerCocking;
        private bool m_isHammerCocked;
        private float m_hammerCockLerp;
        private float m_hammerCockSpeed = 10f;
        private float xSpinRot;
        private float xSpinVel;
        private float timeSinceColFire;
        [Header("DoubleAction Config")]
        private float m_tarTriggerFloat;
        private float m_tarRealTriggerFloat;
        private float m_triggerCurrentRot;
        private float m_curTriggerFloat;
        private float m_curRealTriggerFloat;
        private float lastTriggerRot;
        private Revolver.RecockingState m_recockingState;
        private float m_recockingLerp;
        private bool m_shouldRecock;
        private bool DoesFiringRecock;
        private bool m_hasTriggerCycled;
        private bool m_hasChamberCycled;
        private bool CanManuallyCockHammer = false;
        private bool m_isHammerLocked;
        private float m_hammerCurrentRot;
        private Vector2 RecockingSpeeds = new Vector2(8f, 3f);
        public Transform RecockingPiece;
        public Transform RecockingPoint_Forward;
        public Transform RecockingPoint_Rearward;
        public float ejectedRoundOffset;
        public bool isEjecting;

        private int cylinderSpringCharge = 0;


#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public int CurChamber
        {
            get => this.m_curChamber;
            set => this.m_curChamber = value % this.Cylinder.NumChambers;
        }

        public int NextChamber => (this.m_curChamber + 1) % this.Cylinder.NumChambers;

        public int PrevChamber
        {
            get
            {
                int num = this.m_curChamber - 1;
                return num < 0 ? this.Cylinder.NumChambers - 1 : num;
            }
        }

        public int PrevChamber2
        {
            get
            {
                int num = this.m_curChamber - 2;
                return num < 0 ? this.Cylinder.NumChambers + num : num;
            }
        }

        public override void Awake()
        {
            base.Awake();
            foreach (FVRFireArmChamber chamber in this.Cylinder.Chambers)
                this.FChambers.Add(chamber);
            if (!((Object)this.PoseOverride_Touch != (Object)null))
                return;
            this.Pose_Main.localPosition = this.PoseOverride_Touch.localPosition;
            this.Pose_Main.localRotation = this.PoseOverride_Touch.localRotation;
            this.m_hasChamberCycled = true;
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
            if (this.IsAltHeld)
                return;

            this.PoseOverride.localPosition = this.Pose_Main.localPosition;
            this.PoseOverride.localRotation = this.Pose_Main.localRotation;
            if (!((Object)this.m_grabPointTransform != (Object)null))
                return;
            this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
            this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;

        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            this.m_isSpinning = false;
            this.UpdateTriggerHammer();
            this.UpdateCylinderRot();
            this.UpdateCylinderRelease();
            if (this.IsHeld)
                return;
            this.m_isSpinning = false;
        }

        public override void EndInteraction(FVRViveHand hand)
        {
            this.m_triggerFloat = 0.0f;
            base.EndInteraction(hand);
            this.RootRigidbody.AddRelativeTorque(new Vector3(this.xSpinVel, 0.0f, 0.0f), ForceMode.Impulse);
        }

        public override void FVRFixedUpdate()
        {
            this.UpdateSpinning();
            if ((double)this.timeSinceColFire < 3.0)
                this.timeSinceColFire += Time.deltaTime;
            base.FVRFixedUpdate();
            this.UpdateRecocking();
        }

        private void UpdateRecocking()
        {
            switch (this.m_recockingState)
            {
                case Revolver.RecockingState.GoingBack:
                    this.m_recockingLerp += Time.deltaTime * this.RecockingSpeeds.x;
                    if ((double)this.m_recockingLerp >= 1.0)
                    {
                        this.m_recockingState = Revolver.RecockingState.GoingForward;
                        this.m_isHammerLocked = true;
                    }
                    this.m_recockingLerp = Mathf.Clamp(this.m_recockingLerp, 0.0f, 1f);
                    this.RecockingPiece.position = Vector3.Lerp(this.RecockingPoint_Forward.position, this.RecockingPoint_Rearward.position, this.m_recockingLerp);
                    break;
                case Revolver.RecockingState.GoingForward:
                    this.m_recockingLerp -= Time.deltaTime * this.RecockingSpeeds.y;
                    if ((double)this.m_recockingLerp <= 0.0)
                        this.m_recockingState = Revolver.RecockingState.Forward;
                    this.m_recockingLerp = Mathf.Clamp(this.m_recockingLerp, 0.0f, 1f);
                    this.RecockingPiece.position = Vector3.Lerp(this.RecockingPoint_Forward.position, this.RecockingPoint_Rearward.position, this.m_recockingLerp);
                    break;
            }
        }

        private void InitiateRecock()
        {
            if (this.m_recockingState != Revolver.RecockingState.Forward)
                return;
            this.m_recockingLerp = 0.0f;
            this.m_recockingState = Revolver.RecockingState.GoingBack;
        }

        private void UpdateTriggerHammer()
        {
            if (this.m_hasTriggeredUpSinceBegin && !this.m_isSpinning && !this.m_isStateToggled)
            {
                this.m_tarTriggerFloat = this.m_hand.Input.TriggerFloat;
                this.m_tarRealTriggerFloat = this.m_hand.Input.TriggerFloat;
            }
            else
            {
                this.m_tarTriggerFloat = 0.0f;
                this.m_tarRealTriggerFloat = 0.0f;
            }
            if (this.m_isHammerLocked)
            {
                this.m_tarTriggerFloat += 0.8f;
                this.m_triggerCurrentRot = Mathf.Lerp(this.Trigger_Rot_Forward, this.Trigger_Rot_Rearward, this.m_curTriggerFloat);
            }
            else
                this.m_triggerCurrentRot = Mathf.Lerp(this.Trigger_Rot_Forward, this.Trigger_Rot_Rearward, this.m_curTriggerFloat);
            this.m_curTriggerFloat = Mathf.MoveTowards(this.m_curTriggerFloat, this.m_tarTriggerFloat, Time.deltaTime * 14f);
            this.m_curRealTriggerFloat = Mathf.MoveTowards(this.m_curRealTriggerFloat, this.m_tarRealTriggerFloat, Time.deltaTime * 14f);
            if (DoesCylinderTranslateForward)
            {
                this.Cylinder.transform.localPosition = Vector3.Lerp(this.CylinderBackPos, this.CylinderFrontPos, this.m_curTriggerFloat);
            }
            if ((double)Mathf.Abs(this.m_triggerCurrentRot - this.lastTriggerRot) > 0.00999999977648258)
            {
                if ((UnityEngine.Object)this.Trigger != (UnityEngine.Object)null)
                    this.Trigger.localEulerAngles = new Vector3(this.m_triggerCurrentRot, 0.0f, 0.0f);
            }

            this.lastTriggerRot = this.m_triggerCurrentRot;
            if (this.m_shouldRecock)
            {
                this.m_shouldRecock = false;
                this.InitiateRecock();
            }
            if (this.m_hasChamberCycled && this.cylinderSpringCharge > 0)
            {
                if ((double)this.m_curTriggerFloat >= 0.5 && !this.m_hand.Input.TouchpadPressed)
                {
                        this.m_hasChamberCycled = false;
                }
            }
            else if (!this.m_hasChamberCycled && (double)this.m_curRealTriggerFloat <= 0.2 && this.cylinderSpringCharge > 0)
            {
                this.m_hasChamberCycled = true;
                this.m_hand.Buzz(this.m_hand.Buzzer.Buzz_OnHoverInventorySlot);
                if (this.Foregrip != null && this.Foregrip.GetComponent<FVRAlternateGrip>() != null && this.Foregrip.GetComponent<FVRAlternateGrip>().m_hand != null)
                {
                    this.Foregrip.GetComponent<FVRAlternateGrip>().m_hand.Buzz(this.Foregrip.GetComponent<FVRAlternateGrip>().m_hand.Buzzer.Buzz_OnHoverInventorySlot);
                }
                this.AdvanceCylinder();
            }
            if (!this.m_hasTriggerCycled && !this.DoesFiringRecock)
            {
                bool flag1 = false;
                if (this.DoesFiringRecock && this.m_recockingState != Revolver.RecockingState.Forward)
                    flag1 = true;
                if (!flag1 && (double)this.m_curTriggerFloat >= 0.980000019073486 && !this.m_hand.Input.TouchpadPressed)
                {
                    if (!this.m_isStateToggled)
                    {
                        this.m_hasTriggerCycled = true;
                        this.m_isHammerLocked = false;
                        this.PlayAudioEvent(FirearmAudioEventType.HammerHit);
                        this.Fire();
                        if (this.DoesFiringRecock)
                            this.m_shouldRecock = true;
                    }
                }
                else if (((double)this.m_curTriggerFloat <= 0.0799999982118607) && !this.m_isHammerLocked && this.CanManuallyCockHammer)
                {
                    bool flag2 = false;
                    if (this.DoesFiringRecock && this.m_recockingState != Revolver.RecockingState.Forward)
                        flag2 = true;
                    if (!this.IsAltHeld && !flag2)
                    {
                        if (this.m_hand.IsInStreamlinedMode)
                        {
                            if (this.m_hand.Input.AXButtonDown)
                            {
                                this.m_isHammerLocked = true;
                                this.PlayAudioEvent(FirearmAudioEventType.Prefire);
                            }
                        }
                        else if (this.m_hand.Input.TouchpadDown && (double)Vector2.Angle(this.m_hand.Input.TouchpadAxes, Vector2.down) < 45.0)
                        {
                            this.m_isHammerLocked = true;
                            this.PlayAudioEvent(FirearmAudioEventType.Prefire);
                        }
                    }
                }
            }
            else if (this.m_hasTriggerCycled && (double)this.m_curRealTriggerFloat <= 0.0799999982118607)
            {
                this.m_hasTriggerCycled = false;
                this.PlayAudioEvent(FirearmAudioEventType.TriggerReset);
            }
            this.m_hammerCurrentRot = this.m_hasTriggerCycled ? (!this.m_isHammerLocked ? Mathf.Lerp(this.m_hammerCurrentRot, this.Hammer_Rot_Uncocked, Time.deltaTime * 30f) : Mathf.Lerp(this.m_hammerCurrentRot, this.Hammer_Rot_Cocked, Time.deltaTime * 10f)) : (!this.m_isHammerLocked ? Mathf.Lerp(this.Hammer_Rot_Uncocked, this.Hammer_Rot_Cocked, this.m_curTriggerFloat) : Mathf.Lerp(this.m_hammerCurrentRot, this.Hammer_Rot_Cocked, Time.deltaTime * 10f));
            if (!((UnityEngine.Object)this.Hammer != (UnityEngine.Object)null))
                return;
            this.Hammer.localEulerAngles = new Vector3(this.m_hammerCurrentRot, 0.0f, 0.0f);
            if ((Object)this.LoadingGate != (Object)null)
                this.LoadingGate.localEulerAngles = this.m_isStateToggled ? new Vector3(0.0f, 0.0f, this.LoadingGate_Rot_Open) : new Vector3(0.0f, 0.0f, this.LoadingGate_Rot_Closed);

        }

        private void UpdateCylinderRelease()
        {
            //if (this.m_isHammerLocked)
            //    this.m_tarChamberLerp = 1f;
            //else if (!this.m_hasTriggerCycled)
            //    this.m_tarChamberLerp = this.m_curTriggerFloat * 1.4f;
            //this.m_tarChamberLerp = 0f;
            //if (this.m_isChamberLerping)
            //{
            //    this.m_tarChamberLerp = 1f;
            //}
            //else
            //{
            //    this.m_tarChamberLerp = 0f;
            //    return;
            //}
            //this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);

            //int cylinder = this.CurChamber + 1 % this.Cylinder.NumChambers;
            //this.Cylinder.transform.localRotation = Quaternion.Slerp(this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber), this.Cylinder.GetLocalRotationFromCylinder(cylinder), this.m_curChamberLerp);
            //Debug.Log("curChamberLerp" + this.m_curChamberLerp);
            //Debug.Log("tarChamberLerp" + this.m_tarChamberLerp);
            //if((this.m_curChamberLerp - this.m_tarChamberLerp) < 0.011)
            //{
            //    this.m_isChamberLerping = false;
            //}
        }

        private void AdvanceCylinder()
        {
            if (this.cylinderSpringCharge == 0)
                return;
            --this.cylinderSpringCharge;
            ++this.CurChamber;
            this.PlayAudioEvent(FirearmAudioEventType.FireSelector);
        }
        public void RetreatCylinder()
        {
            if (this.cylinderSpringCharge == 30)
                return;
            ++this.cylinderSpringCharge;
            if(this.CurChamber == 0)
            {
                this.CurChamber = 11;
            } else
            {
                this.CurChamber = this.PrevChamber;
            }
            this.PlayAudioEvent(FirearmAudioEventType.FireSelector);
        }

        public void EjectPrevCylinder()
        {
            int index = this.PrevChamber;
            if (this.IsAccessTwoChambersBack)
                index = this.PrevChamber2;
            FVRFireArmChamber chamber = this.Cylinder.Chambers[index];
            if (chamber.IsFull)
                this.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound);
            chamber.EjectRound(chamber.transform.position + chamber.transform.forward * this.ejectedRoundOffset, -chamber.transform.forward, Vector3.zero);
        }

        private void Fire()
        {
            this.PlayAudioEvent(FirearmAudioEventType.HammerHit);
            if (!this.Cylinder.Chambers[this.CurChamber].Fire())
                return;
            FVRFireArmChamber chamber = this.Cylinder.Chambers[this.CurChamber];
            this.Fire(chamber, this.GetMuzzle(), true);
            this.FireMuzzleSmoke();
            this.Recoil(this.IsTwoHandStabilized(), (Object)this.AltGrip != (Object)null, this.IsShoulderStabilized());
            this.PlayAudioGunShot(chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
            if (!GM.CurrentSceneSettings.IsAmmoInfinite && !GM.CurrentPlayerBody.IsInfiniteAmmo)
                return;
            chamber.IsSpent = false;
            chamber.UpdateProxyDisplay();
        }

        private void UpdateCylinderRot()
        {
            int num = this.PrevChamber;
            if (this.IsAccessTwoChambersBack)
                num = this.PrevChamber2;
            for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
                this.Cylinder.Chambers[index].IsAccessible = index == num;
            this.Cylinder.transform.localRotation = this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber);

            //else
            //{
            //    for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
            //        this.Cylinder.Chambers[index].IsAccessible = false;
            //}
        }

        private void UpdateSpinning()
        {
            if (!this.IsHeld)
                this.m_isSpinning = false;
            if (this.m_isSpinning)
            {
                Vector3 vector3 = Vector3.zero;
                if ((Object)this.m_hand != (Object)null)
                    vector3 = this.m_hand.Input.VelLinearLocal;
                float f = Mathf.Clamp(Vector3.Dot(vector3.normalized, this.transform.up), -vector3.magnitude, vector3.magnitude);
                if ((double)Mathf.Abs(this.xSpinVel) < 90.0)
                    this.xSpinVel += (float)((double)f * (double)Time.deltaTime * 600.0);
                else if ((double)Mathf.Sign(f) == (double)Mathf.Sign(this.xSpinVel))
                    this.xSpinVel += (float)((double)f * (double)Time.deltaTime * 600.0);
                if ((double)Mathf.Abs(this.xSpinVel) < 90.0)
                {
                    if ((double)Vector3.Dot(this.transform.up, Vector3.down) >= 0.0 && (double)Mathf.Sign(this.xSpinVel) == 1.0)
                        this.xSpinVel += Time.deltaTime * 50f;
                    if ((double)Vector3.Dot(this.transform.up, Vector3.down) < 0.0 && (double)Mathf.Sign(this.xSpinVel) == -1.0)
                        this.xSpinVel -= Time.deltaTime * 50f;
                }
                this.xSpinVel = Mathf.Clamp(this.xSpinVel, -500f, 500f);
                this.xSpinRot += (float)((double)this.xSpinVel * (double)Time.deltaTime * 5.0);
                this.PoseSpinHolder.localEulerAngles = new Vector3(this.xSpinRot, 0.0f, 0.0f);
                this.xSpinVel = Mathf.Lerp(this.xSpinVel, 0.0f, Time.deltaTime * 0.6f);
            }
            else
            {
                this.xSpinRot = 0.0f;
                this.xSpinVel = 0.0f;
                this.PoseSpinHolder.localRotation = Quaternion.RotateTowards(this.PoseSpinHolder.localRotation, Quaternion.identity, Time.deltaTime * 500f);
                this.PoseSpinHolder.localEulerAngles = new Vector3(this.PoseSpinHolder.localEulerAngles.x, 0.0f, 0.0f);
            }
        }

        private void CockHammer(float speed)
        {
            if (this.m_isHammerLocked || this.m_isHammerCocking)
                return;
            this.m_isHammerLocked = true;
            this.PlayAudioEvent(FirearmAudioEventType.Prefire);
        }

        //private void ToggleState()
        //{
        //    this.m_isHammerLocked = false;
        //    this.m_isStateToggled = !this.m_isStateToggled;
        //    if (!this.IsAltHeld)
        //    {
        //        if (!this.m_isStateToggled)
        //        {
        //            this.PoseOverride.localPosition = this.Pose_Main.localPosition;
        //            this.PoseOverride.localRotation = this.Pose_Main.localRotation;
        //            if ((Object)this.m_grabPointTransform != (Object)null)
        //            {
        //                this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
        //                this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
        //            }
        //        }
        //        else
        //        {
        //            this.PoseOverride.localPosition = this.Pose_Toggled.localPosition;
        //            this.PoseOverride.localRotation = this.Pose_Toggled.localRotation;
        //            if ((Object)this.m_grabPointTransform != (Object)null)
        //            {
        //                this.m_grabPointTransform.localPosition = this.Pose_Toggled.localPosition;
        //                this.m_grabPointTransform.localRotation = this.Pose_Toggled.localRotation;
        //            }
        //        }
        //    }
        //    this.m_isHammerCocking = false;
        //    this.m_isHammerCocked = false;
        //    this.m_hammerCockLerp = 0.0f;
        //    if (!this.m_isStateToggled)
        //        ;
        //}

        public override void OnCollisionEnter(Collision col)
        {
            base.OnCollisionEnter(col);
        }

        public override List<FireArmRoundClass> GetChamberRoundList()
        {
            bool flag = false;
            List<FireArmRoundClass> fireArmRoundClassList = new List<FireArmRoundClass>();
            for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
            {
                if (this.Cylinder.Chambers[index].IsFull)
                {
                    fireArmRoundClassList.Add(this.Cylinder.Chambers[index].GetRound().RoundClass);
                    flag = true;
                }
            }
            return flag ? fireArmRoundClassList : (List<FireArmRoundClass>)null;
        }

        public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
        {
            if (rounds.Count <= 0)
                return;
            for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
            {
                if (index < rounds.Count)
                    this.Cylinder.Chambers[index].Autochamber(rounds[index]);
            }
        }
#endif
    }

}


