using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class AttachmentMountSmartLink : FVRFireArmAttachmentMount
    {
        public FVRFireArmAttachmentMount socomMount;
        public Vector3 alternateScale;


#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            base.Awake();
            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }
        private void Unhook()
        {
            On.FistVR.FVRFireArmAttachmentMount.RegisterAttachment -= this.FVRFireArmAttachmentMount_RegisterAttachment;
            On.FistVR.FVRFireArmAttachmentMount.DeRegisterAttachment -= this.FVRFireArmAttachmentMount_DeRegisterAttachment;
        }

        private void Hook()
        {
            On.FistVR.FVRFireArmAttachmentMount.RegisterAttachment += this.FVRFireArmAttachmentMount_RegisterAttachment;
            On.FistVR.FVRFireArmAttachmentMount.DeRegisterAttachment += this.FVRFireArmAttachmentMount_DeRegisterAttachment;
        }

        private void FVRFireArmAttachmentMount_RegisterAttachment(On.FistVR.FVRFireArmAttachmentMount.orig_RegisterAttachment orig, FVRFireArmAttachmentMount self, FVRFireArmAttachment attachment)
        {
            if (self == this)
            {
                if(attachment != null)
                {
                    if (socomMount.HasAttachmentsOnIt() && socomMount.AttachmentsList[0].name.Contains("SOCOM-Silencer"))
                    {
                         attachment.transform.localScale = this.alternateScale;
                    }
                }
                orig(self, attachment);
            }
            else
            {
                orig(self, attachment);
            }
        }
        private void FVRFireArmAttachmentMount_DeRegisterAttachment(On.FistVR.FVRFireArmAttachmentMount.orig_DeRegisterAttachment orig, FVRFireArmAttachmentMount self, FVRFireArmAttachment attachment)
        {
            if (self == this)
            {
                if(attachment != null)
                {

                }
                orig(self, attachment);
            }
            else
            {
                orig(self, attachment);
            }
        }
#endif
    }
}
