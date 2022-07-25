using FistVR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PrimeVrScripts
{
    public class LematMk2 : FVRFireArm
    {
		[Header("Lemat Revolver Config")]

		private SingleActionRevolverCylinder LematCylinder;
		private Vector3 foreStartPos;
		private bool isShotgunActivated = false;

		public Transform hammerSwitch;
		public float hammerSwitchUnflipped = 0;
		public float hammerSwitchFlipped = 0;
		public float hammerUncockedFlipped = 0;
		public float hammerUncockedUnflipped = 0;

		public FVRFireArmChamber shotgunChamber;

		public Transform shotgunMuzzle;
		public Transform regularMuzzle;
		public Transform overrideMuzzle;

		public FVRFirearmAudioSet regularAudio;
		public FVRFirearmAudioSet shotgunAudio;

		public FVRFireArmRecoilProfile shotgunRecoilProfile;
		[Header("Lemat Revolver Hinge Config")]
		public HingeJoint hinge;
		public float hingeLimit = 45f;
		public float hingeEjectLimit = 30f;
		public bool isLatched = true;
		public float EjectOffset = -0.04f;
		public float EjectSpeed = -3.0f;
		public bool IsLatchHeldOpen;
		public float latchRot;
		public bool isExternallyUnlatched = false;
		[Header("Single Action Revolver")]
		public bool AllowsSuppressor;

		public Transform Hammer;

		public Transform LoadingGate;

		public Transform Trigger;

		public Transform EjectorRod;

		public SingleActionRevolverCylinder Cylinder;

		public Transform HammerFanDir;

		private int m_curChamber;

		private float m_curChamberLerp;

		private float m_tarChamberLerp;

		[Header("Component Movement Params")]
		public float Hammer_Rot_Uncocked;

		public float Hammer_Rot_Halfcocked;

		public float Hammer_Rot_Cocked;

		public float LoadingGate_Rot_Closed;

		public float LoadingGate_Rot_Open;

		public float Trigger_Rot_Forward;

		public float Trigger_Rot_Rearward;

		public Vector3 EjectorRod_Pos_Forward;

		public Vector3 EjectorRod_Pos_Rearward;

		public bool DoesCylinderTranslateForward;

		public bool DoesHalfCockHalfRotCylinder;

		public bool HasTransferBarSafety;

		public bool IsAccessTwoChambersBack;

		public Vector3 CylinderBackPos;

		public Vector3 CylinderFrontPos;

		[Header("Spinning Config")]
		public Transform PoseSpinHolder;

		public bool CanSpin;

		private bool m_isSpinning;

		[Header("StateToggling")]
		public bool StateToggles;

		private bool m_isStateToggled;

		public Transform Pose_Main;

		public Transform Pose_Toggled;

		public float TriggerThreshold;

		private float m_triggerFloat;

		private bool m_isHammerCocking;

		private bool m_isHammerCocked;

		private float m_hammerCockLerp;

		private float m_hammerCockSpeed;

		private float xSpinRot;

		private float xSpinVel;

		private float timeSinceColFire;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
		public LematMk2()
		{
			this.CanSpin = true;
			this.StateToggles = true;
			this.TriggerThreshold = 0.9f;
			this.m_hammerCockSpeed = 10f;
		}

		public int CurChamber
		{
			get
			{
				return this.m_curChamber;
			}
			set
			{
				this.m_curChamber = value % this.Cylinder.NumChambers;
			}
		}

		public int NextChamber
		{
			get
			{
				return (this.m_curChamber + 1) % this.Cylinder.NumChambers;
			}
		}

		public int PrevChamber
		{
			get
			{
				int num = this.m_curChamber - 1;
				if (num < 0)
				{
					return this.Cylinder.NumChambers - 1;
				}
				return num;
			}
		}

		public int PrevChamber2
		{
			get
			{
				int num = this.m_curChamber - 2;
				if (num < 0)
				{
					return this.Cylinder.NumChambers + num;
				}
				return num;
			}
		}

		public override void Awake()
		{
			base.Awake();
			foreach (FVRFireArmChamber fvrfireArmChamber in this.Cylinder.Chambers)
			{
				this.FChambers.Add(fvrfireArmChamber);
			}
			if (this.PoseOverride_Touch != null)
			{
				this.Pose_Main.localPosition = this.PoseOverride_Touch.localPosition;
				this.Pose_Main.localRotation = this.PoseOverride_Touch.localRotation;
			}
			this.foreStartPos = this.hinge.transform.localPosition;
		}

		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			if (!this.IsAltHeld)
			{
				if (!this.m_isStateToggled)
				{
					this.PoseOverride.localPosition = this.Pose_Main.localPosition;
					this.PoseOverride.localRotation = this.Pose_Main.localRotation;
					if (this.m_grabPointTransform != null)
					{
						this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
					}
				}
				else
				{
					this.PoseOverride.localPosition = this.Pose_Toggled.localPosition;
					this.PoseOverride.localRotation = this.Pose_Toggled.localRotation;
					if (this.m_grabPointTransform != null)
					{
						this.m_grabPointTransform.localPosition = this.Pose_Toggled.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Toggled.localRotation;
					}
				}
			}
		}

		public void PopOutEmpties()
		{
			if (this.shotgunChamber.IsFull && this.shotgunChamber.IsSpent)
			{
				this.PopOutRound(this.shotgunChamber);
			}
		}
		public void PopOutRound(FVRFireArmChamber chamber)
		{
			base.PlayAudioEvent(FirearmAudioEventType.MagazineOut, 1f);
			chamber.EjectRound(chamber.transform.position + chamber.transform.forward * this.EjectOffset, chamber.transform.forward * this.EjectSpeed, Vector3.right, false);
		}

		public IEnumerator activateShotgunIe()
		{
			this.isShotgunActivated = !this.isShotgunActivated;
			this.PlayAudioEvent(FirearmAudioEventType.Safety);
			if (this.isShotgunActivated)
			{
				this.hammerSwitch.localEulerAngles = new Vector3(0f, 0f, this.hammerSwitchFlipped);
				this.Hammer_Rot_Uncocked = hammerUncockedFlipped;
			}
			else
			{
				this.hammerSwitch.localEulerAngles = new Vector3(0f, 0f, hammerSwitchUnflipped);
				this.Hammer_Rot_Uncocked = hammerUncockedUnflipped;
			}
			yield return new WaitForSeconds(1f);
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{

			this.IsLatchHeldOpen = false;
			if (this.IsAltHeld || !this.isLatched)
			{
				return;
			}
			if (hand.IsInStreamlinedMode)
			{
				if (hand.Input.BYButtonDown && this.isShotgunActivated)
				{
					this.IsLatchHeldOpen = true;
				}
				if (hand.Input.AXButtonDown && m_hammerCockLerp == 1)
				{
					this.activateShotgun();
				}
			}
			else
			{
				if (hand.Input.TouchpadDown && hand.Input.TouchpadNorthPressed && m_hammerCockLerp == 0 && !m_isStateToggled)
				{
					this.IsLatchHeldOpen = true;
				}
				if (hand.Input.TouchpadDown && hand.Input.TouchpadNorthPressed && m_hammerCockLerp == 1)
				{
					this.activateShotgun();
				}
			}
			base.UpdateInteraction(hand);
			if (!this.IsAltHeld)
			{
				if (!this.m_isStateToggled)
				{
					if (hand.IsInStreamlinedMode)
					{
						if (hand.Input.AXButtonDown)
						{
							this.CockHammer(5f);
						}
						if (hand.Input.BYButtonDown && this.StateToggles && !this.isShotgunActivated)
						{
							this.ToggleState();
							base.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
						}
					}
					else if (hand.Input.TouchpadDown)
					{
						if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
						{
							this.CockHammer(5f);
						}
						else if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && this.StateToggles)
						{
							this.ToggleState();
							base.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
						}
					}
				}
				else
				{
					if (hand.IsInStreamlinedMode)
					{
						if (hand.Input.AXButtonDown)
						{
							this.AdvanceCylinder();
						}
						if (hand.Input.BYButtonDown && this.StateToggles && !this.isShotgunActivated)
						{
							this.ToggleState();
							base.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
						}
					}
					else if (hand.Input.TouchpadDown)
					{
						if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && this.StateToggles)
						{
							this.ToggleState();
							base.PlayAudioEvent(FirearmAudioEventType.BreachClose, 1f);
						}
						else if (Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
						{
							this.AdvanceCylinder();
						}
					}
					if (hand.Input.TriggerDown)
					{
						this.EjectPrevCylinder();
					}
				}
			}
			this.UpdateTriggerHammer();
			this.UpdateCylinderRot();
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			this.m_triggerFloat = 0f;
			base.EndInteraction(hand);
			base.RootRigidbody.AddRelativeTorque(new Vector3(this.xSpinVel, 0f, 0f), ForceMode.Impulse);
		}

		public void activateShotgun()
		{
			StartCoroutine(activateShotgunIe());
		}

		public override void FVRFixedUpdate()
		{
			if (this.timeSinceColFire < 3f)
			{
				this.timeSinceColFire += Time.deltaTime;
			}
			base.FVRFixedUpdate();

			if (this.isLatched && (this.IsLatchHeldOpen || this.isExternallyUnlatched))
			{
				this.isLatched = false;
				base.PlayAudioEvent(FirearmAudioEventType.TopCoverDown, 1f);
				JointLimits limits = this.hinge.limits;
				limits.max = this.hingeLimit;
				this.hinge.limits = limits;
				this.shotgunChamber.IsAccessible = true;
			}
			if (!this.isLatched)
			{
				if (!this.IsLatchHeldOpen && this.hinge.transform.localEulerAngles.x <= 1f && !this.isExternallyUnlatched)
				{
					this.isLatched = true;
					base.PlayAudioEvent(FirearmAudioEventType.TopCoverUp, 1f);
					JointLimits limits2 = this.hinge.limits;
					limits2.max = 0f;
					this.hinge.limits = limits2;
					this.shotgunChamber.IsAccessible = false;
					this.hinge.transform.localPosition = this.foreStartPos;
				}
				if (Mathf.Abs(this.hinge.transform.localEulerAngles.x) >= this.hingeEjectLimit)
				{
					this.PopOutEmpties();
				}
			}
		}

		private void UpdateTriggerHammer()
		{
			if (base.IsHeld && !this.m_isStateToggled && !this.m_isHammerCocked && !this.m_isHammerCocking && this.m_hand.OtherHand != null)
			{
				Vector3 velLinearWorld = this.m_hand.OtherHand.Input.VelLinearWorld;
				float num = Vector3.Distance(this.m_hand.OtherHand.PalmTransform.position, this.HammerFanDir.position);
				if (num < 0.15f && Vector3.Angle(velLinearWorld, this.HammerFanDir.forward) < 60f && velLinearWorld.magnitude > 1f)
				{
					this.CockHammer(10f);
				}
			}
			if (this.m_isHammerCocking)
			{
				if (this.m_hammerCockLerp < 1f)
				{
					this.m_hammerCockLerp += Time.deltaTime * this.m_hammerCockSpeed;
				}
				else
				{
					this.m_hammerCockLerp = 1f;
					this.m_isHammerCocking = false;
					this.m_isHammerCocked = true;
					this.CurChamber++;
					this.m_curChamberLerp = 0f;
					this.m_tarChamberLerp = 0f;
				}
			}
			if (!this.m_isStateToggled)
			{
				this.Hammer.localEulerAngles = new Vector3(Mathf.Lerp(this.Hammer_Rot_Uncocked, this.Hammer_Rot_Cocked, this.m_hammerCockLerp), 0f, 0f);
			}
			else
			{
				this.Hammer.localEulerAngles = new Vector3(this.Hammer_Rot_Halfcocked, 0f, 0f);
			}
			if (this.LoadingGate != null)
			{
				if (!this.m_isStateToggled)
				{
					this.LoadingGate.localEulerAngles = new Vector3(0f, 0f, this.LoadingGate_Rot_Closed);
				}
				else
				{
					this.LoadingGate.localEulerAngles = new Vector3(0f, 0f, this.LoadingGate_Rot_Open);
				}
			}
			this.m_triggerFloat = 0f;
			if (this.m_hasTriggeredUpSinceBegin && !this.m_isSpinning && !this.m_isStateToggled)
			{
				this.m_triggerFloat = this.m_hand.Input.TriggerFloat;
			}
			this.Trigger.localEulerAngles = new Vector3(Mathf.Lerp(this.Trigger_Rot_Forward, this.Trigger_Rot_Rearward, this.m_triggerFloat), 0f, 0f);
			if (this.m_triggerFloat > this.TriggerThreshold)
			{
				this.DropHammer();
			}
		}

		private void DropHammer()
		{
			if (this.m_isHammerCocked)
			{
				this.m_isHammerCocked = false;
				this.m_isHammerCocking = false;
				this.m_hammerCockLerp = 0f;
				this.Fire();
			}
		}

		private void AdvanceCylinder()
		{
			this.CurChamber++;
			base.PlayAudioEvent(FirearmAudioEventType.FireSelector, 1f);
		}

		public void EjectPrevCylinder()
		{
			if (this.m_isStateToggled)
			{
				int num = this.PrevChamber;
				if (this.IsAccessTwoChambersBack)
				{
					num = this.PrevChamber2;
				}
				FVRFireArmChamber fvrfireArmChamber = this.Cylinder.Chambers[num];
				if (fvrfireArmChamber.IsFull)
				{
					base.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
				}
				fvrfireArmChamber.EjectRound(fvrfireArmChamber.transform.position + fvrfireArmChamber.transform.forward * 0.0025f, -fvrfireArmChamber.transform.forward, Vector3.zero, false);
			}
		}

		private void Fire()
		{
			base.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
            if (this.isShotgunActivated)
            {
				if (this.shotgunChamber.Fire())
				{
					this.AudioClipSet = this.shotgunAudio;
					this.overrideMuzzle.position = this.shotgunMuzzle.position;
					this.GasOutEffects[2].MaxGasRate = 12;
					this.GasOutEffects[1].MaxGasRate = 0;
					FVRFireArmChamber fvrfireArmChamber = this.shotgunChamber;
					base.Fire(fvrfireArmChamber, shotgunMuzzle, true, 1f, -1f);
					this.FireMuzzleSmoke();
					bool flag = this.IsTwoHandStabilized();
					bool flag2 = base.AltGrip != null;
					bool flag3 = this.IsShoulderStabilized();
					this.Recoil(flag, flag2, flag3, this.shotgunRecoilProfile, 1f);
					FVRFireArmRound round = fvrfireArmChamber.GetRound();
					base.PlayAudioGunShot(fvrfireArmChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
					if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
					{
						fvrfireArmChamber.IsSpent = false;
						fvrfireArmChamber.UpdateProxyDisplay();
					}
				}

			} else
            {
				if (this.Cylinder.Chambers[this.CurChamber].Fire())
				{
					this.AudioClipSet = this.regularAudio;
					this.overrideMuzzle.position = this.regularMuzzle.position;
					this.GasOutEffects[2].MaxGasRate = 0;
					this.GasOutEffects[1].MaxGasRate = 12;
					FVRFireArmChamber fvrfireArmChamber = this.Cylinder.Chambers[this.CurChamber];
					base.Fire(fvrfireArmChamber, regularMuzzle, true, 1f, -1f);
					this.FireMuzzleSmoke();
					bool flag = this.IsTwoHandStabilized();
					bool flag2 = base.AltGrip != null;
					bool flag3 = this.IsShoulderStabilized();
					this.Recoil(flag, flag2, flag3, null, 1f);
					base.PlayAudioGunShot(fvrfireArmChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
					if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
					{
						fvrfireArmChamber.IsSpent = false;
						fvrfireArmChamber.UpdateProxyDisplay();
					}
				}
			}
			this.AudioClipSet = this.regularAudio;

		}

		private void UpdateCylinderRot()
		{
			if (this.m_isStateToggled)
			{
				int num = this.PrevChamber;
				if (this.IsAccessTwoChambersBack)
				{
					num = this.PrevChamber2;
				}
				for (int i = 0; i < this.Cylinder.Chambers.Length; i++)
				{
					if (i == num)
					{
						this.Cylinder.Chambers[i].IsAccessible = true;
					}
					else
					{
						this.Cylinder.Chambers[i].IsAccessible = false;
					}
				}
				if (this.DoesHalfCockHalfRotCylinder)
				{
					int num2 = (this.CurChamber + 1) % this.Cylinder.NumChambers;
					this.Cylinder.transform.localRotation = Quaternion.Slerp(this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber), this.Cylinder.GetLocalRotationFromCylinder(num2), 0.5f);
				}
				else
				{
					this.Cylinder.transform.localRotation = this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber);
				}
				if (this.DoesCylinderTranslateForward)
				{
					this.Cylinder.transform.localPosition = this.CylinderBackPos;
				}
			}
			else
			{
				for (int j = 0; j < this.Cylinder.Chambers.Length; j++)
				{
					this.Cylinder.Chambers[j].IsAccessible = false;
				}
				if (this.m_isHammerCocking)
				{
					this.m_tarChamberLerp = this.m_hammerCockLerp;
				}
				else
				{
					this.m_tarChamberLerp = 0f;
				}
				this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);
				int num3 = (this.CurChamber + 1) % this.Cylinder.NumChambers;
				this.Cylinder.transform.localRotation = Quaternion.Slerp(this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber), this.Cylinder.GetLocalRotationFromCylinder(num3), this.m_curChamberLerp);
				if (this.DoesCylinderTranslateForward)
				{
					this.Cylinder.transform.localPosition = Vector3.Lerp(this.CylinderBackPos, this.CylinderFrontPos, this.m_hammerCockLerp);
				}
			}
		}

		private void CockHammer(float speed)
		{
			if (!this.m_isHammerCocked && !this.m_isHammerCocking)
			{
				this.m_hammerCockSpeed = speed;
				this.m_isHammerCocking = true;
				base.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
			}
		}

		private void ToggleState()
		{

			this.m_isStateToggled = !this.m_isStateToggled;
			if (!this.IsAltHeld)
			{
				if (!this.m_isStateToggled)
				{
					this.PoseOverride.localPosition = this.Pose_Main.localPosition;
					this.PoseOverride.localRotation = this.Pose_Main.localRotation;
					if (this.m_grabPointTransform != null)
					{
						this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
					}
				}
				else
				{
					this.PoseOverride.localPosition = this.Pose_Toggled.localPosition;
					this.PoseOverride.localRotation = this.Pose_Toggled.localRotation;
					if (this.m_grabPointTransform != null)
					{
						this.m_grabPointTransform.localPosition = this.Pose_Toggled.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Toggled.localRotation;
					}
				}
			}
			this.m_isHammerCocking = false;
			this.m_isHammerCocked = false;
			this.m_hammerCockLerp = 0f;
			if (this.m_isStateToggled)
			{
			}
		}

		public override void OnCollisionEnter(Collision col)
		{
			base.OnCollisionEnter(col);
			if (!this.HasTransferBarSafety && col.collider.attachedRigidbody == null && this.Cylinder.Chambers[this.CurChamber].IsFull && !this.Cylinder.Chambers[this.CurChamber].IsSpent && !this.m_isHammerCocked && !this.m_isHammerCocking && !this.m_isStateToggled && col.relativeVelocity.magnitude > 2f && this.timeSinceColFire > 2.9f)
			{
				this.timeSinceColFire = 0f;
				this.Fire();
			}
		}

		public override List<FireArmRoundClass> GetChamberRoundList()
		{
			bool flag = false;
			List<FireArmRoundClass> list = new List<FireArmRoundClass>();
			for (int i = 0; i < this.Cylinder.Chambers.Length; i++)
			{
				if (this.Cylinder.Chambers[i].IsFull)
				{
					list.Add(this.Cylinder.Chambers[i].GetRound().RoundClass);
					flag = true;
				}
			}
			if (flag)
			{
				return list;
			}
			return null;
		}

		public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
		{
			if (rounds.Count > 0)
			{
				for (int i = 0; i < this.Cylinder.Chambers.Length; i++)
				{
					if (i < rounds.Count)
					{
						this.Cylinder.Chambers[i].Autochamber(rounds[i]);
					}
				}
			}
		}
#endif
	}

}
