using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using Fusee.Tutorial.Core.Common;
using Fusee.Base.Common;
using System.Collections.Generic;
using Java.IO;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        #region Fields

        // --- constants

        private const float VoxelSideLength = 2;
        public float ParticleSize = 0.05f; // maybe gets changed from platform specific classes

        private const int ConsiderEvery = 4; // take only every xth point into account in order to speed up calculation
        private int _pointCounter = 0;
        
        // --- mesh data / pointcloud / voxelspace

        //private DynamicMesh _pointCloud;
        private DynamicMesh _voxelSpace;
        private BoundingBox _boundingBox;

        Octree<OctreeNodeStates> _octree;

        // --- shader params

        private IShaderParam _yBoundsParam;
        private float2 _yBounds;

        //private IShaderParam _particleSizeParam;

        // --- camera positioning

        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        private float3 _cameraPosition = new float3(0, 0, -5.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        private float3 _cameraPivotOffset = new float3(0, 0, 0);

        private float _minAngleX = (float) -System.Math.PI / 4;
        private float _maxAngleX = (float) System.Math.PI / 4;

        #endregion

        /// <summary>
        /// Initializes variables (very good summary).
        /// </summary>
        public override void Init()
        {
            // octree

            _octree = new Octree<OctreeNodeStates>(float3.Zero, VoxelSideLength);
            _octree.OnNodeAddedCallback += OnNewNodeAdded;

            _voxelSpace = new DynamicMesh();
            
            // point cloud + bounding box

            //_pointCloud = new DynamicMesh();

            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            // callbacks for various readings and stuff

            PointCloudReader.OnNewPointCallbacks += OnNewPointAdded;
            PointCloudReader.OnAssetLoadedCallbacks += OnAssetLoaded;

            // start loading points

            //PointCloudReader.ReadFromAsset("PointCloud_IPM.txt", _pointCloud.Merge);
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt");
            
            // read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            
            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _yBoundsParam = RC.GetShaderParam(shader, "yBounds");
            RC.SetShaderParam(_yBoundsParam, float2.Zero);
            /*
            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));
            //*/

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        #region Rendering

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            float4x4 cameraView = MoveInScene();

            /*
            for(var i=0; i<_voxelPositions.Count; i++)
            {
                float3 voxelPos = _voxelPositions[i];

                // color calculation

                if(voxelPos.y != _lastVoxelPosY)
                {
                    _lastVoxelPosY = voxelPos.y;

                    float hue = (voxelPos.y - _yScale.x) / (_yScale.y - _yScale.x);

                    float r, g, b;
                    Color.HSVtoRGB(out r, out g, out b, hue * 360, 1, 1);
                    float3 albedo = new float3(r, g, b);

                    RC.SetShaderParam(_albedoParam, albedo);
                }
                
                // model view

                float4x4 modelView = cameraView * float4x4.CreateTranslation(voxelPos);
                RC.ModelView = modelView * float4x4.CreateScale( VoxelSideLength / 2 );
                
                RC.Render(_cubeMesh);
            }
            */
            
            RC.ModelView = cameraView;

            RC.SetShaderParam(_yBoundsParam, _yBounds);

            //List<Mesh> meshes = _pointCloud.GetMeshes();
            ///*
            List<Mesh> meshes = _voxelSpace.GetMeshes();
            for (var i=0; i<meshes.Count; i++) // foreach would not work here because of multithreading
            {
                RC.Render(meshes[i]);
            }
            //*/

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }
        
        private float4x4 MoveInScene()
        {
            // set origin to camera pivot
            float4x4 xform = float4x4.CreateTranslation(-1 * ( _cameraPivot + _cameraPivotOffset ));
            
            // rotate around camera pivot
            if (Mouse.LeftButton || Touch.ActiveTouchpoints == 1)
            {
                float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);

                _rotationY -= speed.x * 0.0001f;
                _rotationX -= speed.y * 0.0001f;

                // clamp rotation x between min and max angle
                _rotationX = _rotationX < _minAngleX ? _minAngleX : (_rotationX > _maxAngleX ? _maxAngleX : _rotationX);
            }

            var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
            xform = rotation * xform;

            // --- move camera position
            
            if (Mouse.Wheel != 0 || Touch.TwoPoint)
            {
                float speed = Mouse.WheelVel + Touch.TwoPointDistanceVel * 0.1f;
                _cameraPosition.z += speed * 0.1f;
            }

            xform = float4x4.CreateTranslation(-1 * _cameraPosition) * xform;
            
            // --- move camera pivot
            /*
            float3 translation = float3.Zero;

            if (Mouse.RightButton || Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
                translation.x = speed.x * -0.005f;
                translation.y = speed.y * 0.005f;
            }
            
            if (translation.Length > 0)
            {
                rotation.Invert();
                translation = rotation * translation;
                _cameraPivotOffset += translation;
                //_xform = float4x4.CreateTranslation(translation) * _xform;
            }
            */
            // --- projection matrix

            return xform;
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            float aspectRatio = Width / (float) Height;
            
            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
        }

        #endregion

        #region Various Event Handler

        /// <summary>
        /// Event handler for when a new node gets added to the octree.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNewNodeAdded(OctreeNode<OctreeNodeStates> node)
        {
            if (node.Data == OctreeNodeStates.Occupied && node.SideLength == VoxelSideLength)
            {
                // create single cube with VoxelSideLength at specified position

                Cube cube = new Cube();

                for (var i = 0; i < cube.Vertices.Length; i++)
                {
                    float3 vertex = cube.Vertices[i];

                    // scale cube (vertices) according to voxel side length
                    vertex *= VoxelSideLength; // no dividing by 2 neccessary, because vertex is already 0.5

                    // position vertex according to voxel center position
                    vertex += node.Position;

                    // apply to mesh
                    cube.Vertices[i] = vertex;
                }

                // --- add cube mesh to final mesh
                _voxelSpace.AddMesh(cube);
                _voxelSpace.Apply();                    
            }
        }

        /// <summary>
        /// Whenever a new point gets loaded, this is what happens with him.
        /// </summary>
        /// <param name="point">Point data structure containing position and stuff.</param>
        private void OnNewPointAdded(Point point)
        {
            _pointCounter++;

            if (_pointCounter % ConsiderEvery != 0)
                return;

            //octree.Add(point.Position, OctreeNodeStates.Occupied);
            _boundingBox.Update(point.Position);

            // add point to point cloud
            /*
            PointMesh pointMesh = new PointMesh(point.Position);

            if (point.Color != null)
            {
                uint color = (uint)new ColorUint((float3) point.Color, 1);
                pointMesh.Colors = new uint[] { color, color, color, color };
            }

            _pointCloud.AddMesh(pointMesh);
            //*/ 

            // add point to octree
            _octree.Add(point.Position, OctreeNodeStates.Occupied);
        }

        /// <summary>
        /// Event handler for when asset has finished loading.
        /// </summary>
        private void OnAssetLoaded()
        {
            // apply remaining vertices etc.
            //_pointCloud.Apply();
            _voxelSpace.Bake();
        }
        
        // update cameraPivot, whenever bounding box of point cloud gets updated
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _cameraPivot = boundingBox.GetCenterPoint();
            
            _yBounds.x = boundingBox.GetMinValues().y;
            _yBounds.y = boundingBox.GetMaxValues().y;
        }

        #endregion
    }
}