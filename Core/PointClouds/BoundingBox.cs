using Fusee.Math.Core;

/// <summary>
/// Represents a bounding box of some point cloud or whatever.
/// </summary>

namespace Fusee.Tutorial.Core.PointClouds
{
    public class BoundingBox
    {
        public delegate void OnBoundingBoxUpdate(BoundingBox boundingBox);
        public OnBoundingBoxUpdate UpdateCallbacks;

        private float3 _minValues = new float3(0, 0, 0);
        private float3 _maxValues = new float3(0, 0, 0);

        private float3 _center = new float3(0, 0, 0);

        private bool firstPoint = true;

        public void Update(float3 vertex)
        {
            float3 oldMinValues = _minValues;
            float3 oldMaxValues = _maxValues;
            
            if (firstPoint)
            {
                firstPoint = false;
                _minValues = vertex;
                _maxValues = vertex;
            }

            if (vertex.x < _minValues.x)
                _minValues.x = vertex.x;

            if (vertex.y < _minValues.y)
                _minValues.y = vertex.y;

            if (vertex.z < _minValues.z)
                _minValues.z = vertex.z;

            if (vertex.x > _maxValues.x)
                _maxValues.x = vertex.x;

            if (vertex.y > _maxValues.y)
                _maxValues.y = vertex.y;

            if (vertex.z > _maxValues.z)
                _maxValues.z = vertex.z;
            
            // if something changed
            if(oldMinValues != _minValues || oldMaxValues != _maxValues)
            {
                ComputeCenter();
                UpdateCallbacks?.Invoke(this);
            }
        }

        public float3 GetMinValues()
        {
            return _minValues;
        }

        public float3 GetMaxValues()
        {
            return _maxValues;
        }

        public float3 GetCenterPoint()
        {
            return _center;
        }

        private void ComputeCenter()
        {
            float centerX = _minValues.x + ( _maxValues.x - _minValues.x ) / 2;
            float centerY = _minValues.y + ( _maxValues.y - _minValues.y ) / 2;
            float centerZ = _minValues.z + ( _maxValues.z - _minValues.z ) / 2;

            _center = new float3(centerX, centerY, centerZ);
        }
    }
}
