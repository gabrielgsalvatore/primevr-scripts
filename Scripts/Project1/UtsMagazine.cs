using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;
using static Valve.VR.SteamVR_TrackedObject;

namespace PrimeVrScripts
{
    public class UtsMagazine : FVRFireArmMagazine
    {
        [Header("UTS-15 Specifics")]
        public UtsFollower follower;
        public UtsLoadingGate loadingGate;
        public GameObject DisplayRoundsOrigin;
        public GameObject DisplayFinalRound;
        public GameObject DisplayFirstRound;
        public Vector3 DisplayRoundsOriginPressedTransform;
        public Vector3 DisplayRoundsOriginDePressedTransform;
        public Vector3 DisplayRoundsOriginDePressedFullTransform;
        public Vector3 DisplayFinalRoundOriginPressedTransform;
        public Vector3 DisplayFinalRoundOriginDePressedTransform;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            base.Awake();
            this.IsDropInLoadable = true;
            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }
        private void Unhook()
        {
            On.FistVR.FVRFireArmMagazine.AddRound_FVRFireArmRound_bool_bool_bool -= this.FVRFireArmMagazine_AddRound;
        }

        private void Hook()
        {
            On.FistVR.FVRFireArmMagazine.AddRound_FVRFireArmRound_bool_bool_bool += this.FVRFireArmMagazine_AddRound;
        }

        private void FVRFireArmMagazine_AddRound(On.FistVR.FVRFireArmMagazine.orig_AddRound_FVRFireArmRound_bool_bool_bool orig, FVRFireArmMagazine self, FVRFireArmRound round, bool makeSound, bool updateDisplay, bool animate)
        {
            if (self == this)
            {
                if (this.m_numRounds == this.m_capacity - 1)
                {
                    orig(self, round, makeSound, false, false);
                    if(this.m_numRounds == this.m_capacity)
                    {
                        this.DisplayBullets[this.DisplayBullets.Length - 1].SetActive(true);
                        this.DisplayMeshFilters[this.DisplayBullets.Length - 1].mesh = this.LoadedRounds[this.LoadedRounds.Length-1].LR_Mesh;
                        this.DisplayRenderers[this.DisplayBullets.Length - 1].material = this.LoadedRounds[this.LoadedRounds.Length - 1].LR_Material;
                    }
                } else
                {
                    orig(self, round, makeSound, updateDisplay, animate);
                }
                this.follower.isSpringPressed = true;
                this.DisplayRoundsOrigin.transform.localPosition = this.DisplayRoundsOriginPressedTransform;
                this.DisplayFinalRound.transform.localPosition = this.DisplayFinalRoundOriginPressedTransform;
            } else
            {
                orig(self, round, makeSound, updateDisplay, animate);
            }
        }
#endif
    }
}
