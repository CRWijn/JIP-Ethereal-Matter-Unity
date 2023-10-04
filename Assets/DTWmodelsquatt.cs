using System;
using UnityEngine;
using System.Collections.Generic; // Import List from System.Collections.Generic
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using DTW;

public class DTWmodelsquat : MonoBehaviour
{
    public GameObject squatsnel;
    public GameObject squattraag;
    public Transform heuptraag;
    public Transform knietraag;
    public Transform enkeltraag;
    public Transform heupsnel;
    public Transform kniesnel;
    public Transform enkelsnel;

    private int CounterA = 0;
    private int CounterB = 0;

    private double[] x = new double[120];
    private double[] y = new double[120];
    private double sum = 0;

    bool written = false;
    
    private Animator animator;
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
        Array.Resize<double>(ref x, maxFrameCount);
        Array.Resize<double>(ref y, maxFrameCount);
        Debug.Log("lengthe x : " + x.Length);
    }

    public void Update()
    {
            //Making the resultValue equal to the input value from myValueA concatenating it with myValueB.
            Vector3 bovenbeentraag = heuptraag.position - knietraag.position;
            Vector3 onderbeentraag = knietraag.position - enkeltraag.position;
            Vector3 bovenbeensnel = heupsnel.position - kniesnel.position;
            Vector3 onderbeensnel = kniesnel.position - enkelsnel.position;
            
            float anglesnel = Vector3.Angle(onderbeentraag, bovenbeentraag);
            float angletraag = Vector3.Angle(onderbeensnel, bovenbeensnel);

            x[CounterA] = anglesnel;
            y[CounterB] = angletraag;

        if (CounterA == (maxFrameCount - 1))
        {
            SimpleDTW simpleDTW = new SimpleDTW(x, y);
            simpleDTW.computeDTW();
            double[,] f = simpleDTW.getFMatrix();
            //Debug.Log (f[15,15]);
            int i = x.Length;
            int j = y.Length;
            double sumX = x[i - 1];
            double sumY = y[j - 1];
            int counterX = 1;
            int counterY = 1;
            double averageX = 1;
            double averageY = 1;
            double AbsDiffXY = 1;


            string path = Directory.GetCurrentDirectory();
            using (StreamWriter sw = new StreamWriter(path + "/output.txt"))
            {
                if (!written)
                {
                    Debug.Log("Writing x and y");
                    foreach (double x_val in x)
                    {
                        sw.Write(x_val);
                        sw.Write(", ");
                    }
                    sw.Write("\n");
                    foreach (double y_val in y)
                    {
                        sw.Write(y_val);
                        sw.Write(", ");
                    }
                    sw.Write("\n");
                }
                while (i > 0 || j > 0)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                    {
                        sumX = sumX + x[i - 1];
                        counterX++;
                        i--;
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                    {
                        sumY = sumY + y[j - 1];
                        counterY++;
                        j--;
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                    {
                        averageX = sumX / (double)counterX;
                        averageY = sumY / (double)counterY;
                        AbsDiffXY = Math.Abs(averageX - averageY);
                        i--;
                        j--;
                        sumX = x[i];
                        sumY = y[j];
                        counterX = 1;
                        counterY = 1;
                    }
                    //Debug.Log ("i="+i+" ,j="+j);

                    // Extract data based on the matched frames (i and j)
                    if (i < x.Length && j < y.Length)
                    {
                        double valueFromX = x[i];  // Value from array x at index i
                        double valueFromY = y[j];  // Value from array y at index j

                        // Do something with the extracted data (e.g., print or store it)
                        Debug.Log("i =" + i + " : " + valueFromX + " j = " + j + " : " + valueFromY + " Difference = " + Math.Abs(valueFromX - valueFromY));
                        //Debug.Log(f[1,1]);
                        if (!written)
                        {
                            sw.Write("i:");
                            sw.Write(i);
                            sw.Write("j:");
                            sw.Write(j);
                        }
                    }
                    written = true;
                }

                sum = simpleDTW.getSum();
                CounterA = 0;
                CounterB = 0;
            }
        }
        else
        {
            CounterA++;
            CounterB++;
        }
     }
}
