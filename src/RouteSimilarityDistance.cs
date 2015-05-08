using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDWConsole
{
    public class RouteSimilarityDistance
    {
        public double distance, penalty, n, matching_length;
        public double last_common_point_y1, last_common_point_y2, last_common_point_y3, last_common_point_y4;

        public RouteSimilarityDistance()
        {
            distance = penalty = n = matching_length = 0;
            last_common_point_y1 = last_common_point_y2 = last_common_point_y3 = last_common_point_y4 = 1000;

        }
        public double point_distance(double x1, double x2, double x3, double x4, double y1, double y2, double y3, double y4)
        {
            double d = 0;
            return d = Math.Sqrt(Math.Pow((x1 - y1), 2) + Math.Pow((x2 - y2), 2) + Math.Pow((x3 - y3), 2) + Math.Pow((x4 - y4), 2));
            /*x1 = x1 *Math.PI / 180; 
            y1 = y1 * Math.PI / 180; 
            x2 = x2 * Math.PI / 180;
            y2 = y2 * Math.PI / 180; 
            double d = 2 * Math.Asin(Math.Sqrt(Math.Pow((Math.Sin((y1 - y2) / 2)), 2) + Math.Cos(y1) * Math.Cos(y2) * Math.Pow((Math.Sin((x1 - x2) / 2)), 2)));
            d = (d * 180 * 60) / Math.PI;
            return d;*/
        }

        public Tuple<double, double, double, double> get_middle_point(double x1, double x2, double x3, double x4,  double y1, double y2, double y3, double y4)
        {
            double z1 = (x1 + y1) / 2;
            double z2 = (x2 + y2) / 2;
            double z3 = (x3 + y3) / 2;
            double z4 = (x4 + y4) / 2;
            //double z5 = (x5 + y5) / 2;
            //double z6 = (x6 + y6) / 2;
            return Tuple.Create(z1, z2, z3, z4);

        }

        public double distanceFunction(double[,] P, double[,] Q)
        {
            //Console.WriteLine(P.GetLength(0));

            int i = 0, j = 0;
            while (i < P.GetLength(0) && j < Q.GetLength(0))
            {
                //Console.WriteLine(Q.GetLength(0));
                double d = point_distance(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]);

                //Console.WriteLine(d);
                int loop_i = i;
                while (loop_i + 1 < P.GetLength(0))
                {
                    if (point_distance(P[loop_i + 1, 0], P[loop_i + 1, 1], P[loop_i + 1, 2], P[loop_i + 1, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]) < d)
                    {
                        penalty = penalty + point_distance(P[loop_i, 0], P[loop_i, 1], P[loop_i, 2], P[loop_i, 3],  P[loop_i + 1, 0], P[loop_i + 1, 1], P[loop_i + 1, 2], P[loop_i + 1, 3]);
                        loop_i = loop_i + 1;
                        d = point_distance(P[loop_i, 0], P[loop_i, 1], P[loop_i, 2], P[loop_i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]);

                    }
                    else
                    {
                        loop_i = loop_i + 1;
                        //Console.WriteLine(i);

                    }

                }
                int loop_j = j;
                while (loop_j + 1 < Q.GetLength(0))
                {
                    if (point_distance(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[loop_j + 1, 0], Q[loop_j + 1, 1], Q[loop_j + 1, 2], Q[loop_j + 1, 3]) < d)
                    {
                        penalty = penalty + point_distance(Q[loop_j, 0], Q[loop_j, 1], Q[loop_j, 2], Q[loop_j, 3], Q[loop_j + 1, 0], Q[loop_j + 1, 1], Q[loop_j + 1, 2], Q[loop_j + 1, 3]);
                        loop_j = loop_j + 1;
                        d = point_distance(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[loop_j, 0], Q[loop_j, 1], Q[loop_j, 2], Q[loop_j, 3]);

                    }

                    else
                    {
                        loop_j = loop_j + 1;
                        //Console.WriteLine(i);

                    }


                }
                distance = distance + d;
                //Console.WriteLine("distance {0}:{1}",i,distance);
                n = n + 1;
                double common_point_x1 = get_middle_point(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]).Item1;
                double common_point_x2 = get_middle_point(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]).Item2;
                double common_point_x3 = get_middle_point(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]).Item3;
                double common_point_x4 = get_middle_point(P[i, 0], P[i, 1], P[i, 2], P[i, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]).Item4;
               
                if (last_common_point_y1 != 1000 && last_common_point_y2 != 1000 && last_common_point_y3 != 1000 && last_common_point_y4 != 1000)
                {
                    matching_length = matching_length + point_distance(last_common_point_y1, last_common_point_y2, last_common_point_y3, last_common_point_y4, common_point_x1, common_point_x2, common_point_x3, common_point_x4);


                }

                last_common_point_y1 = common_point_x1;
                last_common_point_y2 = common_point_x2;
                last_common_point_y3 = common_point_x3;
                last_common_point_y4 = common_point_x4;
                
                i = i + 1;
                j = j + 1;
                //Console.WriteLine(i);
            }
            if (n < 1)
            {
                Console.WriteLine("INF");
                return 1000000000000;

            }
            distance = distance / n;
            //Console.WriteLine(distance);

            //Console.WriteLine(i);
            //Console.WriteLine(j);

            while (i < P.GetLength(0))
            {
                penalty = penalty + point_distance(P[i - 1, 0], P[i - 1, 1], P[i - 1, 2], P[i - 1, 3], P[i, 0], P[i, 1], P[i, 2], P[i, 3]);
                i = i + 1;

            }

            while (j < Q.GetLength(0))
            {
                penalty = penalty + point_distance(Q[j - 1, 0], Q[j - 1, 1], Q[j - 1, 2], Q[j - 1, 3], Q[j, 0], Q[j, 1], Q[j, 2], Q[j, 3]);
                j = j + 1;

            }

            //Console.WriteLine(matching_length);

            if (penalty > 0 && matching_length > 0)
            {
                double ratio = penalty / (matching_length);
                //penalty = Math.Sqrt(penalty * ratio);
                penalty = penalty * ratio;

            }
            //Console.WriteLine("Total Distance is:");

            return distance + penalty;

        }


        public static void main()
        {
            //double distance = 0, penalty = 0, n = 0, matching_length = 0;
            //double last_common_point_x = 1000, last_common_point_y = 1000;
            //int i = 0, j = 0;
            double[,] P = new double[5, 2] { { 1, 2 }, { 6, 4 }, { 3, 6 }, { 4, 8 }, { 5, 10 } };
            double[,] Q = new double[4, 2] { { 2, 2 }, { 4, 4 }, { 6, 6 }, { 8, 8 } };

            RouteSimilarityDistance rsd = new RouteSimilarityDistance();

            //double totaldistance = rsd.distanceFunction(P,Q);
            //Console.WriteLine(totaldistance);
            //double d = rsd.point_distance(-122.607933, 47.21216,-122.601715, 47.21357);

            //Console.WriteLine(d);

        }




    }


}

