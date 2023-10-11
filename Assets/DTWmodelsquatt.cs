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
                SimpleDTW simpleDTW = new SimpleDTW(joints[0].liveData, joints[0].refData);
                simpleDTW.computeDTW();
                double[,] f = simpleDTW.getFMatrix();
                int i = joints[0].liveData.Length;
                int j = joints[0].refData.Length;

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

                while (i > 0 || j > 0)
                {
                    Debug.Log(i + ", " + j);
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1] && di == 1) //Down
                    {
                        if (dj == 1 && i != (joints[0].liveData.Length-1) && j != (joints[0].refData.Length-1)) //Diagonal or Left
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
                            writeNdx(i, j);
                            counterX = 1;
                            counterY = 1;
                            totalLength++;
                        }
                        else
                        {
                            foreach (bodyAngle.bodyAngle joint in joints)
                            {
                                joint.sumLive += joint.liveData[i - 1];
                            }
                            counterX++;
                        }
                        di = 1;
                        dj = 0;
                        i--;
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j] && dj == 1) //Left
                    {
                        if (di == 1 && i != (joints[0].liveData.Length-1) && j != (joints[0].refData.Length-1)) //Diagonal or Down
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
                            writeNdx(i, j);
                            counterX = 1;
                            counterY = 1;
                            totalLength++;
                        }
                        else
                        {
                            foreach (bodyAngle.bodyAngle joint in joints)
                            {
                                joint.sumRef += joint.refData[j - 1];
                            }   
                            counterY++;
                        }
                        di = 0;
                        dj = 1;
                        j--;
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j]) //Diagonal
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
                        writeNdx(i, j);
                        counterX = 1;
                        counterY = 1;
                        totalLength++;
                        di = 1;
                        dj = 1;
                        i--;
                        j--;
                    }
                }
                Debug.Log("Dumping");
                joints[0].dump();
                joints[0].checkFrame();
                saveFMatrix(f);
                double otherAvg = joints[0].sumDiff / (double) totalLength;
                Debug.Log("Other AVG: " + otherAvg);
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
}
