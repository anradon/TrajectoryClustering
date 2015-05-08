using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDWConsole
{
    class AppController
    {

        public static void Main()
        {
            Console.WriteLine("Enter your choice:");
            Console.WriteLine("1 = Cleaning");
            Console.WriteLine("2 = Store Tracks");
            Console.WriteLine("3 = Mine Patterns");
            Console.WriteLine("4 = Anomaly Detection");
            Console.WriteLine("5 = Test Input Track Writing");
            string input = Console.ReadLine();
            if (input == "1")
            {

                Console.WriteLine("We will do cleaning now..........");
                Cleaning cl = new Cleaning();
                cl.cleaningMain();
            }
            else if (input == "2")
            {

                Console.WriteLine("We will store tracks now.........");
                ShipTrackCreation stc = new ShipTrackCreation();
                stc.shipTrackCreationMain();
            }
            else if (input == "3")
            {

                Console.WriteLine("We will mine patterns now..........");
                StorePattern sp = new StorePattern();
                sp.storePatternMain();
            }

            else if (input == "4")
            {

                Console.WriteLine("We will detect anomalies now..........");
                AnomalyDetector ad = new AnomalyDetector();
                ad.anomalyDetectorMain();
            }

            else if (input == "5")
            {

                Console.WriteLine("We will write test tracks into file for visualization..........");
                TestInputTrackVis ti = new TestInputTrackVis();
                ti.myVis();
            }
        
        }
    }
}
