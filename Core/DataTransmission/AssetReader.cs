using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Math.Core;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Fusee.Tutorial.Core.Data_Transmission
{
    /// <summary>
    /// Class which asynchronously loads assets, especially text files containing point clouds.
    /// </summary>

    public class AssetReader
    {
        // callback for when new points gets added
        public delegate void OnNewPointAdded(Common.Point point);
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

            //*
            Task task = new Task(StreamFromAsset);
            task.Start();
            //*/

            //StreamFromAsset();
        }

        /// <summary>
        /// Reads the asset.
        /// </summary>
        private static void StreamFromAsset()
        {
            Stream storage = IO.StreamFromFile("Assets/" + _assetName, FileMode.Open);
            using (var sr = new StreamReader(storage))
            {
                string line;
                while ((line = sr.ReadLine()) != null) // read per line
                {
                    Common.Point point = ConvertTextToPoint(line);

                    if (point != null)
                        OnNewPointCallbacks?.Invoke(point);
                }

                OnAssetLoadedCallbacks?.Invoke();
            }
        }

        /// <summary>
        /// Called from StreamFromAsset().
        /// </summary>
        /// <param name="line">Text line.</param>
        /// <returns>Point object</returns>
        private static Common.Point ConvertTextToPoint(string line)
        {
            string delimiter = "\t";
            string[] textElements = line.Split(delimiter.ToCharArray());

            if (textElements.Length == 1) // empty line
                return null;

            Common.Point point = new Common.Point();

            // convert each string to float

            float[] numbers = new float[textElements.Length];
            for (var i = 0; i < numbers.Length; i++)
            {
                numbers[i] = float.Parse(textElements[i], CultureInfo.InvariantCulture.NumberFormat);

                // prevent fusee from the same thing that happend to ariane 5
                // because e.g. 16 - 0.0000001f = 16, but Math.Floor(0.1E-06 / 16) = -1 and that isn't what we want
                if (numbers[i] < 0.000001f && numbers[i] > -0.000001f)
                {
                    numbers[i] = 0;
                }
            }

            point.Position = new float3(numbers[0], numbers[2], numbers[1]);

            if (numbers.Length == 9)
            {
                //point.Intensity = new float3(numbers[3], numbers[4], numbers[5]);

                //point.EchoId = numbers[6];
                point.ScanNr = numbers[8];
            }

            return point;
        }
    }
}
