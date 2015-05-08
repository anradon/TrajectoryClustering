using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
namespace MDWConsole
{
    public class Point
    {
        //public const int NOISE = -1;
        //public const int UNCLASSIFIED = 0;
        public double X, Y, SOG, COG;
        //public double Heading, ROT;
        public DateTime Time;
        public long VoyageID, MMSI;
        //public int ClusterId;
        public Point(double x, double y, double SOG, double COG,  DateTime Time, long VoyageID, long MMSI)
        {
            this.X = x;
            this.Y = y;
            this.SOG = SOG;
            this.COG = COG;
            //this.Heading = Heading;
            //this.ROT = ROT;
            this.Time = Time;
            this.VoyageID = VoyageID;
            this.MMSI = MMSI;
        }
        
    }




    public class TSC
    {
        public static int[] ClusterID;
        
        public void myTSC(string segmentFilePath, string patternPath, int totalDuration, int totalPartition, int partitionWindow)
        {
            //double paritionWindow = 1;
            int partition = 1;
            while (partition <= totalPartition)
            {
                double eps;
                int minPts = 8;


                String filename = segmentFilePath + partitionWindow + "_" + partition + ".txt";
                System.IO.StreamReader Objreader;
                Objreader = new System.IO.StreamReader(filename);

               
                int lineCount = 0;

                lineCount = System.IO.File.ReadLines(filename).Count();

                if (lineCount > 8)
                {
                    List<List<Point>> Tracks = new List<List<Point>>();
                    List<List<Point>> iTracks = new List<List<Point>>();
                    ClusterID = new int[lineCount];
                    for (int i = 0; i < lineCount; i++)
                    {
                        Tracks.Add(new List<Point>());
                        iTracks.Add(new List<Point>());
                    }


                    for (int i = 0; i < lineCount; i++)
                    {

                        string text = Objreader.ReadLine();
                        string[] bits = text.Split(' ');

                        for (int k = 0; k < bits.Length - 1; k = k + 9)
                        {
                            double longitude = double.Parse(bits[k]);
                            double latitude = double.Parse(bits[k + 1]);
                            double SOG = double.Parse(bits[k + 2]);
                            double COG = double.Parse(bits[k + 3]);
                            string datefield = bits[k + 4];
                            string timefield = bits[k + 5];
                            string timeExt = bits[k + 6];
                            string newTime = datefield + " " + timefield + " " + timeExt;
                            DateTime Time = Convert.ToDateTime(newTime);
                            long VoyageID = long.Parse(bits[k + 7]);
                            long MMSI = long.Parse(bits[k + 8]);
                            Tracks[i].Add(new Point(longitude, latitude, SOG, COG, Time, VoyageID, MMSI));
                            iTracks[i].Add(new Point(longitude, latitude, SOG, COG, Time, VoyageID, MMSI));

                        }

                    }
                    Objreader.Close();
                    for (int i = 0; i < Tracks.Count; i++)
                    {
                        if (Tracks[i].Count < 2)
                        {
                            Tracks.RemoveAt(i);
                            iTracks.RemoveAt(i);
                            i = i - 1;
                        }
                    }

                    //-----------Normalizing-----------------------
                    for (int i = 0; i < Tracks.Count; i++)
                    {

                        for (int j = 0; j < Tracks[i].Count; j++)
                        {
                            double length = Math.Sqrt(Math.Pow(Tracks[i][j].X, 2) + Math.Pow(Tracks[i][j].Y, 2) + Math.Pow(Tracks[i][j].SOG, 2) + Math.Pow(Tracks[i][j].COG, 2));
                            Tracks[i][j].X = Tracks[i][j].X / length;
                            Tracks[i][j].Y = Tracks[i][j].Y / length;
                            Tracks[i][j].SOG = Tracks[i][j].SOG / length;
                            Tracks[i][j].COG = Tracks[i][j].COG / length;
                            //Tracks[i][j].Heading = Tracks[i][j].Heading / length;
                            //Tracks[i][j].ROT = Tracks[i][j].ROT / length;


                        }


                    }

                    //---------END of Normalization----------------




                    for (int i = 0; i < lineCount; i++)
                    {
                        ClusterID[i] = 0;

                    }

                    Console.WriteLine("Calculating sorted 8-dist value of each Trajectory of segment:{0}", partition);
                    List<int> anomalyIndex = new List<int>();
                    EpsilonEstimation epsEst = new EpsilonEstimation();
                    anomalyIndex = epsEst.epsFinder(filename, patternPath, partition, partitionWindow);

                    Console.WriteLine("Enter the trajectory number of the first valley in the 8-dist plot:");
                    int tp = Convert.ToInt32(Console.ReadLine());

                    string myRoot = Settings1.Default.rootPath;
                    String epsfile = myRoot + patternPath + partitionWindow + "_eps_" + partition + ".txt";
                    System.IO.StreamReader Objreader2;
                    Objreader2 = new System.IO.StreamReader(epsfile);
                    for (int i = 0; i < tp; i++)
                    {
                        //Console.WriteLine("Test anomalies are {0}", anomalyIndex[i]);
                        ClusterID[anomalyIndex[i]] = -1;

                    }

                    string text2 = Objreader2.ReadLine();
                    string[] bits2 = text2.Split(' ');
                    eps = double.Parse(bits2[tp - 1]);
                    Objreader2.Close();
                    Console.WriteLine("Chosen eps value is: {0}", eps);

                    TSC obj = new TSC();
                    List<List<int>> clusters = obj.GetClusters(Tracks, eps, minPts);

                    Console.WriteLine("Total number of Clusters {0}", clusters.Count);

                    int totalNormalTracks = 0;
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        Console.WriteLine("Total tracks in cluster {0} :{1}", i + 1, clusters[i].Count);
                        totalNormalTracks = totalNormalTracks + clusters[i].Count;

                    }

                    string root = Settings1.Default.rootPath;
                    String filename2 = root + patternPath + partitionWindow + "_" + partition + ".txt";  //ids of normal tracks will be stored here
                    System.IO.StreamWriter Objwriter2;
                    Objwriter2 = new System.IO.StreamWriter(filename2, true);


                    List<List<List<double>>> statList = new List<List<List<double>>>();
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        statList.Add(new List<List<double>>());
                        for (int j = 0; j < 4; j++)
                        {
                            statList[i].Add(new List<double>());

                        }

                    }


                    for (int i = 0; i < clusters.Count; i++)
                    {

                        foreach (var j in clusters[i])
                        {

                            Objwriter2.Write(j + 1);
                            Objwriter2.Write(" ");


                        }

                        Objwriter2.WriteLine();

                    }
                    Objwriter2.Close();
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        for (int j = 0; j < clusters[i].Count; j++)
                        {


                            for (int k = 0; k < iTracks[clusters[i][j]].Count; k++)
                            {
                                statList[i][0].Add(iTracks[clusters[i][j]][k].X);
                                statList[i][1].Add(iTracks[clusters[i][j]][k].Y);
                                statList[i][2].Add(iTracks[clusters[i][j]][k].SOG);
                                statList[i][3].Add(iTracks[clusters[i][j]][k].COG);
                                //statList[i][4].Add(iTracks[clusters[i][j]][k].Heading);
                                //statList[i][5].Add(iTracks[clusters[i][j]][k].ROT);
                            }

                        }

                    }

