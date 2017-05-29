namespace Fusee.Tutorial.Desktop.HelperClasses
{
    partial class ConnectionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionDialog));
            this.connectButton = new System.Windows.Forms.Button();
            this.disconButton = new System.Windows.Forms.Button();
            this.statusText = new System.Windows.Forms.RichTextBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.IPLabel = new System.Windows.Forms.Label();
            this.IPinputBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.connectButton.Location = new System.Drawing.Point(11, 105);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(611, 41);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click_1);
            // 
            // disconButton
            // 
            this.disconButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.disconButton.Location = new System.Drawing.Point(12, 607);
            this.disconButton.Name = "disconButton";
            this.disconButton.Size = new System.Drawing.Size(612, 43);
            this.disconButton.TabIndex = 5;
            this.disconButton.Text = "Disconnect";
            this.disconButton.UseVisualStyleBackColor = true;
            this.disconButton.Click += new System.EventHandler(this.disconButton_Click);
            // 
            // statusText
            // 
            this.statusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.statusText.Location = new System.Drawing.Point(12, 197);
            this.statusText.Name = "statusText";
            this.statusText.ReadOnly = true;
            this.statusText.Size = new System.Drawing.Size(611, 380);
            this.statusText.TabIndex = 6;
            this.statusText.Text = "Ready to Connect";
            this.statusText.TextChanged += new System.EventHandler(this.statusText_TextChanged);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.statusLabel.Location = new System.Drawing.Point(12, 163);
            this.statusLabel.MinimumSize = new System.Drawing.Size(57, 20);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(57, 20);
            this.statusLabel.TabIndex = 7;
            this.statusLabel.Text = "Status";
            // 
            // IPLabel
            // 
            this.IPLabel.AutoSize = true;
            this.IPLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.IPLabel.Location = new System.Drawing.Point(12, 41);
            this.IPLabel.MinimumSize = new System.Drawing.Size(200, 20);
            this.IPLabel.Name = "IPLabel";
            this.IPLabel.Size = new System.Drawing.Size(200, 20);
            this.IPLabel.TabIndex = 8;
            this.IPLabel.Text = "Please enter IP of Server";
            // 
            // IPinputBox
            // 
            this.IPinputBox.Location = new System.Drawing.Point(262, 36);
            this.IPinputBox.Name = "IPinputBox";
            this.IPinputBox.Size = new System.Drawing.Size(224, 28);
            this.IPinputBox.TabIndex = 9;
            // 
            // ConnectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 673);
            this.Controls.Add(this.IPinputBox);
            this.Controls.Add(this.IPLabel);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.disconButton);
            this.Controls.Add(this.connectButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ConnectionDialog";
            this.Text = "Setup Connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button disconButton;
        private System.Windows.Forms.RichTextBox statusText;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label IPLabel;
        private System.Windows.Forms.TextBox IPinputBox;
    }
}