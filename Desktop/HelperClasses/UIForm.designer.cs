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
            this.chg_view_btn = new System.Windows.Forms.Button();
            this.label_particle_size = new System.Windows.Forms.Label();
            this.plus_button = new System.Windows.Forms.Button();
            this.minus_button = new System.Windows.Forms.Button();
            this.port_txt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.port_apply_btn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.intensity_radio_btn = new System.Windows.Forms.RadioButton();
            this.depth_radio_btn = new System.Windows.Forms.RadioButton();
            this.echo_minus_btn = new System.Windows.Forms.Button();
            this.echo_plus_btn = new System.Windows.Forms.Button();
            this.echo_txt = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // canvaspanel
            // 
            this.canvaspanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvaspanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.canvaspanel.Location = new System.Drawing.Point(0, 75);
            this.canvaspanel.Margin = new System.Windows.Forms.Padding(70, 5, 5, 5);
            this.canvaspanel.MinimumSize = new System.Drawing.Size(1180, 600);
            this.canvaspanel.Name = "canvaspanel";
            this.canvaspanel.Size = new System.Drawing.Size(1182, 600);
            this.canvaspanel.TabIndex = 0;
            // 
            // chg_view_btn
            // 
            this.chg_view_btn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.chg_view_btn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chg_view_btn.Location = new System.Drawing.Point(521, 19);
            this.chg_view_btn.MinimumSize = new System.Drawing.Size(180, 40);
            this.chg_view_btn.Name = "chg_view_btn";
            this.chg_view_btn.Size = new System.Drawing.Size(181, 40);
            this.chg_view_btn.TabIndex = 1;
            this.chg_view_btn.Text = "Change View";
            this.chg_view_btn.UseVisualStyleBackColor = false;
            this.chg_view_btn.Click += new System.EventHandler(this.chg_view_btn_Click);
            // 
            // label_particle_size
            // 
            this.label_particle_size.AutoSize = true;
            this.label_particle_size.Location = new System.Drawing.Point(970, 19);
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
            this.plus_button.Location = new System.Drawing.Point(1122, 19);
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
            this.minus_button.Location = new System.Drawing.Point(1066, 19);
            this.minus_button.MinimumSize = new System.Drawing.Size(50, 40);
            this.minus_button.Name = "minus_button";
            this.minus_button.Size = new System.Drawing.Size(50, 40);
            this.minus_button.TabIndex = 3;
            this.minus_button.Text = "-";
            this.minus_button.UseVisualStyleBackColor = true;
            this.minus_button.Click += new System.EventHandler(this.minus_button_Click);
            // 
            // port_txt
            // 
            this.port_txt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.port_txt.Location = new System.Drawing.Point(15, 29);
            this.port_txt.Name = "port_txt";
            this.port_txt.Size = new System.Drawing.Size(155, 21);
            this.port_txt.TabIndex = 0;
            this.port_txt.Text = "50123";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port";
            // 
            // port_apply_btn
            // 
            this.port_apply_btn.Location = new System.Drawing.Point(176, 28);
            this.port_apply_btn.Name = "port_apply_btn";
            this.port_apply_btn.Size = new System.Drawing.Size(75, 24);
            this.port_apply_btn.TabIndex = 2;
            this.port_apply_btn.Text = "Apply";
            this.port_apply_btn.UseVisualStyleBackColor = true;
            this.port_apply_btn.Click += new System.EventHandler(this.port_apply_btn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.intensity_radio_btn);
            this.groupBox1.Controls.Add(this.depth_radio_btn);
            this.groupBox1.Location = new System.Drawing.Point(286, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 55);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Shading";
            // 
            // intensity_radio_btn
            // 
            this.intensity_radio_btn.AutoSize = true;
            this.intensity_radio_btn.Location = new System.Drawing.Point(120, 32);
            this.intensity_radio_btn.Name = "intensity_radio_btn";
            this.intensity_radio_btn.Size = new System.Drawing.Size(69, 19);
            this.intensity_radio_btn.TabIndex = 1;
            this.intensity_radio_btn.Text = "Intensity";
            this.intensity_radio_btn.UseVisualStyleBackColor = true;
            this.intensity_radio_btn.Click += new System.EventHandler(this.intensity_radio_btn_Click);
            // 
            // depth_radio_btn
            // 
            this.depth_radio_btn.AutoSize = true;
            this.depth_radio_btn.Checked = true;
            this.depth_radio_btn.Location = new System.Drawing.Point(7, 32);
            this.depth_radio_btn.Name = "depth_radio_btn";
            this.depth_radio_btn.Size = new System.Drawing.Size(107, 19);
            this.depth_radio_btn.TabIndex = 0;
            this.depth_radio_btn.TabStop = true;
            this.depth_radio_btn.Text = "Depth Shading";
            this.depth_radio_btn.UseVisualStyleBackColor = true;
            this.depth_radio_btn.Click += new System.EventHandler(this.depth_radio_btn_Click);
            // 
            // echo_minus_btn
            // 
            this.echo_minus_btn.Location = new System.Drawing.Point(816, 19);
            this.echo_minus_btn.Name = "echo_minus_btn";
            this.echo_minus_btn.Size = new System.Drawing.Size(50, 40);
            this.echo_minus_btn.TabIndex = 5;
            this.echo_minus_btn.Text = "-";
            this.echo_minus_btn.UseVisualStyleBackColor = true;
            this.echo_minus_btn.Click += new System.EventHandler(this.echo_minus_btn_Click);
            // 
            // echo_plus_btn
            // 
            this.echo_plus_btn.Location = new System.Drawing.Point(873, 19);
            this.echo_plus_btn.Name = "echo_plus_btn";
            this.echo_plus_btn.Size = new System.Drawing.Size(50, 40);
            this.echo_plus_btn.TabIndex = 6;
            this.echo_plus_btn.Text = "+";
            this.echo_plus_btn.UseVisualStyleBackColor = true;
            this.echo_plus_btn.Click += new System.EventHandler(this.echo_plus_btn_Click);
            // 
            // echo_txt
            // 
            this.echo_txt.AutoSize = true;
            this.echo_txt.Location = new System.Drawing.Point(749, 19);
            this.echo_txt.Name = "echo_txt";
            this.echo_txt.Size = new System.Drawing.Size(65, 15);
            this.echo_txt.TabIndex = 7;
            this.echo_txt.Text = "Echo Id: -1";
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.echo_txt);
            this.Controls.Add(this.echo_plus_btn);
            this.Controls.Add(this.echo_minus_btn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.port_apply_btn);
            this.Controls.Add(this.chg_view_btn);
            this.Controls.Add(this.port_txt);
            this.Controls.Add(this.minus_button);
            this.Controls.Add(this.plus_button);
            this.Controls.Add(this.label_particle_size);
            this.Controls.Add(this.canvaspanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1200, 600);
            this.Name = "UIForm";
            this.Text = "Fusee Cloud Vision";
            this.Load += new System.EventHandler(this.UIForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel canvaspanel;
        private System.Windows.Forms.Label label_particle_size;
        private System.Windows.Forms.Button plus_button;
        private System.Windows.Forms.Button minus_button;
        private System.Windows.Forms.Button chg_view_btn;
        private System.Windows.Forms.TextBox port_txt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button port_apply_btn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton intensity_radio_btn;
        private System.Windows.Forms.RadioButton depth_radio_btn;
        private System.Windows.Forms.Button echo_minus_btn;
        private System.Windows.Forms.Button echo_plus_btn;
        private System.Windows.Forms.Label echo_txt;
    }
}