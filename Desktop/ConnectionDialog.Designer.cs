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
            this.connectButton = new System.Windows.Forms.Button();
            this.receivedDataText = new System.Windows.Forms.RichTextBox();
            this.receivedLabel = new System.Windows.Forms.Label();
            this.disconButton = new System.Windows.Forms.Button();
            this.statusText = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.connectButton.Location = new System.Drawing.Point(60, 184);
            this.connectButton.Margin = new System.Windows.Forms.Padding(4);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(525, 41);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click_1);
            // 
            // receivedDataText
            // 
            this.receivedDataText.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.receivedDataText.Location = new System.Drawing.Point(59, 347);
            this.receivedDataText.MinimumSize = new System.Drawing.Size(525, 258);
            this.receivedDataText.Name = "receivedDataText";
            this.receivedDataText.Size = new System.Drawing.Size(525, 272);
            this.receivedDataText.TabIndex = 3;
            this.receivedDataText.Text = "";
            // 
            // receivedLabel
            // 
            this.receivedLabel.AutoSize = true;
            this.receivedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.receivedLabel.Location = new System.Drawing.Point(56, 309);
            this.receivedLabel.MinimumSize = new System.Drawing.Size(132, 24);
            this.receivedLabel.Name = "receivedLabel";
            this.receivedLabel.Size = new System.Drawing.Size(132, 24);
            this.receivedLabel.TabIndex = 4;
            this.receivedLabel.Text = "Received Data";
            // 
            // disconButton
            // 
            this.disconButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.disconButton.Location = new System.Drawing.Point(60, 244);
            this.disconButton.Name = "disconButton";
            this.disconButton.Size = new System.Drawing.Size(525, 43);
            this.disconButton.TabIndex = 5;
            this.disconButton.Text = "Disconnect";
            this.disconButton.UseVisualStyleBackColor = true;
            this.disconButton.Click += new System.EventHandler(this.disconButton_Click);
            // 
            // statusText
            // 
            this.statusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.statusText.Location = new System.Drawing.Point(64, 48);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(525, 96);
            this.statusText.TabIndex = 6;
            this.statusText.Text = "Ready to Connect";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(60, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Status";
            // 
            // ConnectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 658);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.disconButton);
            this.Controls.Add(this.receivedLabel);
            this.Controls.Add(this.receivedDataText);
            this.Controls.Add(this.connectButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ConnectionDialog";
            this.Text = "Setup connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.RichTextBox receivedDataText;
        private System.Windows.Forms.Label receivedLabel;
        private System.Windows.Forms.Button disconButton;
        private System.Windows.Forms.RichTextBox statusText;
        private System.Windows.Forms.Label label1;
    }
}