                    for (int i = 0; i < clusters.Count; i++)
                    {
                        String normalRangePath = root + patternPath + partitionWindow + "_" + partition + "_range_" + (i + 1) + ".txt";
                        System.IO.StreamWriter ObjNormalRangeWriter = new System.IO.StreamWriter(normalRangePath, true);
                        for (int j = 0; j < 4; j++)
                        {
                            ObjNormalRangeWriter.Write(statList[i][j].Min());
                            ObjNormalRangeWriter.Write(" ");
                            ObjNormalRangeWriter.Write(statList[i][j].Max());
                            ObjNormalRangeWriter.Write(" ");
                            ObjNormalRangeWriter.WriteLine(statList[i][j].Average());
                        }
                        ObjNormalRangeWriter.Close();

                    }

                    //-------------storing patterns into data warehouse-------------------------
                     SqlConnection myConnection1 = new SqlConnection("Data Source=TASC1-9006-01;Initial Catalog=MDW;Integrated Security=True");
                     try
                     {
                         myConnection1.Open();
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e.ToString());
                     }
                     string patternTable = "[Pattern-" + StorePattern.zone + "]";
                     String TrackSegmentList = "'"+ patternPath + partitionWindow + "_" + partition + ".txt'";
                     string sy = "'"+ StorePattern.syear +"'";
                     string sm = "'" + StorePattern.smonth + "'";
                     string ey = "'" + StorePattern.eyear + "'";
                     string em = "'" + StorePattern.emonth + "'";
                     string orig = "'" + StorePattern.origin + "'";
                     string dest = "'" + StorePattern.destination + "'";
                     string vType = "'" + StorePattern.vesseltype + "'";

                     String s1 = "insert into " + patternTable + "values("+sy+","+sm+","+ey+","+em+","+orig+","+dest+","+vType+","+totalDuration+","+ partitionWindow+","+partition+","+eps +","+lineCount+","+clusters.Count+","+ TrackSegmentList+")";
                     SqlCommand myCommand1 = new SqlCommand(s1, myConnection1);
                     myCommand1.ExecuteNonQuery();

