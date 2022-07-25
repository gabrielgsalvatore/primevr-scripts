using System.Collections;
using UnityEngine;
using FistVR;

namespace PrimeVrScripts
{
    public class LematShotgunCartridgeRenderer : MonoBehaviour
    {
        public FVRFireArmChamber chamber;

        public Mesh cartridgeMesh;
        public Material[] cartridgeMaterials;

#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        void LateUpdate()
        {
            if (chamber.m_round != null && !chamber.m_round.IsSpent && chamber.ProxyMesh.mesh != cartridgeMesh)
            {
                chamber.ProxyMesh.mesh = cartridgeMesh;
                chamber.ProxyRenderer.materials = cartridgeMaterials;
            }
        }
#endif
    }
}