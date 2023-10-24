using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace DTW
{
    // Open Begin-End DTW
    public class OBEDTW
    {
        double[] x;
        double[] y;
        double[,] distance;
        double[,] f;

        public OBEDTW(double[] _live, double[] _ref)
        {
            x = _live;
            y = _ref;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 2];

            for (int i = 0; i < x.Length; ++i) // Calculate distances between every single frame of the live and ref
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }
            
            // Write -1 to each element of the matrix
            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length+1; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            // Set the live axis to infinite
            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
                f[i, 1] = double.PositiveInfinity;
            }
            // Set the ref axis to 0
            for (int j = 0; j <= y.Length; ++j)
            {
                f[0, j] = 0;
            }
        }

        // Get the cumulative distances matrix
        public double[,] getFMatrix()
        {
            return f;
        }

        public Tuple<int[], int[]> computeDTW()
        {
            computeFForward();
            int endNdx = smallestIndex();
            Tuple<List<int>,List<int>> indices = findPath(endNdx);
            int[] i = indices.Item1.ToArray();
            int[] j = indices.Item2.ToArray();
            return new Tuple<int[], int[]>(i, j);
        }

        public void computeFForward()
        {
            // You can implement different kind of minimum check patterns this one checks the previous one in live, the prev one in ref and the diagonal prev one
            for (int i = 1; i < f.GetLength(0); ++i)
            {
                for (int j = 2; j < f.GetLength(1); ++j)
                {
                    // Diag 1
                    if (f[i - 1, j - 1] <= f[i - 1, j] && f[i - 1, j - 1] <= f[i - 1, j - 2])
                    {
                        f[i, j] = distance[i - 1, j - 2] + f[i - 1, j - 1];
                    }
                    // Diag 2
                    else if (f[i - 1, j - 2] <= f[i - 1, j] && f[i - 1, j - 2] <= f[i - 1, j - 1])
                    {
                        f[i, j] = distance[i - 1, j - 2] + f[i - 1, j - 2];
                    }
                    // Down
                    else
                    {
                        f[i, j] = distance[i - 1, j - 2] + f[i - 1, j];
                    }
                }
            }
        }

        // This function gets the smallest number in an array and returns the index of it
        public int smallestIndex()
        {
            int ndx = 0;
            int i = f.GetLength(0) - 1;
            double startPoint = double.PositiveInfinity;
            for (int j = 2; j < f.GetLength(1); j++)
            {
                if (f[i, j] < startPoint)
                {
                    startPoint = f[i, j];
                    ndx = j;
                }
            }
            return ndx;
        }

        public Tuple<List<int>,List<int>> findPath(int end)
        {
            int i = f.GetLength(0) - 1;
            int j = end;
            List<int> iList = new List<int>();
            List<int> jList = new List<int>();
            iList.Add(i);
            jList.Add(j);

            while (i > 1)
            {
                // Diag 1
                if (f[i - 1, j - 1] <= f[i - 1, j] && f[i - 1, j - 1] <= f[i - 1, j - 2])
                {
                    i--;
                    j--;
                }
                // Diag 2
                else if (f[i - 1, j - 2] <= f[i - 1, j] && f[i - 1, j - 2] <= f[i - 1, j - 1])
                {
                    i--;
                    j -= 2;
                }
                // Down
                else
                {
                    i--;
                }
                iList.Add(i);
                jList.Add(j);
            }
            return new Tuple<List<int>,List<int>>(iList, jList);
        }
    }

    // Open Begin DTW
    public class OEDTW
    {
        double[] x;
        double[] y;
        double[,] distance;
        double[,] f;

        public OEDTW(double[] _live, double[] _ref)
        {
            x = _live;
            y = _ref;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 1];

            for (int i = 0; i < x.Length; ++i) // Calculate distances between every single frame of the live and ref
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }
            
            // Write -1 to each element of the matrix
            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            // Set the live axis to infinite
            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
            }
            // Set the ref axis to 0
            for (int j = 1; j <= y.Length; ++j)
            {
                f[0, j] = 0;
            }

            // Initial value of 0
            f[0, 0] = 0.0;
        }

        // Get the cumulative distances matrix
        public double[,] getFMatrix()
        {
            return f;
        }

        public void computeDTW()
        {
            computeFForward();
        }

        public void computeFForward()
        {
            // You can implement different kind of minimum check patterns this one checks the previous one in live, the prev one in ref and the diagonal prev one
            for (int i = 1; i <= x.Length; ++i)
            {
                for (int j = 1; j <= y.Length; ++j)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j];
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i, j - 1];
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j - 1];
                    }
                }
            }
        }

    }
    public class SimpleDTW
    {
        double[] x;
        double[] y;
        double[,] distance;
        double[,] f;

        // Standard DTW
        public SimpleDTW(double[] _x, double[] _y)
        {
            x = _x;
            y = _y;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 1];

            // Calculate distance from each ref frame to each live frame
            for (int i = 0; i < x.Length; ++i)
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }

            // Put -1 in each element of the f matrix
            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            // Set all values on the ref and live axis to infinite
            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
            }
            for (int j = 1; j <= y.Length; ++j)
            {
                f[0, j] = double.PositiveInfinity;
            }

            // Initial cumulation value
            f[0, 0] = 0.0;
        }

        // Get cumulative distances matrix
        public double[,] getFMatrix()
        {
            return f;
        }

        // Compute DTW
        public void computeDTW()
        {
            computeFForward();
        }

        public double computeFForward()        
        {
            // You can implement different kind of minimum check patterns this one checks the previous one in live, the prev one in ref and the diagonal prev one
            for (int i = 1; i <= x.Length; ++i)
            {
                for (int j = 1; j <= y.Length; ++j)
                {
                    if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j];
                    }
                    else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i, j - 1];
                    }
                    else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
                    {
                        f[i, j] = distance[i - 1, j - 1] + f[i - 1, j - 1];
                    }
                }
            }
            return f[x.Length, y.Length];
        }
    }
}