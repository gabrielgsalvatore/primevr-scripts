using FistVR;
using UnityEngine;
using System.Collections.Generic;


namespace PrimeVrScripts
{
    public class DAMerwinHulbert : FVRFireArm
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
        private float m_tarChamberLerp;
        [Header("Component Movement Params")]
        public float Hammer_Rot_Uncocked;
        public float Hammer_Rot_Halfcocked;
        public float Hammer_Rot_Cocked;
        public float LoadingGate_Rot_Closed;
        public float LoadingGate_Rot_Open;
        public float Trigger_Rot_Forward;
        public float Trigger_Rot_Rearward;
        public bool IsAccessTwoChambersBack;
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
        private bool m_isHammerCocking;
        private bool m_isHammerCocked;
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
        public bool CanManuallyCockHammer;
        private bool m_isHammerLocked;
        private float m_hammerCurrentRot;
        private Vector2 RecockingSpeeds = new Vector2(8f, 3f);
        public Transform RecockingPiece;
        public Transform RecockingPoint_Forward;
        public Transform RecockingPoint_Rearward;
        public float ejectedRoundOffset;
        public bool doesToggleStateHalfRotatesCylinder;
        [Header("Merwin Hulbert")]
        public RetractableBarrel retractableBarrel;
        public float ejectionPositionTrigger;
        public float LoadingGate_Pos_Open;
        public float LoadingGate_Pos_Closed;



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
        }

        private void verifyIfShouldEject()
        {
            if (this.retractableBarrel.objectToMove.localPosition.z >= this.ejectionPositionTrigger)
            {
                for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
                {
                    if (this.Cylinder.Chambers[index].GetRound() != null && this.Cylinder.Chambers[index].IsSpent)
                    {
                        var chmber = this.Cylinder.Chambers[index];
                        this.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound);
                        chmber.EjectRound(chmber.transform.position, Vector3.zero, Vector3.zero);
                    }
                }

            }
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
            if (this.IsAltHeld)
                return;
            if (!this.m_isStateToggled)
            {
                this.PoseOverride.localPosition = this.Pose_Main.localPosition;
                this.PoseOverride.localRotation = this.Pose_Main.localRotation;
                if (!((Object)this.m_grabPointTransform != (Object)null))
                    return;
                this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
                this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
            }
            else
            {
                this.PoseOverride.localPosition = this.Pose_Toggled.localPosition;
                this.PoseOverride.localRotation = this.Pose_Toggled.localRotation;
                if (!((Object)this.m_grabPointTransform != (Object)null))
                    return;
                this.m_grabPointTransform.localPosition = this.Pose_Toggled.localPosition;
                this.m_grabPointTransform.localRotation = this.Pose_Toggled.localRotation;
            }
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            this.StateToggles = false;
            if (this.retractableBarrel.isClosed)
            {
                this.StateToggles = true;
            }
            this.verifyIfShouldEject();
            base.UpdateInteraction(hand);
            this.m_isSpinning = false;
            if (!this.IsAltHeld)
            {
                if (!this.m_isStateToggled)
                {
                    if (hand.Input.TouchpadPressed && !hand.IsInStreamlinedMode && (double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45.0)
                        this.m_isSpinning = true;
                    if (hand.IsInStreamlinedMode)
                    {
                        if (hand.Input.BYButtonDown && this.StateToggles)
                        {
                            this.ToggleState();
                            this.PlayAudioEvent(FirearmAudioEventType.BreachOpen);
                        }
                    }
                    else if (hand.Input.TouchpadDown)
                    {
                        if ((double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45.0 && this.StateToggles)
                        {
                            this.ToggleState();
                            this.PlayAudioEvent(FirearmAudioEventType.BreachOpen);
                        }
                    }
                }
                else
                {
                    if (hand.IsInStreamlinedMode)
                    {
                        if (hand.Input.AXButtonDown)
                            this.AdvanceCylinder();
                        if (hand.Input.BYButtonDown && this.StateToggles)
                        {
                            this.ToggleState();
                            this.PlayAudioEvent(FirearmAudioEventType.BreachOpen);
                        }
                    }
                    else if (hand.Input.TouchpadDown)
                    {
                        if ((double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45.0 && this.StateToggles)
                        {
                            this.ToggleState();
                            this.PlayAudioEvent(FirearmAudioEventType.BreachClose);
                        }
                        else if ((double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45.0)
                            this.AdvanceCylinder();
                    }
                }
            }
            this.UpdateTriggerHammer();
            this.UpdateCylinderRot();
            this.UpdateCylinderRelease();
            if (this.IsHeld)
                return;
            this.m_isSpinning = false;
        }

        public override void EndInteraction(FVRViveHand hand)
        {
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
            if (this.IsHeld && !this.m_isStateToggled && !this.m_isHammerCocked && !this.m_isHammerCocking && (Object)this.m_hand.OtherHand != (Object)null && this.CanManuallyCockHammer && !this.m_isStateToggled)
            {
                Vector3 velLinearWorld = this.m_hand.OtherHand.Input.VelLinearWorld;
                if ((double)Vector3.Distance(this.m_hand.OtherHand.PalmTransform.position, this.HammerFanDir.position) < 0.150000005960464 && (double)Vector3.Angle(velLinearWorld, this.HammerFanDir.forward) < 60.0 && (double)velLinearWorld.magnitude > 1.0)
                    this.CockHammer(10f);
            }
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
                        if (this.retractableBarrel.isClosed)
                        {
                            ++this.CurChamber;
                            this.m_curChamberLerp = 0.0f;
                            this.m_tarChamberLerp = 0.0f;
                            this.Fire();
                        }
                        if (this.DoesFiringRecock)
                            this.m_shouldRecock = true;
                    }
                }
                else if (((double)this.m_curTriggerFloat <= 0.0799999982118607) && !this.m_isHammerLocked && this.CanManuallyCockHammer && !this.m_isStateToggled)
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
                this.LoadingGate.localPosition = this.m_isStateToggled ? new Vector3(this.LoadingGate.localPosition.x, this.LoadingGate_Pos_Open, this.LoadingGate.localPosition.z) : new Vector3(this.LoadingGate.localPosition.x, this.LoadingGate_Pos_Closed, this.LoadingGate.localPosition.z);

        }

        private void UpdateCylinderRelease()
        {
            if (!this.retractableBarrel.isClosed)
                return;
            if (this.m_isHammerLocked)
                this.m_tarChamberLerp = 1f;
            else if (!this.m_hasTriggerCycled && this.doesToggleStateHalfRotatesCylinder && this.m_isStateToggled)
                this.m_tarChamberLerp = 0.5f;
            else if (!this.m_hasTriggerCycled)
                this.m_tarChamberLerp = this.m_curTriggerFloat * 1.4f;
            this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);

            int cylinder = this.CurChamber + 1 % this.Cylinder.NumChambers;
            this.Cylinder.transform.localRotation = Quaternion.Slerp(this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber), this.Cylinder.GetLocalRotationFromCylinder(cylinder), this.m_curChamberLerp);
        }

        private void AdvanceCylinder()
        {
            if (!this.retractableBarrel.isClosed)
                return;
            ++this.CurChamber;
            this.PlayAudioEvent(FirearmAudioEventType.FireSelector);
        }

        private void Fire()
        {
            this.PlayAudioEvent(FirearmAudioEventType.HammerHit);
            if (!this.retractableBarrel.isClosed)
                return;
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
            if (this.m_isStateToggled)
            {
                int num = this.PrevChamber;
                if (this.IsAccessTwoChambersBack)
                    num = this.PrevChamber2;
                for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
                    this.Cylinder.Chambers[index].IsAccessible = index == num;
                this.Cylinder.transform.localRotation = this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber);
            }
            else
            {
                for (int index = 0; index < this.Cylinder.Chambers.Length; ++index)
                    this.Cylinder.Chambers[index].IsAccessible = false;
            }
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

        private void ToggleState()
        {
            if (!this.retractableBarrel.isClosed || this.retractableBarrel.IsHeld)
                return;
            this.m_isHammerLocked = false;
            this.m_isStateToggled = !this.m_isStateToggled;
            this.retractableBarrel.canRotate = !this.m_isStateToggled;
            this.retractableBarrel.GetComponent<BoxCollider>().enabled = !this.m_isStateToggled;
            if (!this.IsAltHeld)
            {
                if (!this.m_isStateToggled)
                {
                    this.PoseOverride.localPosition = this.Pose_Main.localPosition;
                    this.PoseOverride.localRotation = this.Pose_Main.localRotation;
                    if ((Object)this.m_grabPointTransform != (Object)null)
                    {
                        this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
                        this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
                    }
                }
                else
                {
                    this.PoseOverride.localPosition = this.Pose_Toggled.localPosition;
                    this.PoseOverride.localRotation = this.Pose_Toggled.localRotation;
                    if ((Object)this.m_grabPointTransform != (Object)null)
                    {
                        this.m_grabPointTransform.localPosition = this.Pose_Toggled.localPosition;
                        this.m_grabPointTransform.localRotation = this.Pose_Toggled.localRotation;
                    }
                }
            }
            this.m_isHammerCocking = false;
            this.m_isHammerCocked = false;
            if (!this.m_isStateToggled)
                ;
        }

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


