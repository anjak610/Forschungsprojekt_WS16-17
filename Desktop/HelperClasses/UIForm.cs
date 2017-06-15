using Fusee.Engine.Core;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Serialization;
using Fusee.Tutorial.Core;
using Path = Fusee.Base.Common.Path;
using Fusee.Tutorial.Core.Data;
using Font = Fusee.Base.Core.Font;

namespace Fusee.Tutorial.Desktop.HelperClasses
{
    public partial class UIForm : Form
    {
        public Panel renderPanel;
        public IntPtr panelHandle;

        private PointVisualizationBase currentApp;
        private sbyte _echoId = -1;

        private RenderControl currentControl;
        private WinformsHost currentHost;
        
        public UIForm()
        {
            InitializeComponent();
            renderPanel = this.Controls.Find("canvaspanel", true).FirstOrDefault() as Panel;
            panelHandle = renderPanel.Handle;

            var width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8f);
            var height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8f);

            SetSize(width, height);
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void SetSize(int width, int height)
        {
            Width = width + (Width - ClientSize.Width);
            Height = height + (Height - ClientSize.Height);
        }

        #region window life cycle

        public void StartCurrentApp()
        {
            Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8f);
            Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8f);

            Left = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2;
            Top = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2;

            renderPanel.Update();

            //
            //  STEP ONE - Create the Winforms Control
            //
            currentControl = new RenderControl
            {
                BackColor = System.Drawing.Color.Black,
                Location = new System.Drawing.Point(0, 0),
                Size = renderPanel.Size,
                Dock = DockStyle.Fill,
                Name = "RenderControl",
                TabIndex = 0
            };

            currentControl.HandleCreated += renderControl_HandleCreated; // <- This is crucial: Prepare for STEP TWO.

            renderPanel.Controls.Add(currentControl);
        }

        private void renderControl_HandleCreated(object sender, EventArgs e)
        {
            //  STEP TWO - Now the underlying Windows Window was created - we can hook OpenGL on it.
            // Take this as an example how to hook up any FUSEE application with a given Winforms form:
            // Then instantiate your app (could be as well _currentApp = new MyOwnRenderCanvasDerivedClass(); )

            // Inject Fusee.Engine.Base InjectMe dependencies
            //FrameRateLogger _fRL = new FrameRateLogger(); // start logging frame rate on console

            // Inject Fusee.Engine.Base InjectMe dependencies
            IO.IOImp = new Fusee.Base.Imp.Desktop.IOImp();

            var fap = new Fusee.Base.Imp.Desktop.FileAssetProvider("Assets");
            fap.RegisterTypeHandler(
                new AssetHandler
                {
                    ReturnedType = typeof(Font),
                    Decoder = delegate (string id, object storage)
                    {
                        if (!Path.GetExtension(id).ToLower().Contains("ttf")) return null;
                        return new Font { _fontImp = new FontImp((Stream)storage) };
                    },
                    Checker = id => Path.GetExtension(id).ToLower().Contains("ttf")
                });
            fap.RegisterTypeHandler(
                new AssetHandler
                {
                    ReturnedType = typeof(SceneContainer),
                    Decoder = delegate (string id, object storage)
                    {
                        if (!Path.GetExtension(id).ToLower().Contains("fus")) return null;
                        var ser = new Serializer();

                        return ser.Deserialize((Stream)storage, null, typeof(SceneContainer)) as SceneContainer;
                    },
                    Checker = id => Path.GetExtension(id).ToLower().Contains("fus")
                });

            AssetStorage.RegisterProvider(fap);

            // First create a WinformsHost around the control
            currentHost = new WinformsHost(currentControl, canvaspanel);

            currentApp = new Core.PointVisualizationBase();
            currentApp.UDPReceiver = new UDPReceiver();
            currentApp.SetUDPPort(50123);

            // Now use the host as the canvas AND the input implementation of your App
            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            currentApp.CanvasImplementor = currentHost;
            currentApp.ContextImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderContextImp(currentApp.CanvasImplementor);
            Input.AddDriverImp(currentHost);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(currentHost.canvas));

            //// If not already done, show the window.
            currentControl.Show();

            //// Then you can run the app
            currentApp.Run();
        }

        private void CloseCurrentApp()
        {

            if (currentApp != null)
            {
                currentApp.DeInit();
            }

            // Clean up
            currentApp = null;

            if (currentControl != null)
            {
                currentControl.HandleCreated -= renderControl_HandleCreated;
                currentControl.Dispose();
                renderPanel.Controls.Remove(currentControl);
                currentControl = null;
            }

            if (currentHost != null)
            {
                currentHost.Dispose();
                currentHost = null;
            }

            // Just in case...
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentApp();
        }

        private void UIForm_Load(object sender, EventArgs e)
        {
            StartCurrentApp();
        }

        #endregion
        
        #region Event Handler for UI Elements

        private void plus_button_Click(object sender, EventArgs e)
        {
            PointCloud.IncreaseParticleSize(0.02f);
        }

        private void minus_button_Click(object sender, EventArgs e)
        {
            PointCloud.DecreaseParticleSize(0.02f);
        }

        private void chg_view_btn_Click(object sender, EventArgs e)
        {
            currentApp.SwitchViewMode();
        }

        private void port_apply_btn_Click(object sender, EventArgs e)
        {
            int port;
            bool success = int.TryParse(this.port_txt.Text, out port);

            if(success)
            {
                currentApp.SetUDPPort(port);
            }
        }

        private void depth_radio_btn_Click(object sender, EventArgs e)
        {
            currentApp.SetShadingMode(PointVisualizationBase.ShadingMode.Depth);
        }

        private void intensity_radio_btn_Click(object sender, EventArgs e)
        {
            currentApp.SetShadingMode(PointVisualizationBase.ShadingMode.Intensity);
        }

        private void echo_minus_btn_Click(object sender, EventArgs e)
        {
            _echoId--;
            this.echo_txt.Text = "Echo Id: " + _echoId;

            currentApp.SetEchoId(_echoId);
        }

        private void echo_plus_btn_Click(object sender, EventArgs e)
        {
            _echoId++;
            this.echo_txt.Text = "Echo Id: " + _echoId;

            currentApp.SetEchoId(_echoId);
        }

        #endregion
    }
}
