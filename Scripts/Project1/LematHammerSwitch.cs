using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class LematHammerSwitch : FVRInteractiveObject
    {

		public LematMk2 lematScript;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
		public override void SimpleInteraction(FVRViveHand hand)
		{
			base.SimpleInteraction(hand);
			this.lematScript.activateShotgun();
		}
#endif
	}
}
