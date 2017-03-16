using Fusee.Math.Core;
using System.Collections.Generic;

/// <summary>
/// Represents the voxel space (acts as data structure).
/// </summary>

namespace Fusee.Tutorial.Core
{
    public enum VoxelState
    {
        Unknown, Occupied, Free
    }

    public class VoxelSpace
    {
        private List<Voxel> _voxels;
        private float _voxelSize;
        
        public VoxelSpace()
        {
            _voxels = new List<Voxel>();
            SetVoxelSize(2);

            PointCloudReader.OnNewPointCallbacks += OnNewPointAdded;
        }

        public void SetVoxelSize(float voxelSize)
        {
            _voxelSize = voxelSize;
        }

        public float GetVoxelSize()
        {
            return _voxelSize;
        }

        public List<Voxel> GetVoxels()
        {
            return _voxels;
        }

        private Voxel CreateVoxel(Point point, VoxelState voxelState)
        {
            // what is the next voxel position in the overall grid?
            // => multiple of voxel size

            int divisionX = (int) System.Math.Round( point.Position.x / _voxelSize );
            int divisionY = (int) System.Math.Round( point.Position.y / _voxelSize );
            int divisionZ = (int) System.Math.Round( point.Position.z / _voxelSize );
            
            float3 newPos = new float3(divisionX, divisionY, divisionZ) * _voxelSize;

            Voxel voxel = new Voxel();
            voxel.Position = newPos;
            voxel.VoxelState = voxelState;

            return voxel;
        }

        private void OnNewPointAdded(Point point)
        {
            if(_voxels.Count == 0)
            {
                Voxel voxel = CreateVoxel(point, VoxelState.Occupied);
                _voxels.Add(voxel);
            }
            else
            {
                // check for each existing voxel, if this point lies within
                bool outlier = true;
                float3 posP = point.Position;

                foreach (Voxel voxel in _voxels)
                {
                    float3 posV = voxel.Position;

                    float diffX = System.Math.Abs(posP.x - posV.x);
                    float diffY = System.Math.Abs(posP.y - posV.y);
                    float diffZ = System.Math.Abs(posP.z - posV.z);

                    float halfVoxelSize = _voxelSize / 2;

                    if ( diffX < halfVoxelSize && diffY < halfVoxelSize && diffZ < halfVoxelSize ) // point lies inside a voxel
                    {
                        outlier = false;
                        break;
                    }
                }

                if(outlier)
                {
                    Voxel newVoxel = CreateVoxel(point, VoxelState.Occupied);
                    _voxels.Add(newVoxel);
                }
            }

        }
    }
}
