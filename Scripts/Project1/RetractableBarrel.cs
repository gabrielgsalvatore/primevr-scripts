using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class RetractableBarrel : FVRInteractiveObject
    {

        private Vector3 objectStartPos;
        private Vector3 handStartPos;
        public Vector3[] chambersStartPos = new Vector3[6];

        public Transform objectToMove;
        public Transform tempParent;
        public FVRFireArm fireArm;


        private float previousRot;
        private bool isRotating;
        private bool isRotatingBack;
        public float endLimit;
        public float startLimit;
        public bool isClosed;

        public SingleActionRevolverCylinder cylinder;

        private float m_handZOffset;
        public float m_chamberZOffset;
        private float m_slideZ_heldTarget;
        private float m_slideZ_rear;
        private float m_slideZ_forward;
        private float m_slideZ_current;
        private float m_curSlideSpeed;
        private float m_slideZ_lock;

        public HandgunSlide.SlidePos CurPos;
        public HandgunSlide.SlidePos LastPos;
        public float Speed_Forward;
        public float Speed_Rearward;
        public float Speed_Held;
        public float SpringStiffness = 5f;
        public Transform Point_Slide_Forward;
        public Transform Point_Slide_LockPoint;
        public Transform Point_Slide_Rear;
        public bool canRotate;
        public double chambersStartZPosition;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)

        public void FixedUpdate()
        {
            if (this.isRotatingBack)
            {
                var stepRearwards = 600f * Time.fixedDeltaTime;
                this.objectToMove.Rotate(0f, 0f, -stepRearwards);
                if (this.objectToMove.localEulerAngles.z <= 0f || this.objectToMove.localEulerAngles.z >= 180f)
                {
                    this.objectToMove.localEulerAngles = new Vector3(this.objectToMove.localEulerAngles.x, this.objectToMove.localEulerAngles.y, 0f);
                    this.isRotatingBack = false;
                    this.fireArm.PlayAudioEvent(FirearmAudioEventType.BipodClosed);
                }
            }
        }

        public override void FVRFixedUpdate()
        {
            base.FVRFixedUpdate();
            if (this.isRotating)
            {
                var step = 600f * Time.fixedDeltaTime;
                if (this.objectToMove.localEulerAngles.z >= 90f && this.objectToMove.localEulerAngles.z < 180f)
                {
                    this.isRotating = false;
                    this.objectToMove.localEulerAngles = new Vector3(this.objectToMove.localEulerAngles.x, this.objectToMove.localEulerAngles.y, 90f);
                }
                this.objectToMove.Rotate(0f, 0f, step);
            }
        }

        public override void Awake()
        {
            base.Awake();
            this.canRotate = true;
            this.m_slideZ_current = this.transform.localPosition.z;
            this.m_slideZ_forward = this.Point_Slide_Forward.localPosition.z;
            this.m_slideZ_lock = this.Point_Slide_LockPoint.localPosition.z;
            this.m_slideZ_rear = this.Point_Slide_Rear.localPosition.z;
            this.isRotating = false;
            this.isRotatingBack = false;
            this.isClosed = true;
        }

        public void UpdateBarrelPosition()
        {
            bool flag = false;
            if (this.IsHeld)
                flag = true;
            if (this.IsHeld)
            {
                this.m_slideZ_heldTarget = this.fireArm.transform.InverseTransformPoint(this.GetClosestValidPoint(this.Point_Slide_Rear.position, this.Point_Slide_Forward.position, this.m_hand.Input.Pos + (-this.transform.forward * m_handZOffset * this.fireArm.transform.localScale.x))).z;
            }
            Vector2 vector2 = new Vector2(this.m_slideZ_rear, this.m_slideZ_forward);
            if (flag)
                this.m_curSlideSpeed = 0.0f;
            else if (this.CurPos < HandgunSlide.SlidePos.LockedToRear && (double)this.m_curSlideSpeed >= 0.0 || this.LastPos >= HandgunSlide.SlidePos.Rear)
                this.m_curSlideSpeed = Mathf.MoveTowards(this.m_curSlideSpeed, this.Speed_Forward, Time.deltaTime * this.SpringStiffness);
            float num = Mathf.Clamp(!flag ? this.m_slideZ_current + this.m_curSlideSpeed * Time.deltaTime : Mathf.MoveTowards(this.m_slideZ_current, this.m_slideZ_heldTarget, this.Speed_Held * Time.deltaTime), vector2.x, vector2.y);
            if ((double)Mathf.Abs(num - this.m_slideZ_current) > (double)Mathf.Epsilon)
            {
                this.m_slideZ_current = num;
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.m_slideZ_current);
                for (int index = 0; index < this.cylinder.Chambers.Length; ++index)
                {
                    if (this.cylinder.Chambers[index].IsSpent)
                    {
                        this.cylinder.Chambers[index].transform.localPosition = new Vector3(this.cylinder.Chambers[index].transform.localPosition.x, this.cylinder.Chambers[index].transform.localPosition.y, (-this.m_slideZ_current) - this.m_chamberZOffset);
                    }
                }

            }
            else
                this.m_curSlideSpeed = 0.0f;
            HandgunSlide.SlidePos curPos1 = this.CurPos;
            HandgunSlide.SlidePos slidePos = (double)Mathf.Abs(this.m_slideZ_current - this.m_slideZ_forward) >= 1.0 / 1000.0 ? ((double)Mathf.Abs(this.m_slideZ_current - this.m_slideZ_lock) >= 1.0 / 1000.0 ? ((double)Mathf.Abs(this.m_slideZ_current - this.m_slideZ_rear) >= 1.0 / 1000.0 ? ((double)this.m_slideZ_current <= (double)this.m_slideZ_lock ? HandgunSlide.SlidePos.LockedToRear : HandgunSlide.SlidePos.ForwardToMid) : HandgunSlide.SlidePos.Rear) : HandgunSlide.SlidePos.Locked) : HandgunSlide.SlidePos.Forward;
            int curPos2 = (int)this.CurPos;
            this.CurPos = (HandgunSlide.SlidePos)Mathf.Clamp((int)slidePos, curPos2 - 1, curPos2 + 1);

            if (this.CurPos == HandgunSlide.SlidePos.Rear && this.LastPos != HandgunSlide.SlidePos.Rear)
            {
                this.isClosed = true;
            }
            else if (this.CurPos != HandgunSlide.SlidePos.Rear && this.LastPos == HandgunSlide.SlidePos.Rear)
            {
                this.isClosed = false;
            }
            this.LastPos = this.CurPos;
        }
        public Vector3 CustomGetClosestValidPoint(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            Vector3 rhs = vPoint - vA;
            Vector3 normalized = (vB - vA).normalized;
            float num1 = Vector3.Distance(vA, vB);
            float num2 = Vector3.Dot(normalized, rhs);
            if (num2 <= 0.0)
                return vA;
            if (num2 >= num1)
                return vB;
            Vector3 vector3 = normalized * num2;
            return vA + vector3;
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            if (!canRotate)
                return;
            this.UpdateBarrelPosition();
        }

        public override void EndInteraction(FVRViveHand hand)
        {
            base.EndInteraction(hand);
            
            if (!canRotate)
                return;
            if (this.isClosed)
            {

                for (int index = 0; index < this.cylinder.Chambers.Length; ++index)
                {
                    this.cylinder.Chambers[index].transform.localPosition = new Vector3(this.cylinder.Chambers[index].transform.localPosition.x, this.cylinder.Chambers[index].transform.localPosition.y, ((float)this.chambersStartZPosition));
                }
                this.isRotating = false;
                this.isRotatingBack = true;
            }
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            if (!canRotate)
                return;
            this.isRotatingBack = false;
            this.isRotating = true;
            this.fireArm.PlayAudioEvent(FirearmAudioEventType.BipodOpen);
            this.m_handZOffset = this.objectToMove.InverseTransformPoint(hand.Input.Pos).z;
            base.BeginInteraction(hand);

        }
#endif
    }
}
