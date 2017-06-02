using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Debug = System.Diagnostics.Debug;



namespace Fusee.Tutorial.Core.DataTransmission
{
    public class PointCalculator
    {

        private float3x3 rotmat;
        //private float[] _dist;
        List<float> _dist = new List<float>();
        private float3[] _points;

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

            float3 dronePoint = new float3 (Posx, PosY, PosZ);            
            RotationMatrix(qx, qy, qz, qw);
          

            //Debug
            //Diagnostics.Log("Pos X: " + drone_posX);
        }

        public void GetDistance(float distance)
        {
            _dist.Add(distance);
            CalculateNewPoint(_dist, 90f);
        }

        public void RotationMatrix(float qx, float qy, float qz, float qw)
        {

            float el1 = (qx * qx) + (qy * qy) - (qz * qz) - (qw * qw);
            float el2 = 2 * (qy * qz) - (qx * qw);
            float el3 = 2 * (qy * qw) - (qx * qz);

            rotmat = new float3x3(new float3(el1, el2, el3), new float3(el2, el1, el3), new float3(el3, el2, el1));
           
        }
              

        public float3[] CalculateNewPoint(List<float> _dist, float phi)
        {
           int k = 0;
           float j = (90/ 500); 
            for (int i = 0; i < _points.Length; i++)
            {
               // float dist = _dist;                
                float3[] distPoint = new float3[_points.Length];
                distPoint[i] = new float3((_dist[i] * (float)(Sin(j)* Cos(phi))),(_dist[i] * (float)(Sin(j)* Sin(phi))),(_dist[i] * (float)(Cos(phi))));
                _points[k] = rotmat * distPoint[k];
                k++;
                j= j + 0.18f;
               // return _points;
            }
            return _points;
        }
        //TODO --> Render Points 
    }
}
