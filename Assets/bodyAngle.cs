using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        [System.NonSerialized]
        public double sumDiff;

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

        public void saveData(int ndx) 
        {
            this.liveData[ndx] = this.getAngle();
        }

        public void checkFrame(int totalLength)
        {
            double averageError = this.sumDiff / (double) totalLength;
        }

        public void saveRef(int ndx)
        {
            this.refData[ndx] = this.getAngle();
            Debug.Log("Saving ref data");
        }

        public void storeReference() {
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt", true))
            {
                foreach (double value in this.refData)
                {
                    sw.Write(value + " ");
                }
            }
        }

        public void resetFile() {
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt", true))
            {
                sw.Write("");
            }
        }
    }
}
