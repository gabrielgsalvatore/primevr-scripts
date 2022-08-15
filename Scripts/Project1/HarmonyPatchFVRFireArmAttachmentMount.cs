using HarmonyLib;
using FistVR;
using UnityEngine;

namespace PrimeVrScripts
{
    [HarmonyPatch(typeof(FVRFireArmAttachmentMount), "isMountableOn")]
    class PatchFvrFireArmAttachment
    {
#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        [HarmonyPrefix]
        static bool isMountableOn(ref bool __result, FVRFireArmAttachment possibleAttachment, FVRFireArmAttachmentMount __instance)
        {
            if (__instance.GetRootMount().MyObject is DoubleActionLoadingGateRevolver)
            {
                __result = !((Object)__instance.Parent == (Object)null) &&
            __instance.AttachmentsList.Count < __instance.m_maxAttachments &&
            (!((Object)possibleAttachment.AttachmentInterface != (Object)null) ||
            !(possibleAttachment.AttachmentInterface is AttachableBipodInterface) ||
            !((Object)__instance.GetRootMount().MyObject.Bipod != (Object)null))
            && (!(possibleAttachment is Suppressor) ||
            (!(__instance.GetRootMount().MyObject is SingleActionRevolver)
            || (__instance.GetRootMount().MyObject as SingleActionRevolver).AllowsSuppressor)
            && (!(__instance.GetRootMount().MyObject is Revolver) ||
            (__instance.GetRootMount().MyObject as Revolver).AllowsSuppressor) &&
            (!(__instance.GetRootMount().MyObject is DoubleActionLoadingGateRevolver) ||
            (__instance.GetRootMount().MyObject as DoubleActionLoadingGateRevolver).AllowsSuppressor)) &&
            (!(possibleAttachment is AttachableMeleeWeapon) ||
            !(__instance.GetRootMount().MyObject is FVRFireArm) ||
            !((Object)(__instance.GetRootMount().MyObject as FVRFireArm)
            .CurrentAttachableMeleeWeapon != (Object)null));
                return false;
            }
            return true;
        }
#endif

    }
}
