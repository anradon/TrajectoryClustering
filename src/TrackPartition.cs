using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace MDWConsole
{
    class TrackPartition
    {
        
        
        public static double point_distance(double x1, double y1, double x2, double y2)
        {
            //double d = 0;
            //return d = Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
            x1 = x1 * Math.PI / 180;
            y1 = y1 * Math.PI / 180;
            x2 = x2 * Math.PI / 180;
            y2 = y2 * Math.PI / 180;
            double d = 2 * Math.Asin(Math.Sqrt(Math.Pow((Math.Sin((y1 - y2) / 2)), 2) + Math.Cos(y1) * Math.Cos(y2) * Math.Pow((Math.Sin((x1 - x2) / 2)), 2)));
            d = (d * 180 * 60) / Math.PI;
            return d;
        }
       

        public void trackPartitioner(List<List<AISPoint>> Tracks,string filePath, int tripDuration,int partitionWindow)
        {

            //-------------------------------------Partitioning Trajectories on hourly interval-------------------------------------------------
            //Console.WriteLine("Starting Partitioning based on {0} hour interval", partitionWindow);
             Stopwatch stopWatch = new Stopwatch();
             stopWatch.Start();
             int totalNumberofFiles = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tripDuration) / Convert.ToDouble(partitionWindow)));
             for (int i = 1; i <= totalNumberofFiles; i++)
             {
                 String filename = filePath + partitionWindow + "_" + i + ".txt";
                 System.IO.StreamWriter Objwriter;
                 Objwriter = new System.IO.StreamWriter(filename, true);
                 Objwriter.Close();

             }
             for (int i = 0; i < Tracks.Count; i++)
             {
                 double totalduration = 0;
                 int fileCounter = 1;
                 int start = 0;
                 int end = 0;
                 TimeSpan totalDuration = Tracks[i][Tracks[i].Count - 1].Time - Tracks[i][0].Time;
                 int totalFileCounter = Convert.ToInt32(Math.Ceiling(totalDuration.TotalHours / Convert.ToDouble(partitionWindow)));
                 for (int j = start; j < Tracks[i].Count - 1; j++)
                 {
                     //int start = j;
                     TimeSpan duration = Tracks[i][j + 1].Time - Tracks[i][j].Time;
                     totalduration = totalduration + duration.TotalHours;
                     if (totalduration > partitionWindow)
                     {

                         end = j;
                         string path = filePath + partitionWindow + "_" + fileCounter + ".txt";
                         System.IO.StreamWriter Objwriter2 = new StreamWriter(path, true);
                         for (int k = start; k <= end; k++)
                         {
                             string wt = Tracks[i][k].X.ToString() + " " + Tracks[i][k].Y.ToString() + " " + Tracks[i][k].SOG.ToString() + " " + Tracks[i][k].COG.ToString()  + " " + Tracks[i][k].Time.ToString() + " " + Tracks[i][k].VoyageID.ToString() + " " + Tracks[i][k].MMSI.ToString() + " ";
                             Objwriter2.Write(wt);

                         }

                         Objwriter2.WriteLine();
                         Objwriter2.Close();
                         start = end;
                         j = j - 1;
                         totalduration = 0;
                         fileCounter = fileCounter + 1;

                     }
                     if (fileCounter == totalFileCounter)
                     {
                         string path = filePath + partitionWindow + "_" + fileCounter + ".txt";
                         System.IO.StreamWriter Objwriter2 = new StreamWriter(path, true);

                         for (int k = start; k <= Tracks[i].Count - 1; k++)
                         {
                             string wt = Tracks[i][k].X.ToString() + " " + Tracks[i][k].Y.ToString() + " " + Tracks[i][k].SOG.ToString() + " " + Tracks[i][k].COG.ToString() +  " " + Tracks[i][k].Time.ToString() + " " + Tracks[i][k].VoyageID.ToString() + " " + Tracks[i][k].MMSI.ToString() + " ";
                             Objwriter2.Write(wt);

                         }

                         Objwriter2.WriteLine();
                         Objwriter2.Close();
                         break;

                     }

                 }


             }
             stopWatch.Stop();
             TimeSpan ts = stopWatch.Elapsed;
             //Console.WriteLine("Partitioning on hourly interval done");
             string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
             //Console.WriteLine("RunTime " + elapsedTime);
             //Console.WriteLine("--------------------------------------");
            //-------------------------------------End Partitioning Trajectories on hourly interval-------------------------------------------------


        
        }

        public void vizTrackPartitioner(List<List<AISPoint>> Tracks, string filePath, int tripDuration, int partitionWindow)
        {

            //-------------------------------------Partitioning Trajectories on hourly interval-------------------------------------------------
            //Console.WriteLine("Starting Partitioning based on {0} hour interval", partitionWindow);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int totalNumberofFiles = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(tripDuration) / Convert.ToDouble(partitionWindow)));
            for (int i = 1; i <= totalNumberofFiles; i++)
            {
                String filename = filePath + partitionWindow + "_" + i + ".txt";
                System.IO.StreamWriter Objwriter;
                Objwriter = new System.IO.StreamWriter(filename, true);
                Objwriter.Close();

            }
            for (int i = 0; i < Tracks.Count; i++)
            {
                double totalduration = 0;
                int fileCounter = 1;
                int start = 0;
                int end = 0;
                TimeSpan totalDuration = Tracks[i][Tracks[i].Count - 1].Time - Tracks[i][0].Time;
                int totalFileCounter = Convert.ToInt32(Math.Ceiling(totalDuration.TotalHours / Convert.ToDouble(partitionWindow)));
                for (int j = start; j < Tracks[i].Count - 1; j++)
                {
                    //int start = j;
                    TimeSpan duration = Tracks[i][j + 1].Time - Tracks[i][j].Time;
                    totalduration = totalduration + duration.TotalHours;
                    if (totalduration > partitionWindow)
                    {

                        end = j;
                        string path = filePath + partitionWindow + "_" + fileCounter + ".txt";
                        System.IO.StreamWriter Objwriter2 = new StreamWriter(path, true);
                        for (int k = start; k <= end; k++)
                        {
                            string wt = Tracks[i][k].X.ToString() + " " + Tracks[i][k].Y.ToString() + " ";
                            Objwriter2.Write(wt);

                        }

                        Objwriter2.WriteLine();
                        Objwriter2.Close();
                        start = end;
                        j = j - 1;
                        totalduration = 0;
                        fileCounter = fileCounter + 1;

                    }
                    if (fileCounter == totalFileCounter)
                    {
                        string path = filePath + partitionWindow + "_" + fileCounter + ".txt";
                        System.IO.StreamWriter Objwriter2 = new StreamWriter(path, true);

                        for (int k = start; k <= Tracks[i].Count - 1; k++)
                        {
                            string wt = Tracks[i][k].X.ToString() + " " + Tracks[i][k].Y.ToString() + " ";
                            Objwriter2.Write(wt);

                        }

                        Objwriter2.WriteLine();
                        Objwriter2.Close();
                        break;

                    }

                }


            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            //Console.WriteLine("Partitioning on hourly interval done");
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);
            //Console.WriteLine("--------------------------------------");
            //-------------------------------------End Partitioning Trajectories on hourly interval-------------------------------------------------



        }
    }
}

