using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class UtsBolt : TubeFedShotgunBolt
    {

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            base.Awake();
            //Hook();
        }

        public void OnDestroy()
        {
            //Unhook();
        }
        private void Unhook()
        {
            On.FistVR.TubeFedShotgunBolt.UpdateBolt -= this.TubeFedShotgunBolt_UpdateBolt;
        }

        private void Hook()
        {
            On.FistVR.TubeFedShotgunBolt.UpdateBolt += this.TubeFedShotgunBolt_UpdateBolt;
        }

        private void TubeFedShotgunBolt_UpdateBolt(On.FistVR.TubeFedShotgunBolt.orig_UpdateBolt orig, TubeFedShotgunBolt self)
        {
            orig(self);
        }

#endif
    }
}
