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
        public FVRFireArmAttachmentMount[] attachmentsToMove;
        public FVRFireArmAttachmentMount smartLinkMount;
        public Vector3 smartLinkAlternateScale;
        public Vector3 smartLinkOriginalScale;


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
                    if (attachment.name.Contains("SOCOM-Silencer"))
                    {
                        for(var i = 0; i < this.attachmentsToMove.Length; i++)
                        {
                            this.attachmentsToMove[i].transform.localPosition = this.alternatePosition[i];
                        }
                        if (this.smartLinkMount.HasAttachmentsOnIt())
                        {
                            this.smartLinkMount.AttachmentsList[0].transform.localScale = this.smartLinkAlternateScale;
                        }
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
                        for (var i = 0; i < this.attachmentsToMove.Length; i++)
                        {
                            this.attachmentsToMove[i].transform.localPosition = this.startPosition[i];
                        }
                        if (this.smartLinkMount.HasAttachmentsOnIt())
                        {
                            this.smartLinkMount.AttachmentsList[0].transform.localScale = smartLinkOriginalScale;
                        }
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
