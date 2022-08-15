using FistVR;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PrimeVrScripts
{
    public class SingleActionCapBallRevolverCylinder : MonoBehaviour
    {
        public int NumChambers = 6;
        public FVRFireArmChamber[] Chambers;
        public FVRFireArmChamber[] Nipples;
        public bool[] chamberRammed;
        public float[] chamberLastLerp;

        public float unrammedPos;
        public float rammedPos;
#if !(MEATKIT || UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            chamberRammed = new bool[NumChambers];
            chamberLastLerp = new float[NumChambers];
            for (int i = 0; i < NumChambers; i++)
            {
                Chambers[i].transform.localPosition = new Vector3(Chambers[i].transform.localPosition.x, Chambers[i].transform.localPosition.y, unrammedPos);
                chamberRammed[i] = false;
                chamberLastLerp[i] = 0f;
            }
        }

        public void RamChamber(int chamber, float lerp)
        {
            if (!chamberRammed[chamber] && lerp > chamberLastLerp[chamber])
            {
                Vector3 lerpPos = Vector3.Lerp(new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, unrammedPos), new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, rammedPos), lerp);
                Chambers[chamber].transform.localPosition = lerpPos;
                chamberLastLerp[chamber] = lerp;
                if (lerp == 1f)
                {
                    chamberRammed[chamber] = true;
                    chamberLastLerp[chamber] = 0f;
                }
            }
        }

        public bool ChamberRammed(int chamber, bool set = false, bool value = false)
        {
            if (set)
            {
                chamberRammed[chamber] = value;

                if (value)
                {
                    Chambers[chamber].transform.localPosition = new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, rammedPos);
                }
                else Chambers[chamber].transform.localPosition = new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, unrammedPos);
            }
            return chamberRammed[chamber];
        }

        public int GetClosestChamberIndex() => Mathf.CeilToInt(Mathf.Repeat(-this.transform.localEulerAngles.z + (float)(360.0 / (double)this.NumChambers * 0.5), 360f) / (360f / (float)this.NumChambers)) - 1;

        public Quaternion GetLocalRotationFromCylinder(int cylinder) => Quaternion.Euler(new Vector3(0.0f, 0.0f, Mathf.Repeat((float)((double)cylinder * (360.0 / (double)this.NumChambers) * -1.0), 360f)));
#endif

    }
}


