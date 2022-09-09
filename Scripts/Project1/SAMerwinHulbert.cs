using FistVR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PrimeVrScripts
{
    public class SAMerwinHulbert : SingleActionRevolver
    {

        public RetractableBarrel retractableBarrel;
        public float ejectionPositionTrigger;
        public float LoadingGate_Pos_Open;
        public float LoadingGate_Pos_Closed;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();
            Hook();
        }

        public override void OnDestroy()
        {
            Unhook();
            base.OnDestroy();
        }
        private void Unhook()
        {
            On.FistVR.SingleActionRevolver.Fire -= SingleActionRevolver_Fire;
            On.FistVR.SingleActionRevolver.AdvanceCylinder -= SingleActionRevolver_AdvanceCylinder;
            On.FistVR.SingleActionRevolver.CockHammer -= SingleActionRevolver_CockHammer;
            On.FistVR.SingleActionRevolver.EjectPrevCylinder -= SingleActionRevolver_EjectPrevCylinder;
        }

        private void Hook()
        {
            On.FistVR.SingleActionRevolver.Fire += SingleActionRevolver_Fire;
            On.FistVR.SingleActionRevolver.AdvanceCylinder += SingleActionRevolver_AdvanceCylinder;
            On.FistVR.SingleActionRevolver.CockHammer += SingleActionRevolver_CockHammer;
            On.FistVR.SingleActionRevolver.EjectPrevCylinder += SingleActionRevolver_EjectPrevCylinder;
        }

        private void SingleActionRevolver_EjectPrevCylinder(On.FistVR.SingleActionRevolver.orig_EjectPrevCylinder orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                return;
            }
            else
            {
                orig(self);
            }
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            this.StateToggles = false;
            if (this.retractableBarrel.isClosed && !this.retractableBarrel.IsHeld)
            {
                this.StateToggles = true;
            }
            this.verifyIfShouldEject();
            base.UpdateInteraction(hand);
            this.retractableBarrel.canRotate = !this.m_isStateToggled;
            this.LoadingGate.localPosition = this.m_isStateToggled ? new Vector3(this.LoadingGate.localPosition.x, this.LoadingGate_Pos_Open, this.LoadingGate.localPosition.z) : new Vector3(this.LoadingGate.localPosition.x, this.LoadingGate_Pos_Closed, this.LoadingGate.localPosition.z);
            this.retractableBarrel.GetComponent<BoxCollider>().enabled = !this.m_isStateToggled;
        }

        private void verifyIfShouldEject()
        {
            if(this.retractableBarrel.objectToMove.localPosition.z >= this.ejectionPositionTrigger)
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

        private void SingleActionRevolver_CockHammer(On.FistVR.SingleActionRevolver.orig_CockHammer orig, SingleActionRevolver self, float speed)
        {
            if(self == this)
            {
                if (!this.retractableBarrel.isClosed)
                {
                    return;
                }
                orig(self, speed);
            }
            else
            {
                orig(self, speed);
            }
        }

        private void SingleActionRevolver_AdvanceCylinder(On.FistVR.SingleActionRevolver.orig_AdvanceCylinder orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                if (!this.retractableBarrel.isClosed)
                {
                    return;
                }
                ++this.CurChamber;
                this.PlayAudioEvent(FirearmAudioEventType.FireSelector);
            }
            else orig(self);
        }

        private void SingleActionRevolver_Fire(On.FistVR.SingleActionRevolver.orig_Fire orig, SingleActionRevolver self)
        {
            if (self == this)
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
            else orig(self);
        }
#endif
    }
}


