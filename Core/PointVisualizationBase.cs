﻿using System.Collections.Generic;
using Fusee.Base.Core;
using Fusee.Base.Common;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using System.ComponentModel;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private PointCloud _pointCloud;

        // camera controls
        private float _rotationY = 0.0f;
        private float _rotationX = 0.0f;
        private float3 _cameraPosition = new float3(0, 0, -20.0f);
        private float3 _cameraPivot = new float3(0, 0, 0);
        
        // shader parameters
        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;
        
        private float4x4 _xform;
        private float _particleSize = 0.015f;

        // Init is called on startup. 
        public override void Init()
        {
            //_pointCloud = new PointCloud();
            _pointCloud = AssetStorage.Get<PointCloud>("BasicPoints.txt");
            PointCloudReader.ReadFromAsset("PointCloud_IPM.txt", _pointCloud.Merge);
            
            // read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            
            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(_particleSize, _particleSize));

            _xFormParam = RC.GetShaderParam(shader, "xForm");
            _xform = float4x4.Identity;
            RC.SetShaderParam(_xFormParam, _xform);

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            MoveInScene();
            
            List<Mesh> meshes = _pointCloud.GetMeshes();
            for(var i=0; i<meshes.Count; i++)
            {
                RC.Render(meshes[i]);
            }
            
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void MoveInScene()
        {
            // set origin to camera pivot
            _xform = float4x4.CreateTranslation(-1 * _cameraPivot);

            // rotate around camera pivot
            if (Mouse.LeftButton || Touch.ActiveTouchpoints == 1)
            {
                float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);

                _rotationY -= speed.x * 0.0001f;
                _rotationX -= speed.y * 0.0001f;
            }

            var rotation = float4x4.CreateRotationX(_rotationX) * float4x4.CreateRotationY(_rotationY);
            _xform = rotation * _xform;

            // set camera to its position
            _xform = float4x4.CreateTranslation(-1 * _cameraPosition) * _xform;

            // --- move camera pivot
            
            float3 translation = float3.Zero;

            if (Mouse.RightButton || Touch.TwoPoint)
            {
                float2 speed = Mouse.Velocity + Touch.TwoPointMidPointVel;
                translation.x = speed.x * -0.005f;
                translation.y = speed.y * 0.005f;
            }

            if (Mouse.Wheel != 0 || Touch.TwoPoint)
            {
                float speed = Mouse.WheelVel + Touch.TwoPointDistanceVel * 0.1f;
                translation.z = speed * 0.1f;
            }

            if (translation.Length > 0)
            {
                rotation.Invert();
                translation = rotation * translation;
                _cameraPivot += translation;
                //_xform = float4x4.CreateTranslation(translation) * _xform;
            }

            // --- projection matrix

            _xform = RC.Projection * _xform;
            RC.SetShaderParam(_xFormParam, _xform);
        }

        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            float aspectRatio = Width / (float) Height;

            RC.SetShaderParam(_particleSizeParam, new float2(_particleSize, _particleSize * aspectRatio)); //set params that can be controlled with arrow keys
            
            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
        }

    }
}