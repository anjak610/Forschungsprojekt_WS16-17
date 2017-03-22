using Fusee.Math.Core;

internal class Bounds
{
    public float3 center;
    public float3 boundSize;

    public Bounds(float3 center, float3 boundSize)
    {
        this.center = center;
        this.boundSize = boundSize;
    }

    public bool Contains(float3 target)
    {
        bool outlier = true;

        float diffX = System.Math.Abs(target.x - center.x);
        float diffY = System.Math.Abs(target.y - center.y);
        float diffZ = System.Math.Abs(target.z - center.z);
        
        if (diffX < boundSize.x / 2 && diffY < boundSize.y / 2 && diffZ < boundSize.z / 2) // point lies inside this voxel
        {
            outlier = false;
        }

        return !outlier;
    }
}