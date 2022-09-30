using FistVR;
using UnityEngine;


namespace PrimeVrScripts
{
    public class LematForegrip : FVRAlternateGrip
    {

        public Transform ShotgunBase;
        public HingeJoint Hinge;
        private Vector3 localPosStart;
        private Rigidbody RB;
        private LematMk2 WepRef;
        private float m_initialDamp;
        private float m_initialSpring;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)

		public LematForegrip()
		{
			this.m_initialDamp = 0.05f;
			this.m_initialSpring = 0.05f;
			this.FunctionalityEnabled = false;
		}

        public override void Awake()
		{
			base.Awake();
			this.localPosStart = this.Hinge.transform.localPosition;
			this.RB = this.Hinge.gameObject.GetComponent<Rigidbody>();
			this.WepRef = this.Hinge.connectedBody.gameObject.GetComponent<LematMk2>();
			JointSpring spring = this.Hinge.spring;
			this.m_initialSpring = spring.spring;
			this.m_initialDamp = spring.damper;
		}

		public override void FVRUpdate()
		{
			base.FVRUpdate();
			if ((double)Vector3.Distance(this.Hinge.transform.localPosition, this.localPosStart) <= 0.00999999977648258)
				return;
			this.Hinge.transform.localPosition = this.localPosStart;
		}

		public override void FVRFixedUpdate()
		{
			base.FVRFixedUpdate();
			if (this.WepRef.IsHeld && this.WepRef.IsAltHeld)
				this.RB.mass = 1f / 1000f;
			else
				this.RB.mass = 0.1f;
		}

		public override bool IsInteractable()
		{
            if (WepRef)
            {
				if (WepRef.isLatched)
					return false;
			}
			return true;
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			Vector3 from = Vector3.ProjectOnPlane(hand.Input.Pos - this.Hinge.transform.position, this.ShotgunBase.right);
			if ((double)Vector3.Angle(from, -this.ShotgunBase.up) > 90.0)
				from = this.ShotgunBase.forward;
			if ((double)Vector3.Angle(from, this.ShotgunBase.forward) > 90.0)
				from = -this.ShotgunBase.up;
			float num = Vector3.Angle(from, this.ShotgunBase.forward);
			this.Hinge.spring = this.Hinge.spring with
			{
				spring = 10f,
				damper = 0.0f,
				targetPosition = Mathf.Clamp(num, 0.0f, this.Hinge.limits.max)
			};
			this.Hinge.transform.localPosition = this.localPosStart;
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			this.Hinge.spring = this.Hinge.spring with
			{
				spring = this.m_initialSpring,
				damper = this.m_initialDamp,
				targetPosition = 45f
			};
			base.EndInteraction(hand);
		}
#endif

	}
}
