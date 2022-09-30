using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class SmartLinkAttachment : FVRFireArmAttachment
    {
        public Vector3 originalScale;
        public SmartLinkTrigger smartLinkTrigger;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            base.Awake();
            Hook();
        }

        public void OnDestroy()
        {
            base.OnDestroy();
            Unhook();
        }
        private void Unhook()
        {
            On.FistVR.FVRFireArmAttachment.DetachFromMount -= this.FVRFireArmAttachment_DetachFromMount;
        }

        private void Hook()
        {
            On.FistVR.FVRFireArmAttachment.DetachFromMount += this.FVRFireArmAttachment_DetachFromMount;
        }

        public override void AttachToMount(FVRFireArmAttachmentMount m, bool playSound)
        {
            base.AttachToMount(m, playSound);
            //this.smartLinkTrigger.isOpening = true;
            //this.smartLinkTrigger.SimpleInteraction(this.m_hand);
        }

        private void FVRFireArmAttachment_DetachFromMount(On.FistVR.FVRFireArmAttachment.orig_DetachFromMount orig, FVRFireArmAttachment self)
        {
            if (self == this)
            {
                orig(self);
                this.transform.localScale = this.originalScale;
                //this.smartLinkTrigger.isOpening = true;
                //this.smartLinkTrigger.SimpleInteraction(this.m_hand);
            }
            else
            {
                orig(self);
            }
        }
#endif
    }
}
