﻿namespace Fusee.Tutorial.Desktop.HelperClasses
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
            this.canvaspanel = new System.Windows.Forms.Panel();
            this.label_particle_size = new System.Windows.Forms.Label();
            this.plus_button = new System.Windows.Forms.Button();
            this.minus_button = new System.Windows.Forms.Button();
            this.setup_btn = new System.Windows.Forms.Button();
            this.canvaspanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // canvaspanel
            // 
            this.canvaspanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.canvaspanel.Controls.Add(this.setup_btn);
            this.canvaspanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvaspanel.Location = new System.Drawing.Point(0, 0);
            this.canvaspanel.Margin = new System.Windows.Forms.Padding(5);
            this.canvaspanel.MinimumSize = new System.Drawing.Size(1180, 800);
            this.canvaspanel.Name = "canvaspanel";
            this.canvaspanel.Size = new System.Drawing.Size(1182, 953);
            this.canvaspanel.TabIndex = 0;
            this.canvaspanel.Paint += new System.Windows.Forms.PaintEventHandler(this.canvaspanel_Paint);
            // 
            // label_particle_size
            // 
            this.label_particle_size.AutoSize = true;
            this.label_particle_size.Location = new System.Drawing.Point(12, 17);
            this.label_particle_size.MinimumSize = new System.Drawing.Size(90, 18);
            this.label_particle_size.Name = "label_particle_size";
            this.label_particle_size.Size = new System.Drawing.Size(90, 18);
            this.label_particle_size.TabIndex = 1;
            this.label_particle_size.Text = "Particle Size";
            // 
            // plus_button
            // 
            this.plus_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.plus_button.Location = new System.Drawing.Point(110, 7);
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
            this.minus_button.Location = new System.Drawing.Point(166, 7);
            this.minus_button.MinimumSize = new System.Drawing.Size(50, 40);
            this.minus_button.Name = "minus_button";
            this.minus_button.Size = new System.Drawing.Size(50, 40);
            this.minus_button.TabIndex = 3;
            this.minus_button.Text = "-";
            this.minus_button.UseVisualStyleBackColor = true;
            this.minus_button.Click += new System.EventHandler(this.minus_button_Click);
            // 
            // setup_btn
            // 
            this.setup_btn.Location = new System.Drawing.Point(246, 6);
            this.setup_btn.MinimumSize = new System.Drawing.Size(150, 40);
            this.setup_btn.Name = "setup_btn";
            this.setup_btn.Size = new System.Drawing.Size(150, 40);
            this.setup_btn.TabIndex = 0;
            this.setup_btn.Text = "Setup Connection";
            this.setup_btn.UseVisualStyleBackColor = false;
            this.setup_btn.Click += new System.EventHandler(this.setup_btn_Click);
            // 
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1182, 953);
            this.Controls.Add(this.minus_button);
            this.Controls.Add(this.plus_button);
            this.Controls.Add(this.label_particle_size);
            this.Controls.Add(this.canvaspanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1200, 1000);
            this.Name = "UIForm";
            this.Text = "Fusee Point Clouds";
            this.Load += new System.EventHandler(this.UIForm_Load);
            this.canvaspanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel canvaspanel;
        private System.Windows.Forms.Label label_particle_size;
        private System.Windows.Forms.Button plus_button;
        private System.Windows.Forms.Button minus_button;
        private System.Windows.Forms.Button setup_btn;
    }
}