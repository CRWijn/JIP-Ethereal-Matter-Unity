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
    public bodyAngle.bodyAngle[] joints;
    public int windowSize;
    

    int CounterLive = 0;
    int CounterRef = 0;
    bool writtenRef = false;
    
    Animator animator;
    int maxFrameCount = 0;
    
    public void Start()
    {
        Application.targetFrameRate = 25;
        animator = GetComponent<Animator>();

        // Get all animation clips from the Animator
        AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;

        // Iterate through all animation clips
        foreach (AnimationClip clip in animationClips)
        {
            int frameCount = Mathf.RoundToInt(clip.frameRate * clip.length);
            if (frameCount > maxFrameCount)
            {
                maxFrameCount = frameCount;
            }
        }
        foreach (bodyAngle.bodyAngle joint in joints)//Set array sizes
        {
            if (isRef) //Only if you want to record reference data
            {
                Array.Resize<double>(ref joint.refData, maxFrameCount);
                if (writeRef)
                {
                    joint.resetFile(); //Wipe the file
                }
            }
            Array.Resize<double>(ref joint.liveData, windowSize);
            joint.readReference();
        }

        // Print the number of frames in the longest animation
        Debug.Log("Max Frame Count: " + maxFrameCount);
        Application.targetFrameRate = 60;
    }

    public void Update()
    {   
        if (isRef)
        {
            gatherReference();
            
        }
        else {
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                joint.saveData(CounterLive);
            }

            if (CounterLive == (windowSize - 1))
            {
                // DTW
                //SimpleDTW simpleDTW = new SimpleDTW(joints[0].liveData, joints[0].refData);
                OEDTW simpleDTW = new OEDTW(joints[0].liveData, joints[0].refData);
                simpleDTW.computeDTW();

                // Path through DTW
                int totalLength = shortestPath(ref joints, simpleDTW);

                // Printing stuff
                //Debug.Log("Dumping");
                //joints[0].dump();
                //joints[0].checkFrame();
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

     public void gatherReference() {
         foreach (bodyAngle.bodyAngle joint in joints)
        {
            joint.saveRef(CounterRef);
        }

        if (CounterRef == (maxFrameCount - 1) && !writtenRef)
        {
            Debug.Log("DONE");
            foreach (bodyAngle.bodyAngle joint in joints)
            {
                if (writeRef)
                {
                    joint.storeReference();
                }
            }
            writtenRef = true;
            CounterRef = 0;
        }
        else if (!writtenRef){
            CounterRef++;
        }
     }

     public void resetNdx() {
         string path = Directory.GetCurrentDirectory() + "/DTW Investigation/DUMP_IANDJ.txt";
         using (StreamWriter sw = new StreamWriter(path))
         {
             sw.Write("");
         }
     }

     public void writeNdx(int i, int j) {
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

     public double pathWeight (ref double[,] f, int j) {
        // Initialisation
        //*------------------------------------------        
        int i = 1;
        double pathSum = 0;
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
        }
        return pathSum;
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

     public int shortestPath(ref bodyAngle.bodyAngle[] joints, OEDTW dtw)
     {
        // Initialisation
        //*------------------------------------------
        int i = 1;
        int j = 1;
        double[,] f = dtw.getFMatrix();
        double[] ndxLst = new double[f.GetLength(1)-1];
        for (int k = 1; k < f.GetLength(1); k++) // Find start point (smallest in j)
        {
            ndxLst[k-1] = pathWeight(ref f, k-1);
        }
        j = smallestIndex(ref ndxLst) + 1;
        double squatPercentStart = 100 * (double) j / ((double) f.GetLength(1)-1);
        foreach (bodyAngle.bodyAngle joint in joints)
        {
            joint.sumLive = joint.liveData[i - 1];
            joint.sumRef = joint.refData[j - 1];
            joint.sumDiff = 0;
            joint.avgErrors.Clear();
        }
        int counterX = 1;
        int counterY = 1;
        int totalLength = 0;
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
                    writeNdx(i, j); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    totalLength++;
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
                    writeNdx(i, j); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    totalLength++;
                    i++;
                    di = 1;
                }
                j++;
            }
            else // Came from diagonal
            {
                averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                writeNdx(i, j); // Write the indices to a file
                // Reset counters
                counterX = 1;
                counterY = 1;
                totalLength++;
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
        writeNdx(i, j); // Write the indices to a file
        //*------------------------------------------
        saveFMatrix(f);
        double squatPercentEnd = 100 * (double) j / ((double) f.GetLength(1)-1);
        Debug.Log(squatPercentStart + "% - " + squatPercentEnd + "% in the squat");
        return totalLength;
     }

     public int shortestPath(ref bodyAngle.bodyAngle[] joints, SimpleDTW dtw)
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
        int totalLength = 0;
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
                    writeNdx(i, j); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    totalLength++;
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
                    writeNdx(i, j); // Write the indices to a file
                    // Reset counters
                    counterX = 1;
                    counterY = 1;
                    totalLength++;
                    i++;
                    di = 1;
                }
                j++;
            }
            else // Came from diagonal
            {
                averagePath(ref joints, i, j, counterX, counterY); // Calculate average
                writeNdx(i, j); // Write the indices to a file
                // Reset counters
                counterX = 1;
                counterY = 1;
                totalLength++;
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
        writeNdx(i, j); // Write the indices to a file
        //*------------------------------------------
        saveFMatrix(f);
        return totalLength;
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
