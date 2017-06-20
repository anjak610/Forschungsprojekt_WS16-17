namespace Fusee.Tutorial.Desktop.HelperClasses
{
    partial class UIForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UIForm));
            this.canvaspanel = new System.Windows.Forms.Panel();
            this.label_particle_size = new System.Windows.Forms.Label();
            this.plus_button = new System.Windows.Forms.Button();
            this.minus_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.intensity_radio_btn = new System.Windows.Forms.RadioButton();
            this.depth_radio_btn = new System.Windows.Forms.RadioButton();
            this.echo_txt = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.echo_id_input = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.vsp_radio_btn = new System.Windows.Forms.RadioButton();
            this.pcl_radio_btn = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.port_input = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.echo_id_input)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port_input)).BeginInit();
            this.SuspendLayout();
            // 
            // canvaspanel
            // 
            this.canvaspanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvaspanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.canvaspanel.Location = new System.Drawing.Point(304, 0);
            this.canvaspanel.Margin = new System.Windows.Forms.Padding(70, 5, 5, 5);
            this.canvaspanel.Name = "canvaspanel";
            this.canvaspanel.Size = new System.Drawing.Size(877, 553);
            this.canvaspanel.TabIndex = 0;
            // 
            // label_particle_size
            // 
            this.label_particle_size.AutoSize = true;
            this.label_particle_size.Location = new System.Drawing.Point(28, 85);
            this.label_particle_size.MinimumSize = new System.Drawing.Size(90, 18);
            this.label_particle_size.Name = "label_particle_size";
            this.label_particle_size.Size = new System.Drawing.Size(90, 18);
            this.label_particle_size.TabIndex = 1;
            this.label_particle_size.Text = "Particle Size";
            this.label_particle_size.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // plus_button
            // 
            this.plus_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.plus_button.Location = new System.Drawing.Point(87, 115);
            this.plus_button.MinimumSize = new System.Drawing.Size(50, 40);
            this.plus_button.Name = "plus_button";
            this.plus_button.Size = new System.Drawing.Size(50, 40);
            this.plus_button.TabIndex = 2;
            this.plus_button.Text = "+";
            this.plus_button.UseVisualStyleBackColor = true;
            this.plus_button.Click += new System.EventHandler(this.plus_button_Click);
            // 
            // minus_button
            // 
            this.minus_button.Location = new System.Drawing.Point(31, 115);
            this.minus_button.MinimumSize = new System.Drawing.Size(50, 40);
            this.minus_button.Name = "minus_button";
            this.minus_button.Size = new System.Drawing.Size(50, 40);
            this.minus_button.TabIndex = 3;
            this.minus_button.Text = "-";
            this.minus_button.UseVisualStyleBackColor = true;
            this.minus_button.Click += new System.EventHandler(this.minus_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.intensity_radio_btn);
            this.groupBox1.Controls.Add(this.depth_radio_btn);
            this.groupBox1.Location = new System.Drawing.Point(13, 402);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(242, 96);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Shading";
            // 
            // intensity_radio_btn
            // 
            this.intensity_radio_btn.AutoSize = true;
            this.intensity_radio_btn.Location = new System.Drawing.Point(31, 60);
            this.intensity_radio_btn.Name = "intensity_radio_btn";
            this.intensity_radio_btn.Size = new System.Drawing.Size(82, 22);
            this.intensity_radio_btn.TabIndex = 1;
            this.intensity_radio_btn.Text = "Intensity";
            this.intensity_radio_btn.UseVisualStyleBackColor = true;
            this.intensity_radio_btn.Click += new System.EventHandler(this.intensity_radio_btn_Click);
            // 
            // depth_radio_btn
            // 
            this.depth_radio_btn.AutoSize = true;
            this.depth_radio_btn.Checked = true;
            this.depth_radio_btn.Location = new System.Drawing.Point(31, 32);
            this.depth_radio_btn.Name = "depth_radio_btn";
            this.depth_radio_btn.Size = new System.Drawing.Size(125, 22);
            this.depth_radio_btn.TabIndex = 0;
            this.depth_radio_btn.TabStop = true;
            this.depth_radio_btn.Text = "Depth Shading";
            this.depth_radio_btn.UseVisualStyleBackColor = true;
            this.depth_radio_btn.Click += new System.EventHandler(this.depth_radio_btn_Click);
            // 
            // echo_txt
            // 
            this.echo_txt.AutoSize = true;
            this.echo_txt.Location = new System.Drawing.Point(28, 42);
            this.echo_txt.Name = "echo_txt";
            this.echo_txt.Size = new System.Drawing.Size(58, 18);
            this.echo_txt.TabIndex = 7;
            this.echo_txt.Text = "Echo Id";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(298, 553);
            this.panel1.TabIndex = 8;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.echo_id_input);
            this.groupBox4.Controls.Add(this.echo_txt);
            this.groupBox4.Controls.Add(this.label_particle_size);
            this.groupBox4.Controls.Add(this.plus_button);
            this.groupBox4.Controls.Add(this.minus_button);
            this.groupBox4.Location = new System.Drawing.Point(13, 223);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(242, 173);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Point Cloud Visualization";
            // 
            // echo_id_input
            // 
            this.echo_id_input.Location = new System.Drawing.Point(91, 40);
            this.echo_id_input.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.echo_id_input.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.echo_id_input.Name = "echo_id_input";
            this.echo_id_input.Size = new System.Drawing.Size(72, 24);
            this.echo_id_input.TabIndex = 8;
            this.echo_id_input.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.echo_id_input.Click += new System.EventHandler(this.echo_id_input_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.vsp_radio_btn);
            this.groupBox3.Controls.Add(this.pcl_radio_btn);
            this.groupBox3.Location = new System.Drawing.Point(13, 110);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(242, 107);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "View Mode";
            // 
            // vsp_radio_btn
            // 
            this.vsp_radio_btn.AutoSize = true;
            this.vsp_radio_btn.Location = new System.Drawing.Point(31, 64);
            this.vsp_radio_btn.Name = "vsp_radio_btn";
            this.vsp_radio_btn.Size = new System.Drawing.Size(111, 22);
            this.vsp_radio_btn.TabIndex = 1;
            this.vsp_radio_btn.Text = "Voxel Space";
            this.vsp_radio_btn.UseVisualStyleBackColor = true;
            this.vsp_radio_btn.Click += new System.EventHandler(this.vsp_radio_btn_Click);
            // 
            // pcl_radio_btn
            // 
            this.pcl_radio_btn.AutoSize = true;
            this.pcl_radio_btn.Checked = true;
            this.pcl_radio_btn.Location = new System.Drawing.Point(31, 36);
            this.pcl_radio_btn.Name = "pcl_radio_btn";
            this.pcl_radio_btn.Size = new System.Drawing.Size(106, 22);
            this.pcl_radio_btn.TabIndex = 0;
            this.pcl_radio_btn.TabStop = true;
            this.pcl_radio_btn.Text = "Point Cloud";
            this.pcl_radio_btn.UseVisualStyleBackColor = true;
            this.pcl_radio_btn.Click += new System.EventHandler(this.pcl_radio_btn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.port_input);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(13, 15);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(242, 88);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "UDP Connection";
            // 
            // port_input
            // 
            this.port_input.Location = new System.Drawing.Point(70, 39);
            this.port_input.Maximum = new decimal(new int[] {
            65000,
            0,
            0,
            0});
            this.port_input.Name = "port_input";
            this.port_input.Size = new System.Drawing.Size(112, 24);
            this.port_input.TabIndex = 3;
            this.port_input.Value = new decimal(new int[] {
            50123,
            0,
            0,
            0});
            this.port_input.Click += new System.EventHandler(this.port_input_Click);
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1182, 553);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.canvaspanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UIForm";
            this.Text = "Fusee Cloud Vision";
            this.Load += new System.EventHandler(this.UIForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.echo_id_input)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port_input)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel canvaspanel;
        private System.Windows.Forms.Label label_particle_size;
        private System.Windows.Forms.Button plus_button;
        private System.Windows.Forms.Button minus_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton intensity_radio_btn;
        private System.Windows.Forms.RadioButton depth_radio_btn;
        private System.Windows.Forms.Label echo_txt;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown port_input;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.NumericUpDown echo_id_input;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton vsp_radio_btn;
        private System.Windows.Forms.RadioButton pcl_radio_btn;
    }
}