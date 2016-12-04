﻿using System.Collections.Generic;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;
using Fusee.Tutorial.Desktop;
using Fusee.Serialization;
using System.Diagnostics;
using System.Linq;
using System;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Forschungsprojekt", Description = "HFU Wintersemester 16-17")]
    public class PointVisualizationBase : RenderCanvas
    {
        private Mesh _mesh;

        //Sceneviewer Parameters    
        private float _maxPinchSpeed;
        private float _minPinchSpeed;
        private bool _scaleKey;
        private bool _twoTouchRepeated;
        public static float _zoomVel, _zoom;
        public bool _mouseWheel;  

        private static float2 _offset;
        private static float2 _offsetInit;

        //End ScneneViewer      

        private IShaderParam _particleSizeParam;
        private IShaderParam _xFormParam;
        private IShaderParam _screenSizeParam;

        private float4x4 projection;

        private float4x4 _xform;
        private float2 _screenSize;
                    
        private List<float3> normals = new List<float3>();
        List<float3> vertices = new List<float3>();
        List<ushort> triangles = new List<ushort>();

        private float _alpha;
        private float _beta;

        public static PointCloud cloud;
        public static PointReader preader;


        private const float ParticleSize = 0.05f;
        
        // Init is called on startup. 
        public override void Init()
        {
            // read point cloud from file and assign vertices to cloud object
            cloud = new PointCloud();
            preader = new PointReader(cloud);
            preader.readPointList();

            //For SceneViewer
            _twoTouchRepeated = false;      
            _twoTouchRepeated = false;
            _zoom = -10;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            //read shaders from files
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");

            // Initialize the shader(s)
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);

            _particleSizeParam = RC.GetShaderParam(shader, "particleSize");
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize));

            _xFormParam = RC.GetShaderParam(shader, "xForm");
            _xform = float4x4.Identity;

            _screenSizeParam = RC.GetShaderParam(shader, "screenSize");
            _screenSize = new float2(Width, Height); //set screen/window dimensions according to the current viewport
            //RC.SetShaderParam(_xFormParam, float4x4.CreateScale(0.5f) * float4x4.CreateTranslation(-2, -33, 34));

            _mesh = new Mesh();

            float3 pickedvertex;

            for (var i = 0; i < cloud.Vertices.Count; i++)
            {

                //vertex list times 4
                pickedvertex = cloud.Vertices[i];
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);
                vertices.Add(pickedvertex);

                normals.Add(new float3(-1, -1, 0));
                normals.Add(new float3(1, -1, 0));
                normals.Add(new float3(-1, 1, 0));
                normals.Add(new float3(1, 1, 0));

                triangles.Add((ushort)(0 + i * 4));
                triangles.Add((ushort)(1 + i * 4));
                triangles.Add((ushort)(3 + i * 4));
                triangles.Add((ushort)(0 + i * 4));
                triangles.Add((ushort)(3 + i * 4));
                triangles.Add((ushort)(2 + i * 4));
                
            }

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(0.95f, 0.95f, 0.95f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            _mesh.Vertices = vertices.ToArray();
            _mesh.Normals = normals.ToArray();
            _mesh.Triangles = triangles.ToArray();

            MoveInScene();


            RC.SetShaderParam(_xFormParam, _xform);
            RC.Render(_mesh);

            var mtxCam = float4x4.LookAt(0, 0, -_zoom, 0, 0, -50, 0, 1, 0);
            var mtxOffset = float4x4.CreateTranslation(2 * _offset.x / Width, -2 * _offset.y / Height, 0);

            RC.Projection = projection  *mtxOffset * mtxCam;

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void MoveInScene()
        {

            //rotate around Object
            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x * 0.0001f;
                _beta -= speed.y * 0.0001f;
            }

            var view = float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta);
            _xform = RC.Projection * float4x4.CreateTranslation(0, 0, 5.0f) * view;


            //Scale Points with W and A
            if (Keyboard.ADAxis != 0 || Keyboard.WSAxis != 0)
            {
                _scaleKey = true;
            }
            else
            {
                _scaleKey = false;
            }

            if (_scaleKey)
            {
                for (int i = 0; i < normals.Count; i++)
                {
                    normals[i] = normals[i] + Keyboard.ADAxis * (normals[i] / 200);
                }
            }

            //Move Camer on x- and y-axis through scene with arrowkeys


            // Scale Points with Touch and move camera on x- and y-axis through scene
            if (Touch.TwoPoint) // TODO: Implement scaling with slide movements on screen 
            {
                if (!_twoTouchRepeated)
                {
                    _twoTouchRepeated = true;
                    _maxPinchSpeed = 0;
                    _minPinchSpeed = 0;
                    _offsetInit = Touch.TwoPointMidPoint - _offset;
                }

                _offset = Touch.TwoPointMidPoint - _offsetInit;
                float pinchSpeed = Touch.TwoPointDistanceVel;
                if (pinchSpeed > _maxPinchSpeed)
                {
                    _maxPinchSpeed = pinchSpeed;
                    for (int i = 0; i < normals.Count; i++)
                    {
                        normals[i] = normals[i] + (normals[i] / 10);
                    }
                }
                else if (pinchSpeed < _minPinchSpeed)
                {
                    _minPinchSpeed = pinchSpeed;
                    for (int i = 0; i < normals.Count; i++)
                    {
                        normals[i] = normals[i] - (normals[i] / 10);
                    }
                }
            }
            else
            {
                _twoTouchRepeated = false;
            }           

            //Zoom with Mousewheel
            if (Mouse.Wheel != 0)
            {
                _mouseWheel = true;

            }

            if (_mouseWheel)
            {
                _zoomVel = Mouse.WheelVel * -0.005f;
            }

            _zoom += _zoomVel;
            // Limit zoom
            if (_zoom < -50)
                _zoom = -50;
            if (_zoom > 200)
                _zoom = 200;

        }
        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);
            //var mtxCam = float4x4.LookAt(0, 20, -_zoom, 0, 0, 0, 0, 1, 0);
            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;
            RC.SetShaderParam(_particleSizeParam, new float2(ParticleSize, ParticleSize * aspectRatio)); //set params that can be controlled with arrow keys

            // Question: should we set particleSize depending on aspect ratio or rather define an amount of pixels, thus taking window size into computation?

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
           // RC.Projection =projection *mtxCam;
        }

    }
}