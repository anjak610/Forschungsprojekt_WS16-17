using Fusee.Math.Core;
using System;
using static Fusee.Tutorial.Core.Data_Transmission.AssetReader;

namespace Fusee.Tutorial.Core.Data_Transmission
{
    /// <summary>
    /// Streams point clouds and drone positions via UDP.
    /// </summary>
    public interface IUDPReceiver
    {
        OnNewPointAdded OnNewPointCallbacks { get; set; }
        Action<float3> OnDronePositionCallbacks { get; set; }

        void StreamFrom(int port);
    }
}
