using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace PrimeVrScripts
{
    public class UtsLoadingGate : FVRInteractiveObject
    {
        public bool isOpen;
        public AudioEvent audioClipOpen;
        public AudioEvent audioClipClose;
        public UtsFollower follower;
        public Vector3 openRotation;
        public Vector3 closedRotation;
        public TubeFedShotgunHandle shotgunHandle;
        public UtsShotgun shotgun;

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            if(this.shotgunHandle.CurPos != TubeFedShotgunHandle.BoltPos.Forward)
            {
                return;
            }
            this.isOpen = !isOpen;
            this.shotgun.Bolt.GetComponents<Collider>().ForEach(collider => collider.enabled = !isOpen);
            if (this.isOpen)
            {
                this.shotgun.gatesOpen++;
                this.follower.reloadTrigger.gameObject.SetActive(true);
                this.transform.localEulerAngles = openRotation;
                SM.PlayGenericSound(audioClipOpen, transform.position);
                this.shotgunHandle.LockHandle();
                this.shotgun.Bolt.GetComponents<Collider>().ForEach(collider => collider.enabled = false);
            }
            else
            {
                this.shotgun.gatesOpen--;
                if(this.shotgun.gatesOpen == 0)
                {
                    this.shotgun.Bolt.GetComponents<Collider>().ForEach(collider => collider.enabled = true);
                    if (this.shotgunHandle.Shotgun.Mode == TubeFedShotgun.ShotgunMode.PumpMode)
                    {
                        this.shotgunHandle.UnlockHandle();
                    }
                }
                this.follower.reloadTrigger.gameObject.SetActive(false);
                this.transform.localEulerAngles = closedRotation;
                SM.PlayGenericSound(audioClipClose, transform.position);
                this.follower.isSpringPressed = false;
                this.follower.tubeMagazine.DisplayRoundsOrigin.transform.localPosition = this.follower.tubeMagazine.DisplayRoundsOriginDePressedTransform;

                if (this.follower.tubeMagazine.IsFull())
                {
                    this.follower.tubeMagazine.DisplayFinalRound.transform.localPosition = this.follower.tubeMagazine.DisplayFinalRoundOriginDePressedTransform;
                    this.follower.tubeMagazine.DisplayRoundsOrigin.transform.localPosition = this.follower.tubeMagazine.DisplayRoundsOriginDePressedFullTransform;
                }
            }
        }
    }
}
