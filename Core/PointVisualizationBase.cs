﻿using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using Fusee.Serialization;
using Fusee.Xene;
using System.Linq;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private const float VoxelSideLength = 2;

        //private PointCloud _pointCloud;
        private BoundingBox _boundingBox;

        private Mesh _cubeMesh;
        private List<float3> _voxelPositions;
        
        // shader params
        
        private IShaderParam _albedoParam;
        private float2 _yScale; // needed for color calculation
        private float _lastVoxelPosY = 0; // speed up calculation
        
        private float _rotationY = (float) System.Math.PI;
        private float _rotationX = (float) System.Math.PI / -8;

        //private float3 _cameraPosition = new float3(0, 0, -20.0f);
        //private float3 _cameraPivot = new float3(60, -25, 10);

        private float3 _cameraPosition = new float3(0, 0, -5.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        private float3 _cameraPivotOffset = new float3(0, 0, 0);

        private float _minAngleX = (float) -System.Math.PI / 4;
        private float _maxAngleX = (float) System.Math.PI / 4;
                
        // Init is called on startup. 
        public override void Init()
        {
            // octree
            _voxelPositions = new List<float3>();

            Octree<OctreeNodeStates> octree = new Octree<OctreeNodeStates>(float3.Zero, VoxelSideLength);

            octree.OnNodeAddedCallback += (OctreeNode<OctreeNodeStates> node) =>
            {
                if(node.Data == OctreeNodeStates.Occupied && node.SideLength == VoxelSideLength)
                {
                    _voxelPositions.Add(node.Position);

                    // nach y position sortieren, damit color calculation schneller läuft => mehr rechenaufwand, lohnt nicht
                    //_voxelPositions = _voxelPositions.OrderBy(o => o.y).ToList();
                }
            };
            
            PointCloudReader.OnNewPointCallbacks += (Point point) =>
            {
                octree.Add(point.Position, OctreeNodeStates.Occupied);
            };

            // point cloud + bounding box

            //_pointCloud = new PointCloud();

            _boundingBox = new BoundingBox();
            _boundingBox.UpdateCallbacks += OnBoundingBoxUpdate;

            PointCloudReader.OnNewPointCallbacks += (Point point) =>
            {
                _boundingBox.Update(point.Position);
            };

            //PointCloudReader.ReadFromAsset("PointCloud_IPM.txt", _pointCloud.Merge);
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt");

            _cubeMesh = LoadMesh("Cube.fus");

            // read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            
            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _albedoParam = RC.GetShaderParam(shader, "albedo");
            RC.SetShaderParam(_albedoParam, float3.One);
            
            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        private Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            float4x4 cameraView = MoveInScene();
            
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
            
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        // update cameraPivot, whenever bounding box of point cloud gets updated
        private void OnBoundingBoxUpdate(BoundingBox boundingBox)
        {
            _cameraPivot = boundingBox.GetCenterPoint();

            _yScale.x = boundingBox.GetMinValues().y;
            _yScale.y = boundingBox.GetMaxValues().y;
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

    }
}