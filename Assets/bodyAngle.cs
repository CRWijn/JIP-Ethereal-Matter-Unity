using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bodyAngle{
    [System.Serializable]
    public class bodyAngle
    {
        public string jointName;
        public Transform top;
        public Transform middle;
        public Transform bottom;
        public double errorMargin;
        public string badFormMsg;

        [System.NonSerialized]
        public double[] liveData;

        [System.NonSerialized]
        public double[] refData;

        [System.NonSerialized]
        public double sumLive;

        [System.NonSerialized]
        public double sumRef;

        [System.NonSerialized]
        public double avgLive;

        [System.NonSerialized]
        public double avgRef;



        public float getAngle()
        {
            Vector3 upperVec = top.position - middle.position;
            Vector3 lowerVec = bottom.position - middle.position;
            return Vector3.Angle(upperVec, lowerVec);
        }

        public void printMsg()
        {
            Debug.Log(badFormMsg);
        }

    }
}
