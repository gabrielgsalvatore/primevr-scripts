﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class LematCenterfireEjectorRod : FVRInteractiveObject
    {

		public LematCenterfire revolver;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)

        public override void FVRFixedUpdate()
		{
			base.FVRFixedUpdate();
            if (this.IsHeld) {
                if (!this.revolver.isEjecting)
                {
					this.revolver.isEjecting = true;
					SM.PlayHandlingGrabSound(HandlingGrabType.BeltSegment, m_handPos, false);
				}
				if ((this.revolver.EjectorRod.transform.localPosition.z - this.revolver.EjectorRod_Pos_Rearward.z) > -0.01f)
				{
					this.revolver.EjectPrevCylinder();
				}
				var step = 1.0f * Time.fixedDeltaTime;
				this.revolver.EjectorRod.transform.localPosition = Vector3.MoveTowards(this.revolver.EjectorRod.transform.localPosition, new Vector3(this.revolver.EjectorRod.transform.localPosition.x, this.revolver.EjectorRod.transform.localPosition.y, this.revolver.EjectorRod_Pos_Rearward.z), step);

			}
		}

        public override void EndInteraction(FVRViveHand hand)
        {
            base.EndInteraction(hand);
			this.revolver.isEjecting = false;
			this.revolver.EjectorRod.transform.localPosition = new Vector3(this.revolver.EjectorRod.transform.localPosition.x, this.revolver.EjectorRod.transform.localPosition.y, this.revolver.EjectorRod_Pos_Forward.z);
		}
#endif
    }
}
