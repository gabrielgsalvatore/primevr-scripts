using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class StreetSweeperCylinderCrank : FVRInteractiveObject
    {

		public StreetSweeper streetSweeper;
		public Transform cylinderRoot;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)

		public override void SimpleInteraction(FVRViveHand hand)
		{
			base.SimpleInteraction(hand);
            if (this.streetSweeper.IsHeld)
            {
				this.streetSweeper.RetreatCylinder();
			}
		}

#endif
    }
}
