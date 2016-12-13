using System;
using System.Globalization;
using System.IO;
using Fusee.Base.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core;

namespace Fusee.Tutorial.Core
{
    /// <summary>
    /// This class loads an asset and creates a point cloud out of it. May be used in the future to load
    /// points over a network stream.
    /// </summary>
    
    public class PointCloudReader
    {
        public static PointCloud ReadFromAsset(string assetName)
        {
            return AssetStorage.Get<PointCloud>(assetName);
        }
    }
}
