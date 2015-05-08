using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace MDWConsole
{
    public class StorePattern
    {
        public static string zone, syear, smonth, eyear, emonth, vesseltype, origin, destination, root;
        public void storePatternMain()
        {
            SqlConnection myConnection = new SqlConnection("Data Source=TASC1-9006-01;Initial Catalog=MDW;Integrated Security=True");
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Enter Zone:");
            zone = Console.ReadLine();
            Console.WriteLine("Enter Start Year:");
            syear = Console.ReadLine();
            Console.WriteLine("Enter Start Month:");
            smonth = Console.ReadLine();
            Console.WriteLine("Enter End Year:");
            eyear = Console.ReadLine();
            Console.WriteLine("Enter End Month:");
            emonth = Console.ReadLine();
            Console.WriteLine("Enter Vessel Type:");
            vesseltype = Console.ReadLine();
            Console.WriteLine("Enter Origin:");
            origin = Console.ReadLine();
            Console.WriteLine("Enter Destination:");
            destination = Console.ReadLine();
            Console.WriteLine("Enter Partition Window in Hour:");
            int partitionWindow = Convert.ToInt32(Console.ReadLine());

            root = Settings1.Default.rootPath;
            string directory = root + "MDW\\Segments\\" + zone + "\\" + origin + "-" + destination + "\\" + vesseltype + "\\" + syear + smonth + "-" + eyear + emonth + "\\";
            string directoryPattern = "MDW\\Patterns\\" + zone + "\\" + origin + "-" + destination + "\\" + vesseltype + "\\" + syear + smonth + "-" + eyear + emonth + "\\";
            string s1 = null;
            string trackTable = "[Track-" + zone + "-" + syear + "]";
            int n = 0, i = 0;

            if (syear == eyear)
            {
                s1 = "select count(distinct TotalDuration) from " + trackTable + " where VesselType='" + vesseltype + "'and Origin='" + origin + "'and Destination='" + destination + "'and Month >= '" + smonth + "' and Month <= '" + emonth + "'";
                SqlCommand myCommand1 = new SqlCommand(s1, myConnection);
                myCommand1.CommandTimeout = 600;
                n = Convert.ToInt32(myCommand1.ExecuteScalar());
                //Console.WriteLine(n);
                int[] totalDurationArray = new int[n];
                s1 = "select distinct TotalDuration from " + trackTable + " where VesselType='" + vesseltype + "'and Origin='" + origin + "'and Destination='" + destination + "'and Month >= '" + smonth + "' and Month <= '" + emonth + "'";
                myCommand1 = new SqlCommand(s1, myConnection);
                myCommand1.CommandTimeout = 600;
                SqlDataReader myReader = null;
                myReader = myCommand1.ExecuteReader();
                while (myReader.Read())
                {
                    totalDurationArray[i++] = Convert.ToInt32(myReader["TotalDuration"].ToString());
                }
                myReader.Close();
                for (i = 0; i < n; i++)
                {
                    s1 = "select Sum(NumberofTracks) from " + trackTable + " where VesselType='" + vesseltype + "'and Origin='" + origin + "'and Destination='" + destination + "'and Month >= '" + smonth + "' and Month <= '" + emonth + "'" + " and TotalDuration=" + totalDurationArray[i];
                    myCommand1 = new SqlCommand(s1, myConnection);
                    myCommand1.CommandTimeout = 600;
                    int totalTracks = Convert.ToInt32(myCommand1.ExecuteScalar());

                    s1 = "select TrackPath from " + trackTable + " where VesselType='" + vesseltype + "'and Origin='" + origin + "'and Destination='" + destination + "'and Month >= '" + smonth + "' and Month <= '" + emonth + "'" + " and TotalDuration=" + totalDurationArray[i] + " order by Month";
                    myCommand1 = new SqlCommand(s1, myConnection);
                    myCommand1.CommandTimeout = 600;
                    myReader = myCommand1.ExecuteReader();
                    List<List<AISPoint>> Tracks = new List<List<AISPoint>>();
                    for (int j = 0; j < totalTracks; j++)
                    {
                        Tracks.Add(new List<AISPoint>());
                    }
                    int trackCounter = 0;
                    while (myReader.Read())
                    {

                        String fileName = root + myReader["TrackPath"].ToString();

                        System.IO.StreamReader Objreader;
                        Objreader = new System.IO.StreamReader(fileName);
                        int lineCount = System.IO.File.ReadLines(fileName).Count();

                        for (int j = 0; j < lineCount; j++)
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
                                Tracks[trackCounter].Add(new AISPoint(longitude, latitude, SOG, COG, Time, VoyageID, MMSI));


                            }
                            trackCounter++;



                        }
                        Objreader.Close();


                    }

                    myReader.Close();
                    //Console.WriteLine(Tracks.Count);
                    TrackPartition tp = new TrackPartition();
                    string filePath = directory + totalDurationArray[i] + "\\";
                    string patternPath = directoryPattern + totalDurationArray[i] + "\\";
                    tp.trackPartitioner(Tracks, filePath, totalDurationArray[i], partitionWindow);
                    Console.WriteLine("Starting pattern mining on {0} hour total duration tracks...", totalDurationArray[i]);
                    Console.WriteLine("Partioning Done!");
                    TSC tscObj = new TSC();
                    int totalPartition = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalDurationArray[i] )/ Convert.ToDouble(partitionWindow)));
                    
                    tscObj.myTSC(filePath, patternPath,totalDurationArray[i], totalPartition, partitionWindow);
                    Console.WriteLine("--------------------------------------------");

                }
            }



            try
            {
                myConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }
    }
}
