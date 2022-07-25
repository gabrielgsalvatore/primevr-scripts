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
			if (Vector3.Distance(this.Hinge.transform.localPosition, this.localPosStart) > 0.01f)
			{
				this.Hinge.transform.localPosition = this.localPosStart;
			}
		}

		public override void FVRFixedUpdate()
		{
			base.FVRFixedUpdate();
			if (this.WepRef.IsHeld && this.WepRef.IsAltHeld)
			{
				this.RB.mass = 0.001f;
			}
			else
			{
				this.RB.mass = 0.1f;
			}
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
			Vector3 vector = hand.Input.Pos - this.Hinge.transform.position;
			Vector3 vector2 = Vector3.ProjectOnPlane(vector, this.ShotgunBase.right);
			if (Vector3.Angle(vector2, -this.ShotgunBase.up) > 90f)
			{
				vector2 = this.ShotgunBase.forward;
			}
			if (Vector3.Angle(vector2, this.ShotgunBase.forward) > 90f)
			{
				vector2 = -this.ShotgunBase.up;
			}
			float num = Vector3.Angle(vector2, this.ShotgunBase.forward);
			JointSpring spring = this.Hinge.spring;
			spring.spring = 10f;
			spring.damper = 0f;
			spring.targetPosition = Mathf.Clamp(num, 0f, this.Hinge.limits.max);
			this.Hinge.spring = spring;
			this.Hinge.transform.localPosition = this.localPosStart;
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			JointSpring spring = this.Hinge.spring;
			spring.spring = this.m_initialSpring;
			spring.damper = this.m_initialDamp;
			spring.targetPosition = 45f;
			this.Hinge.spring = spring;
			base.EndInteraction(hand);
		}
#endif

	}
}
