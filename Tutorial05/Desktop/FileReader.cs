﻿using System;
using System.IO;
using Fusee.Base.Core;
using Fusee.Tutorial.Core;
using Fusee.Math.Core;
using System.Globalization;
using System.Diagnostics;

namespace Fusee.Tutorial.Desktop
{

    public class FileReader
    {
        //member variables
        private PointCloud _cloud;

        //constructor
        public FileReader(PointCloud cloudinput)
        {
            _cloud = cloudinput;
        }

        //getter & setter
        public PointCloud Cloud
        {
            get { return _cloud; }
            set { _cloud = value; }

        }
        public void writePointList()
        {
            var fap = new Fusee.Base.Imp.Desktop.FileAssetProvider("Assets");
            //File Asset Provider kann Textdatei auslesen (Android?)

            string path = "Assets/small_test_points.txt";

            string[] readText = File.ReadAllLines(path);
            string output;

            for (var i = 0; i < readText.Length; i++)
            {
                {
                    string line = readText[i];
                    string[] splitted = line.Split('\t');
                    float x = (float.Parse(splitted[0], CultureInfo.InvariantCulture.NumberFormat));
                    float y = (float.Parse(splitted[1], CultureInfo.InvariantCulture.NumberFormat));
                    float z = (float.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat));
                    float3 insertvalue =  new float3(x, y, z);
                    _cloud.Vertices.Add(insertvalue);
                    output = ("x: " + splitted[0] + "; y: " + splitted[1] + "; z: " + splitted[2] + "; z2: " + splitted[3]);
                    Debug.WriteLine(output);

                   
                }
            }
        }
    }
}