using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class SmartLinkTrigger : FVRInteractiveObject
    {

        public Transform smartLinkObject;
        public bool isOpening = false;
        public AudioEvent audioClipTurnOn;
        public AudioEvent audioClipTurnOff;
        public MeshRenderer smartLinkMesh;
        public Material turnedOffMaterial;
        public Material turnedOnMaterial;
#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            this.isOpening = !isOpening;

            if (this.isOpening)
            {
                SM.PlayGenericSound(audioClipTurnOn, transform.position);
                this.smartLinkObject.gameObject.SetActive(true);
                this.smartLinkMesh.material = turnedOnMaterial;
            }
            else
            {
                this.smartLinkMesh.material = turnedOffMaterial;
                this.smartLinkObject.gameObject.SetActive(false);
                SM.PlayGenericSound(audioClipTurnOff, transform.position);
            }
        }

        public override void Awake()
        {
            base.Awake();
            this.isOpening = false;
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
            //var step = 1 * Time.deltaTime;
            //if (isOpening)
            //{
            //    smartLinkObject.localScale = Vector3.Lerp(openedSize, closedSize, step);
            //} else
            //{
            //    smartLinkObject.localScale = Vector3.Lerp(closedSize, openedSize, step);
            //}
        }
#endif
    }
}
