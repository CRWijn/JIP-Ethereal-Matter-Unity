using System;
using UnityEngine;
using System.Collections.Generic; // Import List from System.Collections.Generic
using System.Linq;
using System.Text;
using System.Collections;
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
    //private int counter = 0;

    private double[] x = new double[1];
    private double[] y = new double[1];
    private double sum = 0;
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
            Vector3 elleboogpolssnel = heuptraag.position - knietraag.position;
            Vector3 schouderarmsnel = knietraag.position - enkeltraag.position;
            Vector3 elleboogpolstraag = heupsnel.position - kniesnel.position;
            Vector3 schouderarmtraag = kniesnel.position - enkelsnel.position;
            
            float anglesnel = Vector3.Angle(schouderarmsnel, elleboogpolssnel);
            float angletraag = Vector3.Angle(schouderarmtraag, elleboogpolstraag);

            x[CounterA] = anglesnel;
            y[CounterB] = angletraag;
           
            if (CounterA == (maxFrameCount-1))
            {
                SimpleDTW simpleDTW = new SimpleDTW(x, y);
                simpleDTW.computeDTW();
                double[,] f = simpleDTW.getFMatrix();
            //Debug.Log("lengthe DTW matrix: " + f.Length);
               
                int i = x.Length;
                int j = y.Length;
                while (i > 0 || j > 0)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                    {
                        i--;
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                    {
                        j--;
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                    {
                        i--;
                        j--;
                    }
                   

                    // Extract data based on the matched frames (i and j)
                    if (i < x.Length && j < y.Length)
                    {
                        double valueFromX = x[i];  // Value from array x at index i
                        double valueFromY = y[j];  // Value from array y at index j

                        // Do something with the extracted data (e.g., print or store it)
                        Debug.Log("i =" + i + " : " + valueFromX + " j = " + j + " : " + valueFromY + " Difference = " + (valueFromX - valueFromY));
                        
                    }
                }

                sum = simpleDTW.getSum();
            //counter = 0;
            CounterA = 0;
            CounterB = 0;
            }
            else
            {
                CounterA++;
                CounterB++;
                //counter++;
            }
     }
}
