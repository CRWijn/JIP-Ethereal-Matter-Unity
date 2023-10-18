using System;
using UnityEngine;
using System.Collections.Generic; // Import List from System.Collections.Generic
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using DTW;
using bodyAngle;

public class DTWmodelsquat : MonoBehaviour
{
    public bool isRef;
    public bool writeRef;
    public bool toggleRec;
    public bodyAngle.bodyAngle[] joints;
    public int windowSize;
    
    int CounterLive = 0;
    double squatPercentStart = 0;
    double squatPercentEnd = 0;
    
    Animator animator;
    int maxFrameCount = 0;
    
    public void Start()
    {
        
        foreach (bodyAngle.bodyAngle joint in joints)//Set array sizes
        {
            Array.Resize<double>(ref joint.liveData, windowSize);
            if (!isRef) {joint.readReference();}
        }

        if (writeRef)
        {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.resetFile();
            }
        }

        // Print the number of frames in the longest animation
        Debug.Log("Max Frame Count: " + maxFrameCount);
        Application.targetFrameRate = -1;
    }

    public void Update()
    {   
        if (Input.GetKeyDown("space") && isRef)
        {
            if (writeRef)
            {
                if (toggleRec)
                {
                    //Write reference data
                    Debug.Log("Saving Data");
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.storeReference();
                    }
                    toggleRec = false;
                }
                else
                {
                    //Wipe reference file
                    Debug.Log("Wiping Ref Data");
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.resetFile();
                    }
                    toggleRec = true;
                }
            }
            Debug.Log("Toggle Reference: " + toggleRec);
        }
        if (toggleRec)
        {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.saveRef();
            }
        }
        else if (!isRef) {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.saveData(CounterLive);
            }

            if (CounterLive == (windowSize - 1))
            {
                System.Diagnostics.Stopwatch stopWatchOE = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch stopWatchNM = new System.Diagnostics.Stopwatch();
                // DTW
                OEDTW oeDTW = new OEDTW(joints[0].liveData, joints[0].refData);
                stopWatchOE.Start();
                oeDTW.computeDTW();
                // Path through DTW
                int j = getFirstNdx(oeDTW);
                stopWatchOE.Stop();
                TimeSpan tsOEDTW = stopWatchOE.Elapsed;

                int refDataLen = joints[0].refData.Length;
                double[] truncatedRef = new double[refDataLen-j];
                Array.Copy(joints[0].refData, j, truncatedRef, 0, refDataLen-j);

                stopWatchNM.Start();
                SimpleDTW normalDTW = new SimpleDTW(joints[0].liveData, truncatedRef);
                normalDTW.computeDTW();
                int totalLength = shortestPath(ref joints, normalDTW, j);
                stopWatchNM.Stop();
                TimeSpan tsNDTW = stopWatchNM.Elapsed;

                //Debug.Log("OE DTW: " + (tsOEDTW.Ticks / 10)  + ", Normal DTW: " + (tsNDTW.Ticks / 10) );
                Debug.Log("Offset: " + j + ", End Index: " + totalLength);
                squatPercentEnd = 100 * (double) (j + totalLength) / (refDataLen);

                // Printing stuff
                Debug.Log(squatPercentStart + "% - " + squatPercentEnd + "%");
                Debug.Log("Dumping");
                joints[0].dump();
                joints[0].checkFrame();
                //double otherAvg = joints[0].sumDiff / (double) totalLength;
                //Debug.Log("Other AVG: " + otherAvg);
                CounterLive = 0;
            }
            else
            {
                CounterLive++;
            }
        }
     }

     public void resetNdx() {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/DUMP_IANDJ.txt";
         using (StreamWriter sw = new StreamWriter(path))
         {
             sw.Write("");
         }
     }

     public void writeNdx(int i, int j)
     {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/DUMP_IANDJ.txt";
         using (StreamWriter sw = new StreamWriter(path, true))
         {
             sw.Write("i:"+i+"j:"+j);
         }
     }

     public void saveFMatrix(double[,] f)
     {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/FMatrix.txt";
         using (StreamWriter sw = new StreamWriter(path))
         {
             for (int i = 1; i < f.GetLength(0); i++)
             {
                 for (int j = 1; j < f.GetLength(1); j++)
                 {
                     sw.Write(f[i,j]);
                     if (j < f.GetLength(1) - 1)
                     {
                         sw.Write(" ");
                     }
                 }
                 sw.Write("\n");
             }
         }
     }

     public double pathWeight (ref double[,] f, int j)
     {
        // Initialisation
        //*------------------------------------------        
        int i = 1;
        double pathSum = 0;
        int totalLength = 0;
        resetNdx();
        int di = 1;
        int dj = 1;
        //*------------------------------------------
        // Shortest Path Method
        //*------------------------------------------
        while (i < f.GetLength(0) - 1)
        {
            if (j == f.GetLength(1) - 1)
            {
                i++;
                di = 1;
                dj = 0;
                pathSum += f[i, j];
            }
            else if (di == 1 && dj == 0) // Came from down
            {
                if (f[i + 1, j] >= f[i + 1, j + 1]) // Diag <= Up
                {
                    j++;
                    dj = 1;
                }
                i++;
                pathSum += f[i, j];
            }
            else if (di == 0 && dj == 1) //Came from left
            {
                if (f[i, j + 1] >= f[i + 1, j + 1]) // Right >= Diag
                {
                    i++;
                    di = 1;
                }
                j++;
                pathSum += f[i, j];
            }
            else // Came from diagonal
            {
                if (f[i + 1, j + 1] <= f[i + 1, j] && f[i + 1, j + 1] <= f[i, j + 1]) // Diag is smallest
                {
                    i++;
                    j++;
                    di = 1;
                    dj = 1;
                }
                else if (f[i + 1, j] <= f[i, j + 1]) // Up is smallest
                {
                    i++;
                    di = 1;
                    dj = 0;
                }
                else // Right is smallest
                {
                    j++;
                    di = 0;
                    dj = 1;
                }
                pathSum += f[i, j];
            }
            totalLength++;
        }
        return pathSum / (double) totalLength;
     }

     public int smallestIndex(ref double[] ndxLst)
     {
         int ndx = 0;
         double startPoint = double.PositiveInfinity;
         for (int i = 0; i < ndxLst.Length; i++)
         {
             if (ndxLst[i] < startPoint)
            {
                startPoint = ndxLst[i];
                ndx = i;
            }
         }
         return ndx;
     }

     public int getFirstNdx(OEDTW oeDTW)
     {
        // Go through all the first values and look when it goes from negative to positive or to 0 (not from 0 to positive or 0)
        double[,] f = oeDTW.getFMatrix();
        List<double> startValues = CalculateTwoSidedMovingAverage(ref f, 4);
        double oldDiff = -1;
        List<Tuple<int, double>> ndxValuePairs = new List<Tuple<int, double>>();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int k = 1; k < startValues.Count; k++)
        {
            double newDiff = startValues[k] - startValues[k - 1];
            if (oldDiff < 0 && newDiff >= 0)
            {
                ndxValuePairs.Add(new Tuple<int, double>(k, f[1, k]));
            }
            oldDiff = newDiff;
        }
        // Do some statistical analysis to remove start points that are big   
        List<int> indices = filterNdx(ref ndxValuePairs);
        double[] ndxLst = new double[indices.Count];
        // Calculate cost functions on the remaining indices
        for (int i = 0; i < indices.Count; i++)
        {
            ndxLst[i] = pathWeight(ref f, indices[i]);
        }

        // Grab the lowest one
        int lowest = smallestIndex(ref ndxLst);
        int j = indices[lowest];
        sw.Stop();
        TimeSpan ts = sw.Elapsed;
        Debug.Log("Total time in us: " + (ts.Ticks/10));
        squatPercentStart = 100 * (double) j / ((double) f.GetLength(1)-1);
        //saveFMatrix(f);
        return j;
     }

    public List<double> CalculateTwoSidedMovingAverage(ref double[,] f, int windowSize)
    {
        List<double> movingAverageList = new List<double>();
        double avg = 0;

        if (f.GetLength(1) < windowSize - 1 || windowSize <= 0)
        {
            throw new ArgumentException("Invalid window size or input list length.");
        }
        
        for (int i = 1; i < f.GetLength(1); i++)
        {
            //Begin & end process
            if (i <= windowSize + 1 || i >= f.GetLength(1) - windowSize - 1)
            {
                int start = Math.Max(1, i - windowSize);
                int end = Math.Min(f.GetLength(1) - 1, i + windowSize);

                double sum = 0;
                for (int j = start; j <= end; j++)
                {
                    sum += f[1, j];
                }

                avg = sum / (end - start + 1);
                movingAverageList.Add(avg);
            }
            //Middle process
            else
            {
                avg -= f[1, i - windowSize - 1] / (2*windowSize + 1);
                avg += f[1, i + windowSize] / (2*windowSize + 1);
                movingAverageList.Add(avg);
            }
        }

        return movingAverageList;
    }

    public List<int> filterNdx(ref List<Tuple<int, double>> ndxValuePairs)
    {
        ndxValuePairs.Sort((x, y) => x.Item2.CompareTo(y.Item2));
        int iLen = ndxValuePairs.Count;
        List<int> retLst = new List<int>();
        if (iLen < 3)
        {
            foreach (Tuple<int, double> pair  in ndxValuePairs)
            {
                retLst.Add(pair.Item1);
            }
            return retLst;
        }
        double lqr = ndxValuePairs[iLen/2].Item2 - ndxValuePairs[iLen/4].Item2;
        double eps = ndxValuePairs[iLen/2].Item2 + lqr;
        foreach (Tuple<int, double> pair  in ndxValuePairs) 
        {
            if (pair.Item2 <= eps)
            {
                retLst.Add(pair.Item1);
            }
        }
        return retLst;
    }

    public int shortestPath(ref bodyAngle.bodyAngle[] joints, SimpleDTW dtw, int jOffset)
    {

        // Initialisation
        //*------------------------------------------
        int i = 1;
        int j = 1;
        double[,] f = dtw.getFMatrix();
        foreach (bodyAngle.bodyAngle joint in joints)
        {
            joint.sumLive = joint.liveData[i - 1];
            joint.sumRef = joint.refData[j - 1];
            joint.sumDiff = 0;
            joint.avgErrors.Clear();
        }
        int counterX = 1;
        int counterY = 1;
        resetNdx();
        int di = 1;
        int dj = 1;
        //*------------------------------------------
        // Shortest Path Method
        //*------------------------------------------
        while (i < f.GetLength(0) - 1 && j < f.GetLength(1) - 1)
        {            
            if (di == 1 && dj == 0) // Came from down
            {
                if (f[i + 1, j] < f[i + 1, j + 1]) // Up < Diag
                {
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumLive += joint.liveData[i - 1];
                    }
                    counterX++;
                }
                else // Diag <= Up
                {                    
                    averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                    writeNdx(i, j+jOffset); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    j++;
                    dj = 1;
                }
                i++;
            }
            else if (di == 0 && dj == 1) //Came from left
            {
                if (f[i, j + 1] < f[i + 1, j + 1]) // Right < Diag
                {                    
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumRef += joint.refData[j - 1];
                    }   
                    counterY++;
                }
                else
                {                    
                    averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                    writeNdx(i, j+jOffset); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    i++;
                    di = 1;
                }
                j++;
            }
            else // Came from diagonal
            {
                averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                writeNdx(i, j+jOffset); // Write the indices to a file
                // Reset counters
                counterX = 1;
                counterY = 1;
                if (f[i + 1, j + 1] <= f[i + 1, j] && f[i + 1, j + 1] <= f[i, j + 1]) // Diag is smallest
                {
                    i++;
                    j++;
                    di = 1;
                    dj = 1;
                }
                else if (f[i + 1, j] <= f[i, j + 1]) // Up is smallest
                {                    
                    i++;
                    di = 1;
                    dj = 0;
                }
                else // Right is smallest
                {
                    j++;
                    di = 0;
                    dj = 1;
                }
            }
        }
        // Need to write one more time for the last index
        averagePath(ref joints, i, j, counterX, counterY); // Calculate average
        writeNdx(i, j+jOffset); // Write the indices to a file
        //*------------------------------------------
        saveFMatrix(f);
        return j;
    }

    public void averagePath(ref bodyAngle.bodyAngle[] joints, int i, int j, int counterX, int counterY)
    {
        foreach (bodyAngle.bodyAngle joint in joints)
        {
            joint.avgLive = joint.sumLive / (double) counterX;
            joint.avgRef = joint.sumRef / (double) counterY;

            joint.sumDiff += Math.Abs(joint.avgLive - joint.avgRef);
            joint.avgErrors.Add(Math.Abs(joint.avgLive - joint.avgRef));

            joint.sumLive = joint.liveData[i - 1];
            joint.sumRef = joint.refData[j - 1];
        }
    }
}
