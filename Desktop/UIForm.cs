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
        public UIForm()
        {
            InitializeComponent();
            //initialize app and canvas here
            //TODO: How to get render canvas of core to be displayed in panel??
            // canvaspanel.Controls.Add(Core.PointVisualizationBase.RC.Viewport); --> RC not accessible
        }

        private void plus_button_Click(object sender, EventArgs e)
        {

        }

        private void minus_button_Click(object sender, EventArgs e)
        {

        }
    }
}
