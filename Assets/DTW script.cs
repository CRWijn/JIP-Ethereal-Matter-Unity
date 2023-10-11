using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DTW
{

    public class OEDTW
    {
        double[] x;
        double[] y;
        double[,] distance;
        double[,] f;
        public ArrayList pathX;
        public ArrayList pathY;
        ArrayList distanceList;

        public OEDTW(double[] _live, double[] _ref)
        {
            x = _live;
            y = _ref;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 1];

            for (int i = 0; i < x.Length; ++i)
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }

            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
            }
            for (int j = 1; j <= y.Length; ++j)
            {
                f[0, j] = 0;
            }

            f[0, 0] = 0.0;

            pathX = new ArrayList();
            pathY = new ArrayList();
            distanceList = new ArrayList();
        }

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
        public ArrayList pathX;
        public ArrayList pathY;
        ArrayList distanceList;
        double sum;

        public SimpleDTW(double[] _x, double[] _y)
        {
            x = _x;
            y = _y;
            distance = new double[x.Length, y.Length];
            f = new double[x.Length + 1, y.Length + 1];

            for (int i = 0; i < x.Length; ++i)
            {
                for (int j = 0; j < y.Length; ++j)
                {
                    distance[i, j] = Math.Abs(x[i] - y[j]);
                }
            }

            for (int i = 0; i <= x.Length; ++i)
            {
                for (int j = 0; j <= y.Length; ++j)
                {
                    f[i, j] = -1.0;
                }
            }

            for (int i = 1; i <= x.Length; ++i)
            {
                f[i, 0] = double.PositiveInfinity;
            }
            for (int j = 1; j <= y.Length; ++j)
            {
                f[0, j] = double.PositiveInfinity;
            }

            f[0, 0] = 0.0;
            sum = 0.0;

            pathX = new ArrayList();
            pathY = new ArrayList();
            distanceList = new ArrayList();
        }

        public ArrayList getPathX()
        {
            return pathX;
        }

        public ArrayList getPathY()
        {
            return pathY;
        }

        public double getSum()
        {
            return sum;
        }

        public double[,] getFMatrix()
        {
            return f;
        }

        public ArrayList getDistanceList()
        {
            return distanceList;
        }

        public void computeDTW()
        {
            sum = computeFBackward(x.Length, y.Length);
            //sum = computeFForward();
        }

        public double computeFForward()
        //Self-made DTW
            // {
            //     for (int i = 1; i <= x.Length; i++)
            //     { 
            //         f[i,1] = f[i-1,1] + distance[i,1];
            //     }
            //     for (int j = 1; j <= y.Length; j++)
            //     { 
            //         f[1,j] = f[1,j-1] + distance[1,j];
            //     }              
            //     for (int i = 1; i <= x.Length; ++i)
            //     {
            //         for (int j = 1; j <= y.Length; ++j)
            //         {
            //             if (f[i - 1, j] <= f[i - 1, j - 1] && f[i - 1, j] <= f[i, j - 1])
            //             {
            //                 f[i, j] = distance[i, j] + f[i - 1, j];
            //             }
            //             else if (f[i, j - 1] <= f[i - 1, j - 1] && f[i, j - 1] <= f[i - 1, j])
            //             {
            //                 f[i, j] = distance[i, j] + f[i, j - 1];
            //             }
            //             else if (f[i - 1, j - 1] <= f[i, j - 1] && f[i - 1, j - 1] <= f[i - 1, j])
            //             {
            //                 f[i, j] = distance[i, j] + f[i - 1, j - 1];
            //             }
            //         }
            //     }
            //     return f[x.Length, y.Length];
            // }
        
        
        
        
        
        
        
        
        //Original DTW
        
        {
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

        public double computeFBackward(int i, int j)
        {
            if (!(f[i, j] < 0.0))
            {
                return f[i, j];
            }
            else
            {
                if (computeFBackward(i - 1, j) <= computeFBackward(i, j - 1) && computeFBackward(i - 1, j) <= computeFBackward(i - 1, j - 1)
                    && computeFBackward(i - 1, j) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j);
                }
                else if (computeFBackward(i, j - 1) <= computeFBackward(i - 1, j) && computeFBackward(i, j - 1) <= computeFBackward(i - 1, j - 1)
                    && computeFBackward(i, j - 1) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i, j - 1);
                }
                else if (computeFBackward(i - 1, j - 1) <= computeFBackward(i - 1, j) && computeFBackward(i - 1, j - 1) <= computeFBackward(i, j - 1)
                    && computeFBackward(i - 1, j - 1) < double.PositiveInfinity)
                {
                    f[i, j] = distance[i - 1, j - 1] + computeFBackward(i - 1, j - 1);
                }
            }
            return f[i, j];
        }


    }
}