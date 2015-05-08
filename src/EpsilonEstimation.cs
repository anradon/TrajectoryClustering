using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MDWConsole
{
    public class EpsilonEstimation
    {
        
        public List<int> epsFinder(string givenFile, string patternPath, int partition, double partitionWindow)
        {
            
            List<List<Point>> Tracks = new List<List<Point>>();
            //String filename = "Z:\\Large Scale Experiment\\Input\\TrajectoryToVanFromSource\\JanAugust\\Partition\\Time\\1.txt";
            string filename = givenFile;
            System.IO.StreamReader Objreader;
            Objreader = new System.IO.StreamReader(filename);
            int lineCount = 0;

            lineCount = System.IO.File.ReadLines(filename).Count();
            //Console.WriteLine("Total Lines:{0}",lineCount);

            for (int i = 0; i < lineCount; i++)
            {
                Tracks.Add(new List<Point>());
            }

          

            for (int i = 0; i < lineCount; i++)
            {

                string text = Objreader.ReadLine();
                string[] bits = text.Split(' ');
                //Console.WriteLine("Bit length at iteration {0}: {1}",i+1,bits.Length-1);
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


                }

            }
            Objreader.Close();
           
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

            List<List<double>> distanceList = new List<List<double>>();
            for (int i = 0; i < lineCount; i++)
            {
                distanceList.Add(new List<double>());
            }

            for (int i = 0; i < distanceList.Count; i++)
            {
                double[,] P = new double[Tracks[i].Count, 4];
                for (int j = 0; j < Tracks[i].Count; j++)
                {
                    P[j, 0] = Tracks[i][j].X;
                    P[j, 1] = Tracks[i][j].Y;
                    P[j, 2] = Tracks[i][j].SOG;
                    P[j, 3] = Tracks[i][j].COG;
                    //P[j, 4] = Tracks[i][j].Heading;
                   // P[j, 5] = Tracks[i][j].ROT;


                }
                for (int k = 0; k < Tracks.Count; k++)
                {
                    double[,] Q = new double[Tracks[k].Count, 4];
                    for (int j = 0; j < Tracks[k].Count; j++)
                    {
                        Q[j, 0] = Tracks[k][j].X;
                        Q[j, 1] = Tracks[k][j].Y;
                        Q[j, 2] = Tracks[k][j].SOG;
                        Q[j, 3] = Tracks[k][j].COG;
                        //Q[j, 4] = Tracks[k][j].Heading;
                        //Q[j, 5] = Tracks[k][j].ROT;


                    }
                    RouteSimilarityDistance rsd = new RouteSimilarityDistance();
                    double totaldistance = rsd.distanceFunction(P, Q);
                    distanceList[i].Add(totaldistance);

                }



            }

            for (int i = 0; i < distanceList.Count; i++)
            {

                
                distanceList[i].Sort();

            }
            List<double> sortedKDist = new List<double>();

            for (int i = 0; i < distanceList.Count; i++)
            {
                double d = distanceList[i].ElementAt(8);
                sortedKDist.Add(d);
            }
           
            List<double> tempList = new List<double>();
            for (int i = 0; i < distanceList.Count; i++)
            {
                tempList.Add(sortedKDist[i]);

            }
            sortedKDist.Sort();
           
            sortedKDist.Reverse();

            string root = Settings1.Default.rootPath;

            String filename2 = root + patternPath+ partitionWindow+ "_eps_" +partition+".txt";
            System.IO.StreamWriter Objwriter;
            Objwriter = new System.IO.StreamWriter(filename2, true);
            foreach (var x in sortedKDist)
            {
                Objwriter.Write(x);
                Objwriter.Write(" ");
            }
            Objwriter.Close();
            List<int> anomalyIndex = new List<int>();
            for (int i = 0; i < sortedKDist.Count; i++)
            {
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (sortedKDist[i] == tempList[j])
                    {
                        anomalyIndex.Add(j);
                        break;
                    }

                }

            }
            Console.WriteLine("8-dist values are written to file");
            return anomalyIndex;
        }
    }
}

