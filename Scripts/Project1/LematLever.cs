using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class LematLever : FVRInteractiveObject
    {
		public enum Mode
		{
			Tilt
		}
		public Mode mode;

		public enum Direction
		{
			X,
			Y,
			Z
		}
		public Direction direction;

		public Transform root;
		public Transform objectToMove;

		public float lowerLimit = 0f;
		public float upperLimit = 0f;
		public float limitWiggleRoom = 0.04f;

		public AudioSource audioSource;
		public AudioClip closeSound;
		public AudioClip openSound;

		public bool rodClosed = true;

		private enum State
		{
			Open,
			Mid,
			Closed
		}

		private State state;
		private State last_state;

		private float pos;
		private Vector3 orig_pos;

		private Vector3 lastHandPlane;

		public bool debug = true;
#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
		public override void Start()
		{
			base.Start();
			orig_pos = objectToMove.localPosition;
		}
		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			switch (direction)
			{
				case Direction.X:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.up, -root.right);
					break;
				case Direction.Y:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.up, root.forward);
					break;
				case Direction.Z:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.forward, -root.up);
					break;
				default:
					break;
			}

		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			TiltMode(hand);
		}

		private void TiltMode(FVRViveHand hand)
		{
			Vector3 vector = (base.m_handPos) - this.root.position;
			Vector3 lhs = this.root.transform.forward;
			vector = Vector3.ProjectOnPlane(vector, this.root.right).normalized;
			pos = Mathf.Atan2(Vector3.Dot(this.root.right, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;

			if (debug)
			{
				Popcron.Gizmos.Line(this.root.position, (base.m_handPos + new Vector3(0, -0.04f, 0)), Color.magenta);
				Popcron.Gizmos.Line(this.root.position, lhs, Color.green);
				Popcron.Gizmos.Line(this.root.position, vector, Color.red);
				Popcron.Gizmos.Line(this.root.position, Vector3.Cross(lhs, vector), Color.blue);
			}

			if (Mathf.Abs(this.pos - this.lowerLimit) < 2f)
			{
				this.pos = this.lowerLimit;
			}
			if (Mathf.Abs(this.pos - this.upperLimit) < 2f || Mathf.Abs(this.pos - this.upperLimit) > 200f)
			{
				this.pos = this.upperLimit;

			}
			if (this.pos >= this.lowerLimit && this.pos <= this.upperLimit)
			{
				switch (direction)
				{
					case Direction.X:
						this.objectToMove.localEulerAngles = new Vector3(this.pos, 0f, 0f);
						break;
					case Direction.Y:
						this.objectToMove.localEulerAngles = new Vector3(0f, this.pos, 0f);
						break;
					case Direction.Z:
						this.objectToMove.localEulerAngles = new Vector3(0f, 0f, this.pos);
						break;
					default:
						break;
				}
				if (audioSource != null)
				{
					float lerp = Mathf.InverseLerp(this.lowerLimit, this.upperLimit, this.pos);
					CheckSound(lerp);
				}
			}
		}

		private void CheckSound(float lerp)
		{
			if (lerp < limitWiggleRoom)
			{
				this.state = State.Closed;
				this.rodClosed = false;

			}
			else if (lerp > 1f - limitWiggleRoom)
			{
				this.state = State.Open;
				this.rodClosed = true;
			}
			else
			{
				this.state = State.Mid;
			}
			if (this.state == State.Open && this.last_state != State.Open)
			{
				audioSource.PlayOneShot(openSound);
				{
					this.m_hand.Buzz(this.m_hand.Buzzer.Buzz_OnMenuOption);
				}
			}
			if (this.state == State.Closed && this.last_state != State.Closed)
			{
				audioSource.PlayOneShot(closeSound);
				{
					this.m_hand.Buzz(this.m_hand.Buzzer.Buzz_OnMenuOption);
				}
			}
			this.last_state = this.state;
		}

#endif
	}
}
