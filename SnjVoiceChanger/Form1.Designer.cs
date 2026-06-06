namespace SnjVoiceChanger
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            leftPanel = new Panel();
            inputLevelGroupBox = new GroupBox();
            inputLevelStatusLabel = new Label();
            inputLevelMeter = new AudioLevelMeterControl();
            virtualMicGroupBox = new GroupBox();
            virtualMicStateValueLabel = new Label();
            virtualMicStateLabel = new Label();
            virtualMicNameValueLabel = new Label();
            virtualMicNameLabel = new Label();
            refreshButton = new Button();
            inputDeviceComboBox = new ComboBox();
            inputDeviceLabel = new Label();
            mainPanel = new Panel();
            mainPlaceholderLabel = new Label();
            leftPanel.SuspendLayout();
            inputLevelGroupBox.SuspendLayout();
            virtualMicGroupBox.SuspendLayout();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.FromArgb(246, 247, 249);
            leftPanel.Controls.Add(inputLevelGroupBox);
            leftPanel.Controls.Add(virtualMicGroupBox);
            leftPanel.Controls.Add(refreshButton);
            leftPanel.Controls.Add(inputDeviceComboBox);
            leftPanel.Controls.Add(inputDeviceLabel);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Margin = new Padding(3, 4, 3, 4);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(16, 19, 16, 19);
            leftPanel.Size = new Size(468, 610);
            leftPanel.TabIndex = 0;
            // 
            // inputLevelGroupBox
            // 
            inputLevelGroupBox.Controls.Add(inputLevelStatusLabel);
            inputLevelGroupBox.Controls.Add(inputLevelMeter);
            inputLevelGroupBox.Location = new Point(16, 309);
            inputLevelGroupBox.Margin = new Padding(3, 4, 3, 4);
            inputLevelGroupBox.Name = "inputLevelGroupBox";
            inputLevelGroupBox.Padding = new Padding(3, 4, 3, 4);
            inputLevelGroupBox.Size = new Size(334, 269);
            inputLevelGroupBox.TabIndex = 4;
            inputLevelGroupBox.TabStop = false;
            inputLevelGroupBox.Text = "Input signal";
            // 
            // inputLevelStatusLabel
            // 
            inputLevelStatusLabel.AutoEllipsis = true;
            inputLevelStatusLabel.ForeColor = Color.FromArgb(98, 103, 112);
            inputLevelStatusLabel.Location = new Point(91, 31);
            inputLevelStatusLabel.Name = "inputLevelStatusLabel";
            inputLevelStatusLabel.Size = new Size(217, 27);
            inputLevelStatusLabel.TabIndex = 1;
            inputLevelStatusLabel.Text = "No input selected";
            // 
            // inputLevelMeter
            // 
            inputLevelMeter.BackColor = Color.FromArgb(28, 30, 32);
            inputLevelMeter.ForeColor = SystemColors.ControlDarkDark;
            inputLevelMeter.Level = 0F;
            inputLevelMeter.Location = new Point(16, 31);
            inputLevelMeter.Name = "inputLevelMeter";
            inputLevelMeter.Size = new Size(74, 226);
            inputLevelMeter.TabIndex = 0;
            // 
            // virtualMicGroupBox
            // 
            virtualMicGroupBox.Controls.Add(virtualMicStateValueLabel);
            virtualMicGroupBox.Controls.Add(virtualMicStateLabel);
            virtualMicGroupBox.Controls.Add(virtualMicNameValueLabel);
            virtualMicGroupBox.Controls.Add(virtualMicNameLabel);
            virtualMicGroupBox.Location = new Point(16, 149);
            virtualMicGroupBox.Margin = new Padding(3, 4, 3, 4);
            virtualMicGroupBox.Name = "virtualMicGroupBox";
            virtualMicGroupBox.Padding = new Padding(3, 4, 3, 4);
            virtualMicGroupBox.Size = new Size(334, 139);
            virtualMicGroupBox.TabIndex = 3;
            virtualMicGroupBox.TabStop = false;
            virtualMicGroupBox.Text = "Virtual microphone";
            // 
            // virtualMicStateValueLabel
            // 
            virtualMicStateValueLabel.AutoEllipsis = true;
            virtualMicStateValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            virtualMicStateValueLabel.Location = new Point(99, 79);
            virtualMicStateValueLabel.Name = "virtualMicStateValueLabel";
            virtualMicStateValueLabel.Size = new Size(217, 27);
            virtualMicStateValueLabel.TabIndex = 3;
            virtualMicStateValueLabel.Text = "Scanning";
            // 
            // virtualMicStateLabel
            // 
            virtualMicStateLabel.AutoSize = true;
            virtualMicStateLabel.Location = new Point(16, 79);
            virtualMicStateLabel.Name = "virtualMicStateLabel";
            virtualMicStateLabel.Size = new Size(49, 20);
            virtualMicStateLabel.TabIndex = 2;
            virtualMicStateLabel.Text = "Status";
            // 
            // virtualMicNameValueLabel
            // 
            virtualMicNameValueLabel.AutoEllipsis = true;
            virtualMicNameValueLabel.Location = new Point(99, 40);
            virtualMicNameValueLabel.Name = "virtualMicNameValueLabel";
            virtualMicNameValueLabel.Size = new Size(217, 27);
            virtualMicNameValueLabel.TabIndex = 1;
            virtualMicNameValueLabel.Text = "Snj Voice Changer";
            // 
            // virtualMicNameLabel
            // 
            virtualMicNameLabel.AutoSize = true;
            virtualMicNameLabel.Location = new Point(16, 40);
            virtualMicNameLabel.Name = "virtualMicNameLabel";
            virtualMicNameLabel.Size = new Size(49, 20);
            virtualMicNameLabel.TabIndex = 0;
            virtualMicNameLabel.Text = "Name";
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(358, 75);
            refreshButton.Margin = new Padding(3, 4, 3, 4);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(91, 28);
            refreshButton.TabIndex = 2;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            // 
            // inputDeviceComboBox
            // 
            inputDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            inputDeviceComboBox.FormattingEnabled = true;
            inputDeviceComboBox.Location = new Point(16, 75);
            inputDeviceComboBox.Margin = new Padding(3, 4, 3, 4);
            inputDeviceComboBox.Name = "inputDeviceComboBox";
            inputDeviceComboBox.Size = new Size(334, 28);
            inputDeviceComboBox.TabIndex = 1;
            // 
            // inputDeviceLabel
            // 
            inputDeviceLabel.AutoSize = true;
            inputDeviceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            inputDeviceLabel.Location = new Point(16, 37);
            inputDeviceLabel.Name = "inputDeviceLabel";
            inputDeviceLabel.Size = new Size(93, 20);
            inputDeviceLabel.TabIndex = 0;
            inputDeviceLabel.Text = "InputDevice";
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(mainPlaceholderLabel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ForeColor = SystemColors.ControlDark;
            mainPanel.Location = new Point(468, 0);
            mainPanel.Margin = new Padding(3, 4, 3, 4);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(23, 27, 23, 27);
            mainPanel.Size = new Size(855, 610);
            mainPanel.TabIndex = 1;
            // 
            // mainPlaceholderLabel
            // 
            mainPlaceholderLabel.AutoSize = true;
            mainPlaceholderLabel.ForeColor = Color.FromArgb(98, 103, 112);
            mainPlaceholderLabel.Location = new Point(23, 37);
            mainPlaceholderLabel.Name = "mainPlaceholderLabel";
            mainPlaceholderLabel.Size = new Size(218, 20);
            mainPlaceholderLabel.TabIndex = 0;
            mainPlaceholderLabel.Text = "VST chain will appear here later";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1323, 610);
            Controls.Add(mainPanel);
            Controls.Add(leftPanel);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(729, 464);
            Name = "MainForm";
            Text = "Snj Voice Changer v0";
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            inputLevelGroupBox.ResumeLayout(false);
            virtualMicGroupBox.ResumeLayout(false);
            virtualMicGroupBox.PerformLayout();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel leftPanel;
        private Label inputDeviceLabel;
        private ComboBox inputDeviceComboBox;
        private Button refreshButton;
        private GroupBox virtualMicGroupBox;
        private Label virtualMicNameLabel;
        private Label virtualMicNameValueLabel;
        private Label virtualMicStateLabel;
        private Label virtualMicStateValueLabel;
        private GroupBox inputLevelGroupBox;
        private Label inputLevelStatusLabel;
        private AudioLevelMeterControl inputLevelMeter;
        private Panel mainPanel;
        private Label mainPlaceholderLabel;
    }
}
