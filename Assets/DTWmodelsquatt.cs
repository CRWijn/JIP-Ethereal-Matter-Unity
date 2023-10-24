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
    public bool useCostFunction;
    public bool debugMode;
    public bool OBEDTW;
    public bodyAngle.bodyAngle[] joints;
    public int windowSize;
    
    
    int CounterLive = 0;
    
    Animator animator;
    int maxFrameCount = 0;
    
    public void Start()
    {
        
        foreach (bodyAngle.bodyAngle joint in joints)//Set array sizes
        {
            Array.Resize<double>(ref joint.liveData, windowSize);
            if (!isRef) joint.readReference(); // Read all the reference data from files
        }

        if (writeRef)
        {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.resetFile(); // If we're writing new reference data then we should reset all the reference data
            }
        }

        // Print the number of frames in the longest animation
        Debug.Log("Max Frame Count: " + maxFrameCount);
        Application.targetFrameRate = 60; // Set the application frame rate
    }

    public void Update()
    {   
        if (Input.GetKeyDown("space") && isRef) // If writing of ref data is enabled and the model is selected as reference then space is used to start and stop recording reference data
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
                if (debugMode) { // Write data to plot in python
                    writeArray<double>("refData", joints[0].refData); // Matching ref
                    writeArray<double>("liveData", joints[0].liveData); // Matching live
                    writeArray<double>("compareRefData", joints[2].refData); // Comparison ref
                    writeArray<double>("compareLiveData", joints[2].liveData); // Comparison live
                }
                if (!OBEDTW)
                {
                    System.Diagnostics.Stopwatch stopWatchOE = new System.Diagnostics.Stopwatch();
                    System.Diagnostics.Stopwatch stopWatchNM = new System.Diagnostics.Stopwatch();
                    // Compute the open begin DTW, if you aren't using cost functions you could just calculate the distances between live[0] and ref[i] and pass that forward (much faster)
                    OEDTW oeDTW = new OEDTW(joints[0].liveData, joints[0].refData);
                    stopWatchOE.Start(); // Used to report how long it takes to find the start point
                    oeDTW.computeDTW(); // Execute the open begin DTW
                    // Get the estimated begin of the live in the ref
                    int j = getFirstNdx(oeDTW);
                    stopWatchOE.Stop();
                    TimeSpan tsOEDTW = stopWatchOE.Elapsed;

                    // Only use reference data from the start point that was calculated
                    int refDataLen = joints[0].refData.Length;
                    double[] truncatedRef = new double[refDataLen-j]; // Cut anything thats not needed
                    Array.Copy(joints[0].refData, j, truncatedRef, 0, refDataLen-j);

                    stopWatchNM.Start(); // Time the pathing 
                    SimpleDTW normalDTW = new SimpleDTW(joints[0].liveData, truncatedRef);
                    normalDTW.computeDTW();
                    int totalLength = shortestPath(ref joints, normalDTW, j); // Find the least cost path
                    stopWatchNM.Stop();
                    TimeSpan tsNDTW = stopWatchNM.Elapsed;

                    // Print the timing in microseconds
                    Debug.Log("OE DTW: " + (tsOEDTW.Ticks / 10)  + "us, Normal DTW: " + (tsNDTW.Ticks / 10) + "us");
                }
                else 
                {
                    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    OBEDTW dtw = new OBEDTW(joints[0].liveData, joints[0].refData);
                    Tuple<int[], int[]> indices = dtw.computeDTW();
                    processIndices(dtw.getFMatrix(), indices.Item1, indices.Item2);

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    // Print the timing in microseconds
                    Debug.Log("Total time: " + (ts.Ticks / 10) + "us");
                }

                // Check all errors
                for (int i = 1; i < joints.Length; i++)
                {
                    joints[i].checkFrame();
                }
                CounterLive = 0;
            }
            else
            {
                CounterLive++;
            }
        }
     }

     public void processIndices(double[,] f, int[] iList, int[] jList)
     {
        foreach (bodyAngle.bodyAngle joint in joints)
        {
            joint.sumLive = joint.liveData[iList[0] - 1];
            joint.sumRef = joint.refData[jList[0] - 2];
            joint.sumDiff = 0;
            joint.avgErrors.Clear();
        }
        int counterX = 1;
        int counterY = 1;
        int prevI = iList[0] - iList[1];
        int prevJ = jList[0] - jList[1];
        resetNdx(); // Reset the DUMP_IANDJ file
        for (int k = 1; k < iList.Length - 1; k++)
        {
            if (prevI == 0)
            {
                if (iList[k] - iList[k + 1] == 0)
                {
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumLive += joint.liveData[iList[k] - 1];
                    }
                    counterX++;
                }
                else
                {
                    averagePath(ref joints, iList[k], jList[k] - 1, counterX, counterY); // Calculate average
                    if (debugMode) writeNdx(iList[k], jList[k]); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                }
            }
            else if (prevJ == 0)
            {
                if (jList[k] - jList[k + 1] == 0)
                {
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumRef += joint.refData[jList[k] - 2];
                    }
                    counterY++;
                }
                else
                {
                    averagePath(ref joints, iList[k], jList[k] - 1, counterX, counterY); // Calculate average
                    if (debugMode) writeNdx(iList[k], jList[k]); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                }
            }
            else
            {
                if (iList[k] - iList[k + 1] == 0)
                {
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumLive += joint.liveData[iList[k] - 1];
                    }
                    counterX++;
                }
                else if (jList[k] - jList[k + 1] == 0)
                {
                    foreach (bodyAngle.bodyAngle joint in joints)
                    {
                        joint.sumRef += joint.refData[jList[k] - 2];
                    }
                    counterY++;
                }
                else
                {
                    averagePath(ref joints, iList[k], jList[k] - 1, counterX, counterY); // Calculate average
                    if (debugMode) writeNdx(iList[k], jList[k]); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                }
            }
            prevI = iList[k] - iList[k + 1];
            prevJ = jList[k] - jList[k + 1];
        }
        if (prevI == 0)
        {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.sumLive += joint.liveData[iList[iList.Length - 1] - 1];
            }
            counterX++;
        }
        if (prevJ == 0)
        {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.sumRef += joint.refData[jList[jList.Length - 1] - 2];
            }
            counterY++;
        }
        averagePath(ref joints, iList[iList.Length - 1], jList[jList.Length - 1] - 1, counterX, counterY);
        if (debugMode)
        {
            writeNdx(iList[iList.Length - 1], jList[jList.Length - 1]); // Write the indices to a file
            saveFMatrix(f);
        }
     }

     // This function writes a nothing to a file which essentially resets it
     public void resetNdx() {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/DUMP_IANDJ.txt";
         using (StreamWriter sw = new StreamWriter(path))
         {
             sw.Write("");
         }
     }

     // This function writes the received i and j to a file called DUMP_IANDJ for plotting in python
     public void writeNdx(int i, int j)
     {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/DUMP_IANDJ.txt";
         using (StreamWriter sw = new StreamWriter(path, true))
         {
             sw.Write("i:"+i+"j:"+j);
         }
     }

     // This function is used to save an entire matrix for plotting in python
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

     // This function is used to calculate the cost of a certain path
     public double pathWeight (ref double[,] f, int j)
     {
        // Initialisation
        //*------------------------------------------        
        int i = 1;
        double pathSum = 0;
        int totalLength = 0;
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

     // This function gets the smallest number in an array and returns the index of it
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

     // This function is used to find the frame in the reference data that best matches the first frame in the live capture segment
     public int getFirstNdx(OEDTW oeDTW)
     {
        // Get the open begin DTW matrix. If you don't use cost function you can easily just use the distances instead of an entire matrix (much faster)
        double[,] f = oeDTW.getFMatrix();
        double[] refRow = new double[f.GetLength(1)-1]; // Reference data distance to first frame
        for (int i = 1; i < f.GetLength(1); i++) // Fill in the reference data with values from the matrix
        {
            refRow[i - 1] = f[1, i];
        }
        
        List<double> startValues = CalculateTwoSidedMovingAverage(ref f, 4);
        
        double oldDiff = -1;
        List<Tuple<int, double>> ndxValuePairs = new List<Tuple<int, double>>();
        // Looking when the d/dt changes sign from negative to positive (minimum)
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
        // Grab the lowest one
        int lowest = 0;
        if (useCostFunction)
        {
            // Calculate cost functions on the selected indices
            for (int i = 0; i < indices.Count; i++)
            {
                ndxLst[i] = pathWeight(ref f, indices[i]);
            }
            lowest = smallestIndex(ref ndxLst);
        }
        else
        {
            indices.Sort(); 
            lowest = 0; // Basically grab the early most start point that hasn't been filtered out since most of the time DTW can correct for that
        }
        int j = indices[lowest];
        if (debugMode) // Write some values for plotting in python
        {
            writeArray<double>("frame0distances", refRow); // The distances from the first live frame to all the frames in the reference data
            writeArray<double>("smoothedDistances", startValues); // The distances after smoothing
            writeArray<int>("selectedIndices", indices); // Minimum indices
            saveFMatrix(f); // Save the matrix
            if (useCostFunction) writeArray<double>("selectedIndicesCosts", ndxLst); // The costs of the select indices
        }
        return j;
     }

    // This function is used to smooth curves by using a windowed average
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
            //Begin & end process: just sum up all the values
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
            //Middle process: only remove last value and add new value. This saves A LOT of time
            else
            {
                avg -= f[1, i - windowSize - 1] / (2*windowSize + 1);
                avg += f[1, i + windowSize] / (2*windowSize + 1);
                movingAverageList.Add(avg);
            }
        }
        return movingAverageList;
    }

    // This function is used to filter out indices. You can do some statistical analysis on this but we kept it simple
    public List<int> filterNdx(ref List<Tuple<int, double>> ndxValuePairs)
    {
        // Sort the ndxValuePairs by their distance to the reference frames
        ndxValuePairs.Sort((x, y) => x.Item2.CompareTo(y.Item2));
        int iLen = ndxValuePairs.Count;
        List<int> retLst = new List<int>();
        double eps = 5 * ndxValuePairs[0].Item2; // Our threshold is all values that are within 5x of the smallest index
        foreach (Tuple<int, double> pair  in ndxValuePairs) 
        {
            if (pair.Item2 <= eps)
            {
                retLst.Add(pair.Item1); // Only add the ones that fit our criteria
            }
        }
        return retLst;
    }

    // This function plots the path through the DTW elements (frame matching)
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
            joint.sumRef = joint.refData[j + jOffset - 1];
            joint.sumDiff = 0;
            joint.avgErrors.Clear();
        }
        int counterX = 1;
        int counterY = 1;
        resetNdx(); // Reset the DUMP_IANDJ file
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
                    averagePath(ref joints, i, j + jOffset, counterX, counterY); // Calculate average
                    if (debugMode) writeNdx(i, j+jOffset); // Write the indices to a file
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
                        joint.sumRef += joint.refData[j + jOffset - 1];
                    }   
                    counterY++;
                }
                else
                {                    
                    averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                    if (debugMode) writeNdx(i, j+jOffset); // Write the indices to a file
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
                averagePath(ref joints, i, j + jOffset, counterX, counterY); // Calculate average
                if (debugMode) writeNdx(i, j+jOffset); // Write the indices to a file
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
        averagePath(ref joints, i, j + jOffset, counterX, counterY); // Calculate average
        if (debugMode) writeNdx(i, j+jOffset); // Write the indices to a file
        //*------------------------------------------
        //saveFMatrix(f);
        return j;
    }

    // This function is used to calculate the average for each included joint
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

    // A generic function to write arrays to a file
    public void writeArray<T>(string fileName, T[] arr)
    {
        string path = Directory.GetCurrentDirectory() + "/DTW Investigation/Reporting/" + fileName + ".txt";
        using (StreamWriter sw = new StreamWriter(path))
        {
            for (int i = 0; i < arr.Length; i++)
            {
                sw.Write(arr[i]);
                if (i < arr.Length - 1) sw.Write(" ");
            }
        }
    }

    // A generic function to write lists to a file
    public void writeArray<T>(string fileName, List<T> arr)
    {
        string path = Directory.GetCurrentDirectory() + "/DTW Investigation/Reporting/" + fileName + ".txt";
        using (StreamWriter sw = new StreamWriter(path))
        {
            for (int i = 0; i < arr.Count; i++)
            {
                sw.Write(arr[i]);
                if (i < arr.Count - 1) sw.Write(" ");
            }
        }
    }
}
