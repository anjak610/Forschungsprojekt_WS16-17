using Fusee.Math.Core;
using System;
using System.Collections;
using System.Linq;

namespace Fusee.Tutorial.Core.Data_Transmission
{
    public class PointCloudData
    {
        public static uint BEGIN_MARKER = 0xFEEDBEEF;
        public static uint FOOTER = 0xDEADBEEF;
        public static ushort TYPE = 0x1010;

        public bool isValid = false;

        public uint beginMarker;
        public uint footer;
        public ushort type;

        public ushort version = 0x0001;
        public uint packetSize;
        public double utcTime;

        public float3 position;
        public float4 quaternion;

        public uint numberOfPoints;
        public uint[] points;

        public PointCloudData(byte[] data)
        {
            beginMarker = BitConverter.ToUInt32(data, 0);
            type = BitConverter.ToUInt16(data, 4);

            numberOfPoints = BitConverter.ToUInt32(data, 48);
            footer = BitConverter.ToUInt32(data, (int) (52 + numberOfPoints * 4));

            // check that all constants are good

            if(beginMarker == BEGIN_MARKER && type == TYPE && footer == FOOTER)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
                return;
            }

            version = BitConverter.ToUInt16(data, 6);
            packetSize = BitConverter.ToUInt32(data, 8);
            utcTime = BitConverter.ToDouble(data, 12);

            position = new float3();

            position.x = BitConverter.ToSingle(data, 20);
            position.y = BitConverter.ToSingle(data, 24);
            position.z = BitConverter.ToSingle(data, 28);

            quaternion = new float4();

            quaternion.w = BitConverter.ToSingle(data, 32);
            quaternion.x = BitConverter.ToSingle(data, 36);
            quaternion.y = BitConverter.ToSingle(data, 40);
            quaternion.z = BitConverter.ToSingle(data, 44);

            points = new uint[numberOfPoints];

            var j = 0;
            for(var i=52; i<52+numberOfPoints*4; i+=4)
            {
                points[j] = BitConverter.ToUInt32(data, i);
                j++;
            }
        }
    }
}
