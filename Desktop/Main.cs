using System;
using System.Globalization;
using System.IO;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Forschungsprojekt.Core;
using Path = Fusee.Base.Common.Path;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fusee.Engine.Imp.Graphics.Desktop;

namespace Fusee.Forschungsprojekt.Desktop
{
    public class Simple
    {
        private static FrameRateLogger _fRL;
        public static ConnectionDialog SetupForm;
        public static UIForm UIWindow;
        public static Core.PointVisualizationBase app;


        //TODO Window Handle in einer Windows Form Component, Render Context anhängen
        

        public static void Main()
        {
            // _fRL = new FrameRateLogger(); // start logging frame rate on console     

            //SetupForm = new ConnectionDialog();
            // SetupForm.Show();
            // UIWindow = new UIForm();




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UIForm());
            //UIWindow.Show();
            // Start the app
            //app.Run();
           


        }

    }

}
 