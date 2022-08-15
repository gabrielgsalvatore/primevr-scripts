using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class StreetSweeperEjectorRod : FVRInteractiveObject
    {

		public StreetSweeper streetSweeper;
		public Transform rodSpring;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)

        public override void FVRFixedUpdate()
		{
			base.FVRFixedUpdate();
            if (this.IsHeld) {
                if (!this.streetSweeper.isEjecting)
                {
					this.streetSweeper.isEjecting = true;
					SM.PlayHandlingGrabSound(HandlingGrabType.BeltSegment, m_handPos, false);
				}
				if ((this.streetSweeper.EjectorRod.transform.localPosition.z - this.streetSweeper.EjectorRod_Pos_Rearward.z) > -0.01f)
				{
					this.streetSweeper.EjectPrevCylinder();
				}
				var step = 1.0f * Time.fixedDeltaTime;
				this.streetSweeper.EjectorRod.transform.localPosition = Vector3.MoveTowards(this.streetSweeper.EjectorRod.transform.localPosition, this.streetSweeper.EjectorRod_Pos_Rearward, step);
				var ejectorRodLerp = Mathf.InverseLerp(this.streetSweeper.EjectorRod_Pos_Rearward.z, this.streetSweeper.EjectorRod_Pos_Forward.z, this.streetSweeper.EjectorRod.transform.localPosition.z);
				var squeezeLerp = Mathf.Lerp(0.450061f, 1f, ejectorRodLerp);
				this.rodSpring.localScale = new Vector3(1f, 1f, squeezeLerp);



			}
		}

        public override void EndInteraction(FVRViveHand hand)
        {
            base.EndInteraction(hand);
			this.streetSweeper.isEjecting = false;
			this.streetSweeper.EjectorRod.localPosition = this.streetSweeper.EjectorRod_Pos_Forward;
			this.rodSpring.localScale = new Vector3(1f, 1f, 1f);
		}
#endif
    }
}
