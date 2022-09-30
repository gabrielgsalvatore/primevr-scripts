﻿using FistVR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PrimeVrScripts
{
    public class SingleActionChainRevolver : FVRFireArm
    {
        [Header("Single Action Revolver")]
        public bool AllowsSuppressor;
        public Transform Hammer;
        public Transform LoadingGate;
        public Transform Trigger;
        public Transform EjectorRod;
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
        public Vector3 EjectorRod_Pos_Forward;
        public Vector3 EjectorRod_Pos_Rearward;
        public bool DoesCylinderTranslateForward;
        public bool DoesHalfCockHalfRotCylinder;
        public bool HasTransferBarSafety;
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
        [Header("ChainRevolver")]
        public SingleActionChainRevolverChain SingleActionChainRevolverChain;
        public GameObject ChainPrefab;
        private GameObject instantiatedClone;
        public Transform ChainTargetPosition;
        public Transform RevolvingCenterAxis;
        public float ejectedRoundOffset;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public int CurChamber
        {
            get => this.m_curChamber;
            set => this.m_curChamber = value % this.SingleActionChainRevolverChain.numChambers;
        }

        //public int NextChamber => (this.m_curChamber + 1) % this.Cylinder.NumChambers;

        public int PrevChamber
        {
            get
            {
                int num = this.m_curChamber - 1;
                return num < 0 ? this.SingleActionChainRevolverChain.numChambers - 1 : num;
            }
        }

        //public int PrevChamber2
        //{
        //    get
        //    {
        //        int num = this.m_curChamber - 2;
        //        return num < 0 ? this.Cylinder.NumChambers + num : num;
        //    }
        //}

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameObject.Destroy(this.instantiatedClone, 0f);
        }

        public override void Awake()
        {
            base.Awake();
            this.instantiatedClone = GameObject.Instantiate(ChainPrefab, this.ChainTargetPosition.transform.position, Quaternion.identity);
            this.SingleActionChainRevolverChain = this.instantiatedClone.GetComponent<SingleActionChainRevolverChain>();
            Collider[] chainRevolverColliders = this.instantiatedClone.GetComponentsInChildren<Collider>();
            Collider[] thisGunColliders = this.gameObject.GetComponentsInChildren<Collider>();
            for(int i = 0; i < chainRevolverColliders.Length; i++)
            {
                for(int j = 0; j < thisGunColliders.Length; j++)
                {
                    Physics.IgnoreCollision(chainRevolverColliders[i], thisGunColliders[j]);
                }
            }

            foreach (FVRFireArmChamber chamber in this.SingleActionChainRevolverChain.Chambers)
                this.FChambers.Add(chamber);
            if (!((Object)this.PoseOverride_Touch != (Object)null))
                return;
            this.Pose_Main.localPosition = this.PoseOverride_Touch.localPosition;
            this.Pose_Main.localRotation = this.PoseOverride_Touch.localRotation;
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
                        if (hand.Input.AXButtonDown)
                            this.CockHammer(5f);
                        if (hand.Input.BYButtonDown && this.StateToggles)
                        {
                            this.ToggleState();
                            this.PlayAudioEvent(FirearmAudioEventType.BreachOpen);
                        }
                    }
                    else if (hand.Input.TouchpadDown)
                    {
                        if ((double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45.0)
                            this.CockHammer(5f);
                        else if ((double)Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45.0 && this.StateToggles)
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
                    if (hand.Input.TriggerDown)
                        this.EjectPrevCylinder();
                }
            }
            this.UpdateTriggerHammer();
            this.UpdateCylinderRot();
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
        }

        public override void FVRUpdate()
        {
            this.instantiatedClone.transform.position = this.ChainTargetPosition.position;
            this.instantiatedClone.transform.rotation = this.ChainTargetPosition.rotation;
            base.FVRUpdate();
        }

        private void UpdateTriggerHammer()
        {
            if (this.IsHeld && !this.m_isStateToggled && !this.m_isHammerCocked && !this.m_isHammerCocking && (Object)this.m_hand.OtherHand != (Object)null)
            {
                Vector3 velLinearWorld = this.m_hand.OtherHand.Input.VelLinearWorld;
                if ((double)Vector3.Distance(this.m_hand.OtherHand.PalmTransform.position, this.HammerFanDir.position) < 0.150000005960464 && (double)Vector3.Angle(velLinearWorld, this.HammerFanDir.forward) < 60.0 && (double)velLinearWorld.magnitude > 1.0)
                    this.CockHammer(6f);
            }
            if (this.m_isHammerCocking)
            {
                if ((double)this.m_hammerCockLerp < 1.0)
                {
                    this.m_hammerCockLerp += Time.deltaTime * this.m_hammerCockSpeed;
                }
                else
                {
                    this.m_hammerCockLerp = 1f;
                    this.m_isHammerCocking = false;
                    this.m_isHammerCocked = true;
                    ++this.CurChamber;
                    this.m_curChamberLerp = 0.0f;
                    this.m_tarChamberLerp = 0.0f;
                }
            }
            this.Hammer.localEulerAngles = this.m_isStateToggled ? new Vector3(this.Hammer_Rot_Halfcocked, 0.0f, 0.0f) : new Vector3(Mathf.Lerp(this.Hammer_Rot_Uncocked, this.Hammer_Rot_Cocked, this.m_hammerCockLerp), 0.0f, 0.0f);
            if ((Object)this.LoadingGate != (Object)null)
                this.LoadingGate.localEulerAngles = this.m_isStateToggled ? new Vector3(0.0f, 0.0f, this.LoadingGate_Rot_Open) : new Vector3(0.0f, 0.0f, this.LoadingGate_Rot_Closed);
            this.m_triggerFloat = 0.0f;
            if (this.m_hasTriggeredUpSinceBegin && !this.m_isSpinning && !this.m_isStateToggled)
                this.m_triggerFloat = this.m_hand.Input.TriggerFloat;
            this.Trigger.localEulerAngles = new Vector3(Mathf.Lerp(this.Trigger_Rot_Forward, this.Trigger_Rot_Rearward, this.m_triggerFloat), 0.0f, 0.0f);
            if ((double)this.m_triggerFloat <= (double)this.TriggerThreshold)
                return;
            this.DropHammer();
        }

        private void DropHammer()
        {
            if (!this.m_isHammerCocked)
                return;
            this.m_isHammerCocked = false;
            this.m_isHammerCocking = false;
            this.m_hammerCockLerp = 0.0f;
            this.Fire();
        }

        private void AdvanceCylinder()
        {
            ++this.CurChamber;
            this.PlayAudioEvent(FirearmAudioEventType.FireSelector);
        }

        //public int nextChamber()
        //{
        //    var nextChamber = CurChamber + 1;
        //    if (nextChamber >= this.SingleActionChainRevolverChain.numChambers)
        //    {
        //        nextChamber = 0;
        //    }
        //    return nextChamber;
        //}

        //public int previousChamber()
        //{
        //    var prevChamber = CurChamber - 1;
        //    if (prevChamber < 0)
        //    {
        //        prevChamber = this.SingleActionChainRevolverChain.numChambers;
        //    }
        //    return prevChamber;
        //}

        public void EjectPrevCylinder()
        {
            if (!this.m_isStateToggled)
                return;
            int index = this.PrevChamber;
            FVRFireArmChamber chamber = this.SingleActionChainRevolverChain.Chambers[index];
            if (chamber.IsFull)
                this.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound);
            chamber.EjectRound(chamber.transform.position + chamber.transform.forward * this.ejectedRoundOffset, -chamber.transform.forward, Vector3.zero);
        }

        private void Fire()
        {
            this.PlayAudioEvent(FirearmAudioEventType.HammerHit);
            if (!this.SingleActionChainRevolverChain.Chambers[this.CurChamber].Fire())
                return;
            FVRFireArmChamber chamber = this.SingleActionChainRevolverChain.Chambers[this.CurChamber];
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
            var desiredChamber = this.CurChamber;
            var desiredLerp = 1f;
            if (this.m_isStateToggled)
            {
                int num = this.PrevChamber;
                for (int index = 0; index < this.SingleActionChainRevolverChain.Chambers.Length; ++index)
                    this.SingleActionChainRevolverChain.Chambers[index].IsAccessible = index == num;
            }
            else
            {
                for (int index = 0; index < this.SingleActionChainRevolverChain.Chambers.Length; ++index)
                    this.SingleActionChainRevolverChain.Chambers[index].IsAccessible = false;
                this.m_tarChamberLerp = !this.m_isHammerCocking ? 0.0f : this.m_hammerCockLerp;
                this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);
                if (this.m_isHammerCocking)
                {
                    desiredChamber = this.CurChamber + 1;
                    desiredLerp = this.m_curChamberLerp;
                }
            }
            this.SingleActionChainRevolverChain.UpdateChainPosition(desiredChamber, desiredLerp);
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
            if (this.m_isHammerCocked || this.m_isHammerCocking)
                return;
            this.m_hammerCockSpeed = speed;
            this.m_isHammerCocking = true;
            this.PlayAudioEvent(FirearmAudioEventType.Prefire);
        }

        private void ToggleState()
        {
            this.m_isStateToggled = !this.m_isStateToggled;
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
            this.m_hammerCockLerp = 0.0f;
            if (!this.m_isStateToggled)
                ;
        }

        public override void OnCollisionEnter(Collision col)
        {
            base.OnCollisionEnter(col);
            if (this.HasTransferBarSafety || !((Object)col.collider.attachedRigidbody == (Object)null) || !this.SingleActionChainRevolverChain.Chambers[this.CurChamber].IsFull || this.SingleActionChainRevolverChain.Chambers[this.CurChamber].IsSpent || this.m_isHammerCocked || this.m_isHammerCocking || this.m_isStateToggled || (double)col.relativeVelocity.magnitude <= 2.0 || (double)this.timeSinceColFire <= 2.90000009536743)
                return;
            this.timeSinceColFire = 0.0f;
            this.Fire();
        }

        public override List<FireArmRoundClass> GetChamberRoundList()
        {
            bool flag = false;
            List<FireArmRoundClass> fireArmRoundClassList = new List<FireArmRoundClass>();
            for (int index = 0; index < this.SingleActionChainRevolverChain.Chambers.Length; ++index)
            {
                if (this.SingleActionChainRevolverChain.Chambers[index].IsFull)
                {
                    fireArmRoundClassList.Add(this.SingleActionChainRevolverChain.Chambers[index].GetRound().RoundClass);
                    flag = true;
                }
            }
            return flag ? fireArmRoundClassList : (List<FireArmRoundClass>)null;
        }

        public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
        {
            if (rounds.Count <= 0)
                return;
            for (int index = 0; index < this.SingleActionChainRevolverChain.Chambers.Length; ++index)
            {
                if (index < rounds.Count)
                    this.SingleActionChainRevolverChain.Chambers[index].Autochamber(rounds[index]);
            }
        }
#endif
    }

}
