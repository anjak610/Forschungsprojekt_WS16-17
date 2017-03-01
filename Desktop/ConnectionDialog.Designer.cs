namespace Fusee.Forschungsprojekt.Desktop
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
            this.sendButton = new System.Windows.Forms.Button();
            this.inputBox = new System.Windows.Forms.TextBox();
            this.enterIPlabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sendButton
            // 
            this.sendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.sendButton.Location = new System.Drawing.Point(410, 106);
            this.sendButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(175, 55);
            this.sendButton.TabIndex = 0;
            this.sendButton.Text = "Connect";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click_1);
            // 
            // inputBox
            // 
            this.inputBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.inputBox.Location = new System.Drawing.Point(60, 114);
            this.inputBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(294, 28);
            this.inputBox.TabIndex = 1;
            // 
            // enterIPlabel
            // 
            this.enterIPlabel.AutoSize = true;
            this.enterIPlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.enterIPlabel.Location = new System.Drawing.Point(55, 40);
            this.enterIPlabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.enterIPlabel.MinimumSize = new System.Drawing.Size(545, 24);
            this.enterIPlabel.Name = "enterIPlabel";
            this.enterIPlabel.Size = new System.Drawing.Size(545, 24);
            this.enterIPlabel.TabIndex = 2;
            this.enterIPlabel.Text = "Please enter IP Adress of this device  (for example 192.168.1.32)";
            // 
            // ConnectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 235);
            this.Controls.Add(this.enterIPlabel);
            this.Controls.Add(this.inputBox);
            this.Controls.Add(this.sendButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ConnectionDialog";
            this.Text = "Setup connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox inputBox;
        private System.Windows.Forms.Label enterIPlabel;
    }
}