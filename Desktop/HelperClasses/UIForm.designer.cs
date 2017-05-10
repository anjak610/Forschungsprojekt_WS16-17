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
            this.canvaspanel = new System.Windows.Forms.Panel();
            this.chg_view_btn = new System.Windows.Forms.Button();
            this.setup_btn = new System.Windows.Forms.Button();
            this.label_particle_size = new System.Windows.Forms.Label();
            this.plus_button = new System.Windows.Forms.Button();
            this.minus_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // canvaspanel
            // 
            this.canvaspanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvaspanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.canvaspanel.Location = new System.Drawing.Point(0, 70);
            this.canvaspanel.Margin = new System.Windows.Forms.Padding(70, 5, 5, 5);
            this.canvaspanel.MinimumSize = new System.Drawing.Size(1180, 800);
            this.canvaspanel.Name = "canvaspanel";
            this.canvaspanel.Size = new System.Drawing.Size(1182, 893);
            this.canvaspanel.TabIndex = 0;
            this.canvaspanel.Paint += new System.Windows.Forms.PaintEventHandler(this.canvaspanel_Paint);
            // 
            // chg_view_btn
            // 
            this.chg_view_btn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.chg_view_btn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chg_view_btn.Location = new System.Drawing.Point(410, 7);
            this.chg_view_btn.MinimumSize = new System.Drawing.Size(180, 40);
            this.chg_view_btn.Name = "chg_view_btn";
            this.chg_view_btn.Size = new System.Drawing.Size(181, 40);
            this.chg_view_btn.TabIndex = 1;
            this.chg_view_btn.Text = "Change View";
            this.chg_view_btn.UseVisualStyleBackColor = false;
            this.chg_view_btn.Click += new System.EventHandler(this.chg_view_btn_Click);
            // 
            // setup_btn
            // 
            this.setup_btn.BackColor = System.Drawing.SystemColors.ControlLight;
            this.setup_btn.Location = new System.Drawing.Point(242, 7);
            this.setup_btn.MinimumSize = new System.Drawing.Size(150, 40);
            this.setup_btn.Name = "setup_btn";
            this.setup_btn.Size = new System.Drawing.Size(150, 40);
            this.setup_btn.TabIndex = 0;
            this.setup_btn.Text = "Setup Connection";
            this.setup_btn.UseVisualStyleBackColor = false;
            this.setup_btn.Click += new System.EventHandler(this.setup_btn_Click);
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
            // UIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1182, 953);
            this.Controls.Add(this.chg_view_btn);
            this.Controls.Add(this.minus_button);
            this.Controls.Add(this.setup_btn);
            this.Controls.Add(this.plus_button);
            this.Controls.Add(this.label_particle_size);
            this.Controls.Add(this.canvaspanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1200, 1000);
            this.Name = "UIForm";
            this.Text = "Fusee Point Clouds";
            this.Load += new System.EventHandler(this.UIForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel canvaspanel;
        private System.Windows.Forms.Label label_particle_size;
        private System.Windows.Forms.Button plus_button;
        private System.Windows.Forms.Button minus_button;
        private System.Windows.Forms.Button setup_btn;
        private System.Windows.Forms.Button chg_view_btn;
    }
}