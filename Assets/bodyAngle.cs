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

        List<double> refDataList = new List<double>();

        // Calculate the angle
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

        // Used to send the message of bad form for this joint
        public void printMsg()
        {
            Debug.Log(badFormMsg);
        }

        // Store the angle within this frame
        public void saveData(int ndx) 
        {
            this.liveData[ndx] = this.getAngle();
        }

        public void checkFrame()
        {
            this.avgErrors.Sort(); // Sort the list from least to greatest
            double sumError = 0;
            int totalCounted = 0;
            double avgError = 0;
            if (avgErrors.Count < 3) // Can't calculate iqr for data less than 3
            {
                foreach (double error in this.avgErrors)
                {
                    sumError += error;
                    totalCounted++;
                }
                avgError = sumError / (double) totalCounted;
                Debug.Log("Avg Error " + this.jointName + ": " + avgError);
                return;
            }
            int midNdx = this.avgErrors.Count / 2; // Median index
            int lowerNdx = this.avgErrors.Count / 4; // Lower quartile index
            int upperNdx = 3 * (this.avgErrors.Count / 4); // Upper quartile index
            double iqr = this.avgErrors[upperNdx] - this.avgErrors[lowerNdx]; // Interquartile range
            double eps = this.avgErrors[midNdx] + (1.5 * iqr); // Outliers are larger than eps (maybe should change this to single sided iqr)
            
            foreach (double error in this.avgErrors)
            {
                if (error <= eps)
                {
                    sumError += error; // Only count errors that are within our range
                    totalCounted++;
                }
            }
            avgError = sumError / (double) totalCounted;
            Debug.Log("Avg Error " + this.jointName + ": " + avgError);
            //Underneath here is where you would call a function for processing an error
            //if (avgError > this.errorMargin)
            //{
            //    Debug.Log(this.badFormMsg);
            //}
        }

        // Save data to the reference list
        public void saveRef()
        {
            this.refDataList.Add(this.getAngle());
        }

        // Write the reference list
        public void storeReference() {
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt", true))
            {
                foreach (double value in this.refDataList)
                {
                    sw.Write(value + " ");
                }
            }
        }

        // Wipe a reference data file
        public void resetFile() {
            this.refDataList.Clear();
            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/ReferenceAngles/Squat/" + this.jointName + ".txt"))
            {
                sw.Write("");
            }
        }

        // Read the reference data from a file
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
    }
}
