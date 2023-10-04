using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bodyAngle{
    [System.Serializable]
    public class bodyAngle
    {
        public Transform top;
        public Transform middle;
        public Transform bottom;

        public float getAngle()
        {
            Vector3 upperVec = top.position - middle.position;
            Vector3 lowerVec = bottom.position - middle.position;
            return Vector3.Angle(upperVec, lowerVec);
        }

    }
}
