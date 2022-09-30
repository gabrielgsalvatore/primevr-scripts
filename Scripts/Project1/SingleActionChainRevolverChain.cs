using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class SingleActionChainRevolverChain : MonoBehaviour
    {
        public Transform[] ChamberLinks;
        public Transform[] ChambersToMove;
        public FVRFireArmChamber[] Chambers;
        private bool chambersMoving;
        public float speed = 1.0f;
        public bool[] chambersFinishedMoving;
        public int numChambers;
        public bool advanceChamberTest;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        // Use this for initialization
        void Start()
        {
            chambersMoving = false;
            chambersFinishedMoving = new bool[numChambers];
        }
        public void UpdateChainPosition(int targetChamber, float hammerLerp)
        {
            for (int i = 0; i < ChambersToMove.Length; i++)
            {
                var chamberLinkTarget = i - targetChamber;
                if (chamberLinkTarget < 0)
                {
                    chamberLinkTarget = this.ChamberLinks.Length + chamberLinkTarget;
                }
                this.ChambersToMove[i].position = Vector3.Lerp(this.ChambersToMove[i].position, this.ChamberLinks[chamberLinkTarget].position, hammerLerp);
            }
        }

        public void FixedUpdate()
        {
            //if (!chambersMoving)
            //{
            //    for (int i = 0; i < this.ChamberLinks.Length; i++)
            //    {
            //        var step = 1f * Time.deltaTime;
            //        var chamberLinkTarget = i + CurChamber;
            //        if (chamberLinkTarget >= this.ChamberLinks.Length)
            //        {
            //            chamberLinkTarget = (0 + (this.numChambers - chamberLinkTarget)) * -1;
            //        }

            //        this.ChambersToMove[i].position = Vector3.MoveTowards(this.ChambersToMove[i].position, this.ChamberLinks[chamberLinkTarget].position, step);

            //    }
            //}
        }


#endif
    }
}
