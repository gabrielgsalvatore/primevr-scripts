using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    public class AttachmentMountSOCOM : FVRFireArmAttachmentMount
    {
        public Vector3[] startPosition;
        public Vector3[] alternatePosition;
        public float[] startScale;
        public float[] alternateScale;
        public FVRFireArmAttachmentMount[] attachmentsToMove;
        public GameObject objectToDisable;


#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
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
                    if (attachment.name.Contains("SOCOM-Silencer"))
                    {
                        this.objectToDisable.SetActive(false);
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
                    if (attachment.name.Contains("SOCOM-Silencer"))
                    {
                        this.objectToDisable.SetActive(true);
                    }
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
