using Fusee.Engine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Imp.Graphics.Desktop;
using Fusee.Forschungsprojekt.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Font = System.Drawing.Font;
using Path = Fusee.Base.Common.Path;
using Point = System.Drawing.Point;

namespace Fusee.Forschungsprojekt.Desktop
{
    public partial class UIForm : Form
    {
        public Panel renderPanel;
        public IntPtr panelHandle;
        private RenderCanvas currentApp;
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
                BackColor = Color.Black,
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
            //
            //  STEP TWO - Now the underlying Windows Window was created - we can hook OpenGL on it.
            //

            // Take this as an example how to hook up any FUSEE application with a given Winforms form:

           

            // Then instantiate your app (could be as well _currentApp = new MyOwnRenderCanvasDerivedClass(); )
         

            // Now use the host as the canvas AND the input implementation of your App
            //currentApp.CanvasImplementor = currentHost;
            //currentApp.ContextImplementor= currentHost;


            // Inject Fusee.Engine.Base InjectMe dependencies
            IO.IOImp = new Fusee.Base.Imp.Desktop.IOImp();

            var fap = new Fusee.Base.Imp.Desktop.FileAssetProvider("Assets");
            fap.RegisterTypeHandler(
                new AssetHandler
                {
                    ReturnedType = typeof(Base.Core.Font),
                    Decoder = delegate (string id, object storage)
                    {
                        if (!Path.GetExtension(id).ToLower().Contains("ttf")) return null;
                        return new Base.Core.Font { _fontImp = new FontImp((Stream)storage) };
                    },
                    Checker = id => Path.GetExtension(id).ToLower().Contains("ttf")
                });
            //fap.RegisterTypeHandler(
            //    new AssetHandler
            //    {
            //        ReturnedType = typeof(SceneContainer),
            //        Decoder = delegate (string id, object storage)
            //        {
            //            if (!Path.GetExtension(id).ToLower().Contains("fus")) return null;
            //            var ser = new Serializer();
            //DESERIALIZE ERROR?? --> protobuf reference missing?
            //            return ser.Deserialize((Stream)storage, null, typeof(SceneContainer)) as SceneContainer;
            //        },
            //        Checker = id => Path.GetExtension(id).ToLower().Contains("fus")
            //    });
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

                                Core.Point point = new Core.Point();
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

            // First create a WinformsHost around the control
            currentHost = new WinformsHost(currentControl, this);

            currentApp = new Core.PointVisualizationBase();

            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            currentApp.CanvasImplementor = currentHost;
            currentApp.ContextImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderContextImp(currentApp.CanvasImplementor);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(currentHost.canvas));

            //app.InputImplementor = new Fusee.Engine.Imp.Graphics.Desktop.InputImp(app.CanvasImplementor);
            //app.AudioImplementor = new Fusee.Engine.Imp.Sound.Desktop.AudioImp();
            //app.NetworkImplementor = new Fusee.Engine.Imp.Network.Desktop.NetworkImp();
            //app.InputDriverImplementor = new Fusee.Engine.Imp.Input.Desktop.InputDriverImp();
            //app.VideoManagerImplementor = ImpFactory.CreateIVideoManagerImp();

            //// If not already done, show the window.
            currentControl.Show();

            //// Then you can run the app
            currentApp.Run();

           
        }

        private void plus_button_Click(object sender, EventArgs e)
        {

        }

        private void minus_button_Click(object sender, EventArgs e)
        {

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

        private void canvaspanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
