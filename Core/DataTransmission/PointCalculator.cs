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
        
        private List <float3x3> rotmat = new List<float3x3>();
        public List<float> _dist = new List<float>();
        private float3[] _points;
        private List<float3> _dronePos = new List<float3>();
 
         private int pos = 0;

        public float3[] _Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public void GetValues(float PosX,float PosY, float PosZ, float qx, float qy, float qz, float qw )
        {
            _points = new float3[500];

           _dronePos.Add(new float3(PosX, PosY, PosZ)); 
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

            rotmat.Add(new float3x3(new float3(el1, el2, el3), new float3(el2, el1, el3), new float3(el3, el2, el1)));
          
        }
              

        public float3[] CalculateNewPoint(List<float> _dist, float phi)
        {
           int k = 0;
           float j = (90/ 500); 
            for (int i = 0; i < _points.Length; i++)
            {
                            
                float3[] distPoint = new float3[_points.Length];
                distPoint[i] = new float3((_dist[i] * (float)(Sin(j)* Cos(phi))),(_dist[i] * (float)(Sin(j)* Sin(phi))),(_dist[i] * (float)(Cos(phi))));
                float3[] _offset = new float3[_dronePos.Count];

                _offset[pos]=_dronePos[pos];
                _points[k] = (rotmat[pos] * distPoint[k])+_offset[pos];
                //Diagnostics.Log("_dronepos " + _dronePos[pos]);
                k++;
                j= j + 0.18f;
               
                //TODO check spherical coordinates --> computation seems to be wrong/ chose wrong directions for X,Y,Z ??
            }
            pos++;
            //Diagnostics.Log("_points: " + _points[41]);
            //PointCloudReader.ConvertCalculatedPointsToPoints(_points);
            return _points;
        }
    }
}
