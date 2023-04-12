using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace PrimeVrScripts
{
    public class UtsTogglePumpSemi : FVRInteractiveObject
    {
        public UtsShotgun UtsShotgun;
        public Vector3 PumpRotation;
        public Vector3 SemiRotation;
        public bool inPump = true;
        private TubeFedShotgun.ShotgunMode previousMode;

        #if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.previousMode = this.UtsShotgun.Mode;
            this.UtsShotgun.ToggleMode();
            if(this.previousMode != this.UtsShotgun.Mode)
            {
                this.UtsShotgun.PlayAudioEvent(FirearmAudioEventType.Safety);
            }else
            {

            }
            if(this.UtsShotgun.Mode == TubeFedShotgun.ShotgunMode.PumpMode)
            {
                this.inPump = true;
            }
            else
            {
                this.inPump = false;
            }
            this.transform.localEulerAngles = this.inPump ? PumpRotation : SemiRotation;
        }
        #endif
    }
}