                     try
                     {
                         myConnection1.Close();
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e.ToString());
                     }
                     //-------------end of storing patterns into data warehouse-------------------------
                    Console.WriteLine("----------------------------------------");
                }
                else 
                {
                    Console.WriteLine("Too few tracks to form cluster!");
                
                }

                partition++;
            }

            Console.WriteLine("Pattern mining completed!");
            Console.WriteLine("----------------------------------------");
        }

        public  List<List<int>> GetClusters(List<List<Point>> Tracks, double eps, int minPts)
        {

            if (Tracks == null) return null;
            List<List<int>> clusters = new List<List<int>>();
            //eps *= eps; // square eps

            int clusterId = 1;
            for (int i = 0; i < Tracks.Count; i++)
            {
                List<Point> t = Tracks[i];
                if (TSC.ClusterID[i] == 0)
                {

                    if (ExpandCluster(Tracks, t, clusterId, eps, minPts))
                        clusterId++;
                }

            }
            
            int highest = ClusterID.Max();
            for (int i = 0; i < highest; i++)
            {
                clusters.Add(new List<int>());
            }

            for (int i = 0; i < highest; i++)
            {
                for (int j = 0; j < ClusterID.Length; j++)
                {
                    if ((i + 1) == ClusterID[j])
                    {

                        clusters[i].Add(j);

                    }

                }

            }



            return clusters;
        }

        public List<List<Point>> GetRegion(List<List<Point>> Tracks, List<Point> t, double eps)
        {

            List<List<Point>> region = new List<List<Point>>();
            int count_P = t.Count;
            int t_position = Tracks.IndexOf(t);
            //Console.WriteLine("Printing region of Track {0}",t_position);
            double[,] P = new double[count_P, 4];
            for (int j = 0; j < count_P; j++)
            {
                P[j, 0] = t[j].X;
                P[j, 1] = t[j].Y;
                P[j, 2] = t[j].SOG;
                P[j, 3] = t[j].COG;
                //P[j, 4] = t[j].Heading;
                //P[j, 5] = t[j].ROT;

            }


            int[] flag = new int[Tracks.Count];
            int counter = 0;
            for (int i = 0; i < Tracks.Count; i++)
            {

                int count_Q = Tracks[i].Count;
                double[,] Q = new double[count_Q, 4];

                for (int j = 0; j < count_Q; j++)
                {
                    Q[j, 0] = Tracks[i][j].X;
                    Q[j, 1] = Tracks[i][j].Y;
                    Q[j, 2] = Tracks[i][j].SOG;
                    Q[j, 3] = Tracks[i][j].COG;
                    //Q[j, 4] = Tracks[i][j].Heading;
                   // Q[j, 5] = Tracks[i][j].ROT;


                }

                RouteSimilarityDistance rsd = new RouteSimilarityDistance();
                double totaldistance = rsd.distanceFunction(P, Q);

                if (totaldistance <= eps)
                {

                    flag[counter] = i;

                    region.Add(Tracks[i]);
                }
            }


            return region;
        }
        public bool ExpandCluster(List<List<Point>> Tracks, List<Point> t, int clusterId, double eps, int minPts)
        {
            List<List<Point>> seeds = GetRegion(Tracks, t, eps);
            //Console.WriteLine(seeds.Count);
            if (seeds.Count < minPts) // no core point
            {
                int position = Tracks.IndexOf(t);
                ClusterID[position] = -1;
                return false;
            }
            else // all points in seeds are density reachable from point 'p'
            {

                for (int i = 0; i < seeds.Count; i++)
                {
                    int position = Tracks.IndexOf(seeds[i]);
                    if (ClusterID[position] != -1)
                    {
                        ClusterID[position] = clusterId;
                    }

                   
                }
                seeds.Remove(t);
                while (seeds.Count > 0)
                {
                    List<Point> currentt = seeds[0];
                    List<List<Point>> result = GetRegion(Tracks, currentt, eps);
                    if (result.Count >= minPts)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            List<Point> resultP = result[i];
                            int position = Tracks.IndexOf(resultP);
                            //Console.WriteLine(position);
                            if (ClusterID[position] == 0 || ClusterID[position] == -1)
                            {
                                if (ClusterID[position] == 0)
                                {
                                    seeds.Add(resultP);
                                    ClusterID[position] = clusterId;

                                }
                            }
                        }
                    }

                    seeds.Remove(currentt);
                }
                return true;
            }
        }
    }
}
