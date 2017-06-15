using Fusee.Math.Core;
using Fusee.Tutorial.Core.Common;
using Fusee.Tutorial.Core.Data_Transmission;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Fusee.Tutorial.Core.Data_Transmission.AssetReader;

namespace Fusee.Tutorial.Desktop.HelperClasses
{
    /// <summary>
    /// Receives input and hands it over.
    /// </summary>

    public class UDPReceiver : IUDPReceiver
    {
        public OnNewPointAdded OnNewPointCallbacks { get; set; }
        public Action<float3> OnDronePositionCallbacks { get; set; }

        private UdpClient client;
        private int _port;

        public void Listen()
        {
            if (client == null)
                return;

            Task task = new Task(StreamFromSub);
            task.Start();
        }

        private void StreamFromSub()
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // receive bytes => one package at a time
                    byte[] data = client.Receive(ref anyIP);

                    // convert byte array into usable numbers
                    PointCloudData package = new PointCloudData(data);

                    if (package.isValid) // continue processing package
                    {
                        Point point2Pass = new Point();

                        // angles in radians
                        double angle = -45 * System.Math.PI / 180.0;
                        double angle_inc = ((90.0) / package.points.Length) * System.Math.PI / 180.0;

                        Quaternion uav_rot = new Quaternion(package.quaternion.x, package.quaternion.y, package.quaternion.z, package.quaternion.w);

                        float4x4 uav_rot_mat = Quaternion.QuaternionToMatrix(uav_rot);
                        uav_rot_mat.Invert();

                        float4x4 global_rot_mat; // = float4x4.Identity;
                        float4x4.CreateFromAxisAngle(float3.UnitY, (float) System.Math.PI, out global_rot_mat);

                        float3 dronePos = global_rot_mat * package.position;
                        OnDronePositionCallbacks?.Invoke(new float3(dronePos.x, dronePos.z, dronePos.y));

                        // convert point data to position, color and echo id
                        for (var i=0; i<package.points.Length; i++)
                        {
                            uint point = package.points[i];

                            double distance = point & 0x00FFFFFF;
                            distance *= 0.0001; // --> to m

                            if (distance < 0.001)
                                continue;

                            float3 ray = new float3(0, (float)(distance * System.Math.Sin(angle)), (float)(distance * System.Math.Cos(angle)));

                            float intensity = (point & 0x3F000000) >> 24;
                            intensity = intensity / 63;

                            intensity *= 0.7f;
                            intensity += 0.3f;

                            byte echoId = (byte)(point >> 30);
                            /*
                            Random rnd = new Random();
                            byte echoId = (byte) rnd.Next(1, 5);
                            */
                            float3 posPoint = global_rot_mat * (uav_rot_mat * ray + package.position);
                            point2Pass.Position = new float3(posPoint.x, posPoint.z, posPoint.y);
                            point2Pass.Intensity = intensity;
                            
                            point2Pass.EchoId = echoId;

                            OnNewPointCallbacks?.Invoke(point2Pass);

                            angle += angle_inc;
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        public void SetPort(int port)
        {
            if(client != null)
                client.Close();

            client = new UdpClient(port);
        }
    }
}