using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Base.Imp.Desktop;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Tutorial.Core;
using Path = Fusee.Base.Common.Path;

namespace Fusee.Tutorial.Desktop
{
    public class Simple
    {

        public static void Main()
        {

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
            app.CanvasImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasImp();
            app.ContextImplementor = new Fusee.Engine.Imp.Graphics.Desktop.RenderContextImp(app.CanvasImplementor);
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(app.CanvasImplementor));
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.RenderCanvasInputDriverImp(app.CanvasImplementor));
            Input.AddDriverImp(new Fusee.Engine.Imp.Graphics.Desktop.WindowsTouchInputDriverImp(app.CanvasImplementor));
            //app.InputImplementor = new Fusee.Engine.Imp.Graphics.Desktop.InputImp(app.CanvasImplementor);
            //app.AudioImplementor = new Fusee.Engine.Imp.Sound.Desktop.AudioImp();
            //app.NetworkImplementor = new Fusee.Engine.Imp.Network.Desktop.NetworkImp();
            //app.InputDriverImplementor = new Fusee.Engine.Imp.Input.Desktop.InputDriverImp();
            // app.VideoManagerImplementor = ImpFactory.CreateIVideoManagerImp();

            //set starting particle size

            app.ParticleSize = 0.2f;

            // Start the app
            app.Run();

        }

    }

}
 