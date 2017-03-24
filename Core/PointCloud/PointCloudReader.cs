using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;
using System;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// This class loads an asset and fires callbacks whenever a new point gets loaded. May be used in the future to load
    /// points over a network stream.
    /// </summary>
    
    public class PointCloudReader
    {
        // callback for when new points gets added
        public delegate void OnNewPointAdded(Point point);
        public static OnNewPointAdded OnNewPointCallbacks;

        // callback when asset has been fully loaded
        public static Action OnAssetLoadedCallbacks;

        // store asset name to load from
        private static string _assetName;

        /// <summary>
        /// Starts loading from the specified asset.
        /// </summary>        
        /// <param name="assetName">The path to the asset to load from.</param>
        public static void ReadFromAsset(string assetName)
        {
            _assetName = assetName;

            Task task = new Task(StreamFromAsset);
            task.Start();
        }

        private static void StreamFromAsset()
        {
            Stream storage = IO.StreamFromFile("Assets/" + _assetName, FileMode.Open);
            using (var sr = new StreamReader(storage))
            {
                string line;
                while ((line = sr.ReadLine()) != null) // read per line
                {
                    string delimiter = "\t";
                    string[] textElements = line.Split(delimiter.ToCharArray());

                    if (textElements.Length == 1) // empty line
                        continue;

                    Point point = new Point();

                    // convert each string to float

                    float[] numbers = new float[textElements.Length];
                    for (var i = 0; i < numbers.Length; i++)
                    {
                        numbers[i] = float.Parse(textElements[i], CultureInfo.InvariantCulture.NumberFormat);
                    }

                    point.Position = new float3(numbers[0], numbers[2], numbers[1]);

                    if (numbers.Length == 9)
                    {
                        point.Color = new float3(numbers[3], numbers[4], numbers[5]);

                        point.EchoId = numbers[6];
                        point.ScanNr = numbers[8];
                    }

                    OnNewPointCallbacks?.Invoke(point);
                }

                OnAssetLoadedCallbacks?.Invoke();
            }
        }
    }
}
