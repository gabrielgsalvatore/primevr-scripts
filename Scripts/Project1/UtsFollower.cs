using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class UtsFollower : MonoBehaviour
    {
        public GameObject FollowerSpring;
        public GameObject FollowerTip;
        public GameObject Lever;
        public Vector3[] SpringScaleDePressed;
        public Vector3[] SpringScalePressed;
        public Vector3[] TipPositionDePressed;
        public Vector3[] TipPositionPressed;
        public Vector3[] LeverPositionDePressed;
        public Vector3[] LeverPositionPressed;
        public UtsMagazine tubeMagazine;
        public FVRFireArmMagazineReloadTrigger reloadTrigger;
        public bool isSpringPressed;
        public int ammoCount;

        public void Awake()
        {
        }

        public void FixedUpdate()
        {
            //this.FollowerSpring.transform.localScale = Vector3.Lerp(this.FollowerSpring.transform.localScale , SpringScale[tubeMagazine.LoadedRounds.Length], 0.5f);
            //this.FollowerTip.transform.localPosition = Vector3.Lerp(this.FollowerTip.transform.localPosition, TipPosition [tubeMagazine.LoadedRounds.Length], 0.5f);
            ammoCount = tubeMagazine.m_numRounds;
            this.FollowerSpring.transform.localScale = Vector3.Lerp(this.FollowerSpring.transform.localScale, this.isSpringPressed ? SpringScalePressed[ammoCount] : SpringScaleDePressed[ammoCount], 0.5f);
            this.FollowerTip.transform.localPosition = Vector3.Lerp(this.FollowerTip.transform.localPosition, this.isSpringPressed ? TipPositionPressed[ammoCount] : TipPositionDePressed[ammoCount], 0.5f);
            this.Lever.transform.localPosition = Vector3.Lerp(this.Lever.transform.localPosition, this.isSpringPressed ? LeverPositionPressed[ammoCount] : LeverPositionDePressed[ammoCount], 0.5f);
        }
    }
}
