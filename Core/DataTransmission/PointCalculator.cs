using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Base.Core;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
//using Debug = System.Diagnostics.Debug;

namespace Fusee.Tutorial.Core.DataTransmission
{
    /// <summary>
    /// This class calculates points from the received data
    /// </summary>
    /// 
    public class PointCalculator
    {
        
        private float3x3 rotmat;
        public List<float> _dist = new List<float>();
        private float3[] _points;
        private float3 _dronePoint;

        public float3[] _Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public void GetValues(float[] values)
        {
            _points = new float3[500];
            float Posx = values[0];
            float PosY = values[1];
            float PosZ = values[2];
            float qx = values[3];
            float qy = values[4];
            float qz = values[5];
            float qw = values[6];
           
            _dronePoint = new float3 (Posx, PosY, PosZ);
           // Diagnostics.Log("PointCalc Values: " + _dronePoint);
            RotationMatrix(qx, qy, qz, qw);
        }

        public List<float> GetDistance(float distance)
        {
            _dist.Add(distance);
            return _dist;          
        }

        public void RotationMatrix(float qx, float qy, float qz, float qw)
        {

            float el1 = (qx * qx) + (qy * qy) - (qz * qz) - (qw * qw);
            float el2 = 2 * (qy * qz) - (qx * qw);
            float el3 = 2 * (qy * qw) - (qx * qz);

            rotmat = new float3x3(new float3(el1, el2, el3), new float3(el2, el1, el3), new float3(el3, el2, el1));
            //Diagnostics.Log("RotationMatrix: " + rotmat);
        }
              

        public float3[] CalculateNewPoint(List<float> _dist, float phi)
        {
           int k = 0;
           float j = (90/ 500); 
            for (int i = 0; i < _points.Length; i++)
            {
                            
                float3[] distPoint = new float3[_points.Length];
                distPoint[i] = new float3((_dist[i] * (float)(Sin(j)* Cos(phi))),(_dist[i] * (float)(Sin(j)* Sin(phi))),(_dist[i] * (float)(Cos(phi))));
                float3 _offset = _dronePoint;
                _points[k] = (rotmat * distPoint[k])+_offset;
                k++;
                j= j + 0.18f;
            }
            //Diagnostics.Log("_points: " + _points[41]);
            return _points;
        }
        //TODO --> check if calculation is working --> fix possible Problems and Render Points
    }
}
