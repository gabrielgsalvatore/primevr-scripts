using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace PrimeVrScripts
{
    public class UtsToggleFlashlight : FVRInteractiveObject
    {
        public UtsShotgun UtsShotgun;

        #if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.UtsShotgun.ToggleFlashlightLaser();
        }
        #endif
    }
}
