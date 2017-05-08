using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Android;
using Fusee.Engine.Imp.Graphics.Android;
using Fusee.Serialization;
using Font = Fusee.Base.Core.Font;
using Path = Fusee.Base.Common.Path;
using Fusee.Tutorial.Android.HelperClasses;
using Android.Runtime;
using Fusee.Tutorial.Core;
using Fusee.Tutorial.Core.PointClouds;
using Java.IO;

namespace Fusee.Tutorial.Android
{

	[Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon",
#if __ANDROID_11__
		HardwareAccelerated=false,
#endif
		ConfigurationChanges = ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
	public class MainActivity : Activity
	{
        private Button plusButton;
        private Button minusButton;
	    private Button viewMode;
        private Core.PointVisualizationBase app;
        private RelativeLayout canvas_view;

        protected override void OnCreate (Bundle savedInstanceState)
		{

			base.OnCreate (savedInstanceState);
            RequestWindowFeature(WindowFeatures.ActionBar);

            SetContentView(Resource.Layout.main_activity_layout);
            canvas_view = FindViewById<RelativeLayout>(Tutorial.Android.Resource.Id.canvas_container);
            plusButton = FindViewById<Button>(Resource.Id.plus_btn);
            minusButton = FindViewById<Button>(Resource.Id.minus_btn);
		    viewMode = FindViewById<Button>(Resource.Id.view_btn);

            this.ActionBar.SetTitle(Resource.String.actionbar_title);
            this.ActionBar.Show();

            //FrameRateLogger frl = new FrameRateLogger();

            if (SupportedOpenGLVersion() >= 3)
		    {
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
                

                AssetStorage.RegisterProvider(fap);

                var app = new Core.PointVisualizationBase();

                plusButton.Click += (sender, e) =>
                {
                    app._pointCloud.IncreaseParticleSize();
                    //app.ParticleSize = app.ParticleSize + app.ParticleSize / 2;
                };

                minusButton.Click += (sender, e) =>
                {
                    app._pointCloud.DecreaseParticleSize();
                    //app.ParticleSize = app.ParticleSize - app.ParticleSize / 2;
                };


                //Change Shader dependent on ViewMode
                viewMode.Click += (sender, e) =>
                {
                    var nextView = app._ViewMode == PointVisualizationBase.ViewMode.PointCloud ? PointVisualizationBase.ViewMode.VoxelSpace : PointVisualizationBase.ViewMode.PointCloud;
                    app.SetViewMode(nextView);
                    System.Diagnostics.Debug.WriteLine(nextView);
                };

                // connect UDPReceiver with PointCloudReader
                PointCloudReader.StartStreamingUDPCallback += new UDPReceiver().StreamFromUDP;
                
                // Inject Fusee.Engine InjectMe dependencies (hard coded)
                RenderCanvasImp rci = new RenderCanvasImp(ApplicationContext, null, delegate { app.Run(); });
		        app.CanvasImplementor = rci;
		        app.ContextImplementor = new RenderContextImp(rci, ApplicationContext);

		       // SetContentView(rci.View);
                canvas_view.AddView(rci.View);

                //app._pointCloud.SetParticleSize(0.05f);
             

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
        /// Inflates Menu Layout
        /// </summary>

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        /// <summary>
        /// Sets up the actions that happen when menu buttons are clicked
        /// </summary>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            //If someone clicks on the Add Service button
            if (id == Resource.Id.action_open_conn_dialog)
            {
                //Create a new dialog and all show on
                //That dialog
                var dialog = ConnectionDialog.NewInstance();
                dialog.Show(FragmentManager, "dialog");
            }
            return base.OnOptionsItemSelected(item);
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
