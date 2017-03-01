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
using Fusee.Forschungsprojekt.Core;
using static Fusee.Engine.Core.Time; // frame rate
using Font = Fusee.Base.Core.Font;
using Path = Fusee.Base.Common.Path;
using Fusee.Math.Core;
using System;
using System.Net.Sockets;
using System.Globalization;
using System.Text;
using System.Net;

namespace Fusee.Forschungsprojekt.Android
{
	[Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon",
#if __ANDROID_11__
		HardwareAccelerated=false,
#endif
		ConfigurationChanges = ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
	public class MainActivity : Activity
	{
	    private FrameRateLogger _fRL;
        private RelativeLayout canvas_view;
        private Button plusButton;
        private Button minusButton;
        private Core.PointVisualizationBase app;


        protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Forschungsprojekt.Android.Resource.Layout.main_activity_layout);
            canvas_view = FindViewById<RelativeLayout>(Forschungsprojekt.Android.Resource.Id.canvas_container);
            plusButton = FindViewById<Button>(Forschungsprojekt.Android.Resource.Id.plus_btn);
            minusButton = FindViewById<Button>(Forschungsprojekt.Android.Resource.Id.minus_btn);

            if (SupportedOpenGLVersion() >= 3)
		    {
                //_fRL = new FrameRateLogger(); // start logging frame rate on console

                // SetContentView(new LibPaintingView(ApplicationContext, null));

                //Simple tcp connection test
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IP Address of sender device/server: change to your current IPv4 Address for debugging!
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.30"), 1994);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.32"), 1994);
                socket.Connect(endPoint);

                //send connection message to server
                string msg = "Connected";
                byte[] msgBuffer = Encoding.Default.GetBytes(msg);
                socket.Send(msgBuffer, 0, msgBuffer.Length, 0);
               

                new System.Threading.Thread(() => //thread for receiving data
                {
                    //wait to receive data//PROGRAM NOT DOES NOT CONTINUE until data received
                    byte[] buffer = new byte[1014];
                    int receive = socket.Receive(buffer, 0, buffer.Length, 0);
                    //resize buffer
                    Array.Resize(ref buffer, receive);
                    //write received message to debug console
                    string output = ("RECEIVED from Server: " + Encoding.Default.GetString(buffer));  
                    System.Diagnostics.Debug.WriteLine(output);

                }).Start();
                

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

                //buttons to increase or decrease particle size
                plusButton.Click += (sender, e) =>
                {
                    app.ParticleSize = app.ParticleSize + app.ParticleSize / 2;
                };

                minusButton.Click += (sender, e) =>
                {
                    app.ParticleSize = app.ParticleSize - app.ParticleSize / 2;
                };


                // Inject Fusee.Engine InjectMe dependencies (hard coded)
                RenderCanvasImp rci = new RenderCanvasImp(ApplicationContext, null, delegate { app.Run(); });
		        app.CanvasImplementor = rci;
		        app.ContextImplementor = new RenderContextImp(rci, ApplicationContext);

                //SetContentView(rci.View);
                canvas_view.AddView(rci.View);
                app.ParticleSize = 0.05f;



                //show display dimensions for testing
                IWindowManager wm = ApplicationContext.GetSystemService(WindowService).JavaCast<IWindowManager>() ;
                //Display display = wm.DefaultDisplay;
                ////app._screenSize = new float2(display.Width, display.Height);
                //float pixel_height = display.Height;
                //float pixel_width = display.Width;
                //string output = "Width: " + pixel_height + " Height:" + pixel_width;
                ////Show roasted bread
                //Toast.MakeText(ApplicationContext, output, ToastLength.Short).Show();

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
