using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class UtsTubeSwitch : FVRInteractiveObject
    {
        public bool isOpen;
        public AudioEvent switchFlick;
        public GameObject switchGameObject;
        public Vector3 leftTubeSwitchRotation;
        public Vector3 rightTubeSwitchRotation;
        public Vector3 middleTubeSwitchRotation;
        public UtsFollower follower;
        public UtsShotgun shotgun;

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            if(this.shotgun.tubeSwitchPosition == 2)
            {
                this.shotgun.tubeSwitchPosition = 0;
            }
            else
            {
                this.shotgun.tubeSwitchPosition ++;
            }
            SM.PlayGenericSound(switchFlick, transform.position);
            switch (this.shotgun.tubeSwitchPosition )
            {
                case 0:
                    //Left Tube
                    this.switchGameObject.transform.localEulerAngles = this.leftTubeSwitchRotation;
                    this.shotgun.Magazine = this.shotgun.leftTubeMagazine;
                    break;
                case 1:
                    //Both
                    this.switchGameObject.transform.localEulerAngles = this.middleTubeSwitchRotation;
                    break;
                case 2:
                    //Right Tube
                    this.shotgun.Magazine = this.shotgun.rightTubeMagazine;
                    this.switchGameObject.transform.localEulerAngles = this.rightTubeSwitchRotation;
                    break;
            }
        }
    }
}
