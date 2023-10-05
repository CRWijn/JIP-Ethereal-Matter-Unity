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
    public bodyAngle.bodyAngle leftKneeLive;
    public bodyAngle.bodyAngle leftKneeRef;
    //public bodyAngle.bodyAngle leftAnkleLive;
    //public bodyAngle.bodyAngle leftAnkleRef;  
    //public bodyAngle.bodyAngle[] joints;

    int CounterLive = 0;
    int CounterRef = 0;

    double[] Live = new double[120];
    double[] Ref = new double[120];
    //double[] lAnkleLive = new double[120];
    //double[] lAnkleRef = new double[120];
    double sum = 0;

    bool written = false;
    
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

        // Print the number of frames in the longest animation
        Debug.Log("Max Frame Count: " + maxFrameCount);
        Array.Resize<double>(ref Live, maxFrameCount);
        Array.Resize<double>(ref Ref, maxFrameCount);
        //Array.Resize<double>(ref lAnkleLive, maxFrameCount);
        //Array.Resize<double>(ref lAnkleRef, maxFrameCount);
        Debug.Log("lengthe x : " + Live.Length);
    }

    public void Update()
    {            
        //Defining the angles
            float lKneeAngleLive = leftKneeLive.getAngle();
            float lKneeAngleRef = leftKneeRef.getAngle();
            //float lAnkleAngleLive = leftAnkleLive.getAngle();
            //float lAnkleAngleRef = leftAnkleRef.getAngle();

        //Filling the angle matrices per frame
            Live[CounterLive] = lKneeAngleLive;
            Ref[CounterRef] = lKneeAngleRef;
            //lAnkleLive[CounterLive] = lAnkleAngleLive;
            //lAnkleRef[CounterRef] = lAnkleAngleRef;

        //DTW script usage and filling in the DTW matrix and matching frames
        if (CounterLive == (maxFrameCount - 1))
        {
            SimpleDTW simpleDTW = new SimpleDTW(Live, Ref);
            simpleDTW.computeDTW();
            double[,] f = simpleDTW.getFMatrix();
            int i = Live.Length;
            int j = Ref.Length;
            double sumX = Live[i - 1];
            double sumY = Ref[j - 1];
            int counterX = 1;
            int counterY = 1;
            double averageX = 1;
            double averageY = 1;
            int di = 1;
            int dj = 1;
            double AbsDiffXY = 1;


            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/output.txt"))
            {
                if (false)//(!written)
                {
                    foreach (double x_val in Live)
                    {
                        sw.Write(x_val);
                        sw.Write(", ");
                    }
                    sw.Write("\n");
                    foreach (double y_val in Ref)
                    {
                        sw.Write(y_val);
                        sw.Write(", ");
                    }
                    sw.Write("\n");
                }
                while (i > 0 || j > 0)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1]) //Left
                    {
                        if (dj == 1 && i != (Live.Length-1) && j != (Ref.Length-1)) //Diagonal or Down
                        {
                            averageX = sumX / (double)counterX;
                            averageY = sumY / (double)counterY;
                            sw.Write("x:");
                            sw.Write(averageX);
                            sw.Write("y:");
                            sw.Write(averageY);
                            AbsDiffXY = Math.Abs(averageX - averageY);
                            Debug.Log(i + ", " + j + ": Diff X and Y " + AbsDiffXY);
                            sumX = Live[i-1];
                            sumY = Ref[j-1];
                            counterX = 1;
                            counterY = 1;
                        }
                        else
                        {
                            sumX += Live[i-1];
                            counterX += 1;
                        }
                        di = 1;
                        dj = 0;
                        i--;
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j]) //Down
                    {
                        if (di == 1 && i != (Live.Length-1) && j != (Ref.Length-1)) //Diagonal or Left
                        {
                            averageX = sumX / (double)counterX;
                            averageY = sumY / (double)counterY;
                            sw.Write("x:");
                            sw.Write(averageX);
                            sw.Write("y:");
                            sw.Write(averageY);
                            AbsDiffXY = Math.Abs(averageX - averageY);
                            Debug.Log(i + ", " + j + ": Diff X and Y " + AbsDiffXY);
                            sumX = Live[i-1];
                            sumY = Ref[j-1];
                            counterX = 1;
                            counterY = 1;
                        }
                        else
                        {
                            sumY += Ref[j-1];
                            counterY += 1;
                        }
                        di = 0;
                        dj = 1;
                        j--;
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j]) //Diagonal
                    {
                        averageX = sumX / (double)counterX;
                        averageY = sumY / (double)counterY;
                        sw.Write("x:");
                        sw.Write(averageX);
                        sw.Write("y:");
                        sw.Write(averageY);
                        AbsDiffXY = Math.Abs(averageX - averageY);
                        Debug.Log(i + ", " + j + ": Diff X and Y " + AbsDiffXY);
                        sumX = Live[i-1];
                        sumY = Ref[j-1];
                        counterX = 1;
                        counterY = 1;
                        di = 1;
                        dj = 1;
                        i--;
                        j--;
                    }

                    // Extract data based on the matched frames (i and j)
                    if (i < Live.Length && j < Ref.Length)
                    {
                        double valueFromX = Live[i];  // Value from array x at index i
                        double valueFromY = Ref[j];  // Value from array y at index j

                        // Do something with the extracted data (e.g., print or store it)
                        //Debug.Log("i =" + i + " : " + valueFromX + " j = " + j + " : " + valueFromY + " Difference = " + Math.Abs(valueFromX - valueFromY));
                        //Debug.Log(f[1,1]);
                        if (false)//(!written)
                        {
                            sw.Write("i:");
                            sw.Write(i);
                            sw.Write("j:");
                            sw.Write(j);
                        }
                    }
                }

                sum = simpleDTW.getSum();
                CounterLive = 0;
                CounterRef = 0;
            }
            written = true;
            // Extract data based on the matched frames (i and j)
        //    if (i < Live.Length && j < Ref.Length)
        //    {
        //        double AbsDiffCD = Math.Abs(lAnkleLive[i] - lAnkleRef[j]);


        //        double AbsDiffAB2 = Math.Abs(Live[i] - Ref[j]); 
        //        double AbsDiffCD2 = Math.Abs(lAnkleLive[i] - lAnkleRef[j]);

        //        double valueFromX = Live[i];  
        //        double valueFromY = Ref[j]; 
        //        double AbsDiffAB = Math.Abs(valueFromX - valueFromY);

        //        double valueFromC = lAnkleLive[i]; 
        //        double valueFromD = lAnkleRef[j];  
        //        double AbsDiffCD = Math.Abs(valueFromC - valueFromD);

        //        Debug.Log((AbsDiffAB - AbsDiffCD));
        //        Debug.Log((AbsDiffAB - AbsDiffCD) + " " + (AbsDiffAB2 - AbsDiffCD2));
        //    }
        }
        else
        {
            CounterLive++;
            CounterRef++;
        }
     }
}
