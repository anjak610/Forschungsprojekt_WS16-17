﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Tutorial.Core;
using Fusee.Serialization;
using Path = Fusee.Base.Common.Path;
using Fusee.Tutorial.Desktop;
using CrossProject;

namespace CrossProject
{
    public partial class StaticVariables
    {
        public static PointCloud cloud;
        public static FileReader reader;
    }
}

namespace Fusee.Tutorial.Desktop
{
    public class Simple
    {
        //public static PointCloud cloud;
        //public static FileReader reader;
      

        public static void Main()   
        {

            StaticVariables.cloud = new PointCloud();
            StaticVariables.reader = new FileReader(StaticVariables.cloud);
            StaticVariables.reader.writePointList();

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
                        return new Font{ _fontImp = new FontImp((Stream)storage) };
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
        
            
            

            /*fap.RegisterTypeHandler( // TO-DO: implement on other platforms as well
                new AssetHandler
                {
                    ReturnedType = typeof(PointCloud),
                    Decoder = delegate(string id, object storage)
                    {
                        if (!Path.GetExtension(id).ToLower().Contains("ipm")) return null;

                        List<float3> vertices = new List<float3>();
                        List<float3> colors = new List<float3>();
                        List<float> echoIds = new List<float>();
                        List<float> scanNrs = new List<float>();

                        using (var sr = new StreamReader((Stream)storage, System.Text.Encoding.Default, true))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null) // read per line
                            {
                                string delimiter = "\t";
                                string[] textElements = line.Split(delimiter.ToCharArray());

                                if (textElements.Length == 1) // end of file
                                    break;

                                float[] numbers = Array.ConvertAll(textElements, n => float.Parse(n, CultureInfo.InvariantCulture.NumberFormat));
                                
                                vertices.Add(new float3(numbers[0], numbers[1], numbers[2]));

                                if (numbers.Length == 9)
                                {
                                    colors.Add(new float3(numbers[3], numbers[4], numbers[5]));
                                    echoIds.Add(numbers[6]);
                                    scanNrs.Add(numbers[8]);
                                }
                            }
                        }
                        
                        PointCloud pointCloud = new PointCloud();
                        pointCloud.Vertices = vertices.ToArray();
                        pointCloud.Colors = colors.ToArray();

                        if (echoIds.Count > 0)
                        {
                            pointCloud.EchoIds = echoIds.ToArray();
                            pointCloud.ScanNrs = scanNrs.ToArray();
                        }

                        return pointCloud;
                    },
                    Checker = id => Path.GetExtension(id).ToLower().Contains("ipm")
                });*/

            AssetStorage.RegisterProvider(fap);

            var app = new Core.PointVisualizationBase();

            // Inject Fusee.Engine InjectMe dependencies (hard coded)
            app.CanvasImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasImp();
            app.ContextImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderContextImp(app.CanvasImplementor);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(app.CanvasImplementor));
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(app.CanvasImplementor));
            //app.InputImplementor = new Fusee.Engine.Imp.Graphics.Desktop.InputImp(app.CanvasImplementor);
            //app.AudioImplementor = new Fusee.Engine.Imp.Sound.Desktop.AudioImp();
            //app.NetworkImplementor = new Fusee.Engine.Imp.Network.Desktop.NetworkImp();
            //app.InputDriverImplementor = new Fusee.Engine.Imp.Input.Desktop.InputDriverImp();
            // app.VideoManagerImplementor = ImpFactory.CreateIVideoManagerImp();

            // Start the app
            app.Run();
        }
    }
}
