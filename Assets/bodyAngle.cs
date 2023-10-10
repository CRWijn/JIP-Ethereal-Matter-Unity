using System;
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

        [System.NonSerialized]
        public List<double> avgErrors = new List<double>();

        public float getAngle()
        {
            Vector3 lowerVec;
            if (middle == bottom) {
                lowerVec = new Vector3(1, 1, 1);
            }
            else {
                lowerVec = bottom.position - middle.position;
            }
            Vector3 upperVec = top.position - middle.position;
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

        public void checkFrame()
        {
            this.avgErrors.Sort(); // Sort the list from least to greatest
            int midNdx = this.avgErrors.Count / 2;
            int lowerNdx = this.avgErrors.Count / 4;
            int upperNdx = 3 * this.avgErrors.Count / 4;
            double iqr = this.avgErrors[upperNdx] - this.avgErrors[lowerNdx];
            double eps = this.avgErrors[midNdx] + (1.5 * iqr); // Outliers are larger than eps (maybe should change this to single sided iqr)
            double sumError = 0;
            int totalCounted = 0;
            foreach (double error in this.avgErrors)
            {
                if (error < eps)
                {
                    sumError += error;
                    totalCounted++;
                }
            }
            double avgError = sumError / (double) totalCounted;
            Debug.Log("Avg Error: " + avgError);
            if (avgError > this.errorMargin)
            {
                Debug.Log(this.badFormMsg);
            }
        }

        public void saveRef(int ndx)
        {
            this.refData[ndx] = this.getAngle();
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
            using (StreamWriter sw = new StreamWriter(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt"))
            {
                sw.Write("");
            }
        }

        public void readReference() {
            string path = Directory.GetCurrentDirectory();
            try 
            {
                using (StreamReader sr = new StreamReader(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt"))
                {
                    List<double> refDataReader = new List<double>();
                    double current = 0;
                    double bfrDec = 0.1;
                    while (true) 
                    {
                        int query = sr.Read();
                        if ((char) query == ' ') // End of a number
                        {
                            refDataReader.Add(current);
                            current = 0;
                            bfrDec = 0.1;
                        }
                        else if (query == -1) // End of the file
                        {
                            break;
                        }
                        else if ((char) query == (char) '.' || (char) query ==  ',') // Passing the decimal -> , and . for support for dutch and english unity
                        {
                            bfrDec = 10;
                        }
                        else // Another digit
                        {
                            int digit = (char) query - '0';
                            if (bfrDec == 0.1) // Tens
                            {
                                current = (current / bfrDec) + (double) digit;
                            }
                            else // Tenths
                            {
                                current += (double) digit / bfrDec;
                                bfrDec *= 10;
                            }
                        }
                    }
                    int refDataLen = refDataReader.Count;
                    Array.Resize<double>(ref this.refData, refDataLen);
                    for (int i = 0; i < refDataLen; i++)
                    {
                        this.refData[i] = refDataReader[i];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw new InvalidOperationException("Must save data first!");
            }
        }

        public void dump()
        {
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/DTW Investigation/DUMP_" + this.jointName + ".txt"))
            {
                for (int i = 0; i < this.liveData.Length; i++) {
                    sw.Write(this.liveData[i]);
                    if (i < this.liveData.Length - 1)
                    {
                        sw.Write(' ');
                    }
                }
                sw.Write('\n');
                for (int i = 0; i < this.refData.Length; i++) {
                    sw.Write(this.refData[i]);
                    if (i < this.refData.Length - 1)
                    {
                        sw.Write(' ');
                    }
                }
            }
        }
    }
}
