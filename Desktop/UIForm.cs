using Fusee.Engine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            renderPanel = new Panel();
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

            // First create a WinformsHost around the control
            currentHost = new WinformsHost(currentControl, this);

            // Then instantiate your app (could be as well _currentApp = new MyOwnRenderCanvasDerivedClass(); )
            currentApp = Simple.app;

            // Now use the host as the canvas AND the input implementation of your App
            currentApp.CanvasImplementor = currentHost;
            //currentApp.ContextImplementor= currentHost;
           

            // Then you can run the app
            currentApp.Run();

            // If not already done, show the window.
            currentControl.Show();
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

    }
}
