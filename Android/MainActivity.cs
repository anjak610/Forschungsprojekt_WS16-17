using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Runtime;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Android;
using Fusee.Engine.Imp.Graphics.Android;
using Fusee.Serialization;
using Fusee.Tutorial.Core;
using static Fusee.Engine.Core.Time; // frame rate
using Font = Fusee.Base.Core.Font;
using Path = Fusee.Base.Common.Path;
using Fusee.Math.Core;
using System;
using System.Globalization;

namespace Fusee.Tutorial.Android
{

	[Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon",
#if __ANDROID_11__
		HardwareAccelerated=false,
#endif
		ConfigurationChanges = ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
	public class MainActivity : Activity
	{
	    private FrameRateLogger _fRL;
        private Button plusButton;
        private Button minusButton;
        //private Core.PointVisualizationBase app;

        protected override void OnCreate (Bundle savedInstanceState)
		{

			base.OnCreate (savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.main_activity_layout);
            plusButton = FindViewById<Button>(Resource.Id.plus_btn);
            minusButton = FindViewById<Button>(Resource.Id.minus_btn);
            //onclick: increase or decrease size of particles    
            /*
            plusButton.Click += (sender, e) =>
            {
                app.ParticleSize = app.ParticleSize + app.ParticleSize / 2;
            };

            minusButton.Click += (sender, e) =>
            {
                app.ParticleSize = app.ParticleSize - app.ParticleSize / 2;
            };
            */
           
		    if (SupportedOpenGLVersion() >= 3)
		    {
                _fRL = new FrameRateLogger(); // start logging frame rate on console

                // SetContentView(new LibPaintingView(ApplicationContext, null));

                // Inject Fusee.Engine.Base InjectMe dependencies
                IO.IOImp = new IOImp(ApplicationContext);

                var fap = new Fusee.Base.Imp.Android.ApkAssetProvider(ApplicationContext);
                fap.RegisterTypeHandler(
                    new AssetHandler
                    {
                        ReturnedType = typeof(Font),
                        Decoder = delegate (string id, object storage)
                        {
                            if (Path.GetExtension(id).ToLower().Contains("ttf"))
                                return new Font
                                {
                                    _fontImp = new FontImp((Stream)storage)
                                };
                            return null;
                        },
                        Checker = delegate (string id) {
                            return Path.GetExtension(id).ToLower().Contains("ttf");
                        }
                    });
                fap.RegisterTypeHandler(
                    new AssetHandler
                    {
                        ReturnedType = typeof(SceneContainer),
                        Decoder = delegate (string id, object storage)
                        {
                            if (Path.GetExtension(id).ToLower().Contains("fus"))
                            {
                                var ser = new Serializer();
                                return ser.Deserialize((Stream)storage, null, typeof(SceneContainer)) as SceneContainer;
                            }
                            return null;
                        },
                        Checker = delegate (string id)
                        {
                            return Path.GetExtension(id).ToLower().Contains("fus");
                        }
                    });
                fap.RegisterTypeHandler( // TO-DO: ending shouldn't be .txt
                    new AssetHandler
                    {
                        ReturnedType = typeof(PointCloud),
                        Decoder = delegate (string id, object storage)
                        {
                            if (!Path.GetExtension(id).ToLower().Contains("txt")) return null;

                            PointCloud pointCloud = new PointCloud();

                            using (var sr = new StreamReader((Stream)storage, System.Text.Encoding.Default, true))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null) // read per line
                                {
                                    string delimiter = "\t";
                                    string[] textElements = line.Split(delimiter.ToCharArray());

                                    if (textElements.Length == 1) // empty line
                                        continue;

                                    Point point = new Point();
                                    float[] numbers = Array.ConvertAll(textElements, n => float.Parse(n, CultureInfo.InvariantCulture.NumberFormat));

                                    point.Position = new float3(numbers[0], numbers[1], numbers[2]);

                                    if (numbers.Length == 9)
                                    {
                                        point.Color = new float3(numbers[3], numbers[4], numbers[5]);
                                        point.EchoId = numbers[6];
                                        point.ScanNr = numbers[8];
                                    }

                                    pointCloud.AddPoint(point);
                                }
                            }

                            pointCloud.FlushPoints();
                            return pointCloud;
                        },
                        Checker = id => Path.GetExtension(id).ToLower().Contains("txt")
                    });

                AssetStorage.RegisterProvider(fap);

                var app = new Core.PointVisualizationBase();
                

		        // Inject Fusee.Engine InjectMe dependencies (hard coded)
		        RenderCanvasImp rci = new RenderCanvasImp(ApplicationContext, null, delegate { app.Run(); });
		        app.CanvasImplementor = rci;
		        app.ContextImplementor = new RenderContextImp(rci, ApplicationContext);

		        SetContentView(rci.View);

                //app.ParticleSize = 0.05f;
                //show display dimensions for testing
                // IWindowManager wm = ApplicationContext.GetSystemService(WindowService).JavaCast<IWindowManager>() ;
                //Display display = wm.DefaultDisplay;
                //app._screenSize = new float2(display.Width, display.Height);
                // float pixel_height = display.Height;
                //float pixel_width = display.Width;
                // string output = "Width: " + pixel_height + " Height:" + pixel_width;
                //Show roasted bread
                //  Toast.MakeText(ApplicationContext, output, ToastLength.Short).Show();

                Engine.Core.Input.AddDriverImp(
		            new Fusee.Engine.Imp.Graphics.Android.RenderCanvasInputDriverImp(app.CanvasImplementor));
                //Engine.Core.Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Android.WindowsTouchInputDriverImp(app.CanvasImplementor));
                // Deleayed into rendercanvas imp....app.Run() - SEE DELEGATE ABOVE;
            }
		    else
		    {
                Toast.MakeText(ApplicationContext, "Hardware does not support OpenGL ES 3.0 - Aborting...", ToastLength.Long);
                Log.Info("@string/app_name", "Hardware does not support OpenGL ES 3.0 - Aborting...");
            }
        }


        /// <summary>
        /// Gets the supported OpenGL ES version of device.
        /// </summary>
        /// <returns>Hieghest supported version of OpenGL ES</returns>
        private long SupportedOpenGLVersion()
        {
            //based on https://android.googlesource.com/platform/cts/+/master/tests/tests/graphics/src/android/opengl/cts/OpenGlEsVersionTest.java
            var featureInfos = PackageManager.GetSystemAvailableFeatures();
            if (featureInfos != null && featureInfos.Length > 0)
            {
                foreach (FeatureInfo info in featureInfos)
                {
                    // Null feature name means this feature is the open gl es version feature.
                    if (info.Name == null)
                    {
                        if (info.ReqGlEsVersion != FeatureInfo.GlEsVersionUndefined)
                            return GetMajorVersion(info.ReqGlEsVersion);
                        else
                            return 0L;
                    }
                }
            }
            return 0L;
        }

        private static long GetMajorVersion(long raw)
        {
            //based on https://android.googlesource.com/platform/cts/+/master/tests/tests/graphics/src/android/opengl/cts/OpenGlEsVersionTest.java
            long cleaned = ((raw & 0xffff0000) >> 16);
            Log.Info("GLVersion", "OpenGL ES major version: " + cleaned);
            return cleaned;
        }

    }
}
