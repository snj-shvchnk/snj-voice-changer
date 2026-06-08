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
            outputLevelGroupBox = new GroupBox();
            outputLevelStatusLabel = new Label();
            outputLevelMeter = new AudioLevelMeterControl();
            inputLevelGroupBox = new GroupBox();
            inputLevelStatusLabel = new Label();
            inputLevelMeter = new AudioLevelMeterControl();
            virtualCableGroupBox = new GroupBox();
            routingStatusValueLabel = new Label();
            routingStatusLabel = new Label();
            cableStateValueLabel = new Label();
            cableStateLabel = new Label();
            cableInputValueLabel = new Label();
            cableInputLabel = new Label();
            cableOutputValueLabel = new Label();
            cableOutputLabel = new Label();
            stopButton = new Button();
            startButton = new Button();
            refreshButton = new Button();
            outputDeviceComboBox = new ComboBox();
            outputDeviceLabel = new Label();
            inputDeviceComboBox = new ComboBox();
            inputDeviceLabel = new Label();
            mainPanel = new Panel();
            pluginChainGroupBox = new GroupBox();
            pluginChainListBox = new ListBox();
            openPluginEditorButton = new Button();
            removePluginButton = new Button();
            addPluginButton = new Button();
            foundPluginsGroupBox = new GroupBox();
            foundPluginsListBox = new ListBox();
            pluginStatusLabel = new Label();
            scanPluginsButton = new Button();
            browsePluginFolderButton = new Button();
            pluginFolderTextBox = new TextBox();
            pluginFolderLabel = new Label();
            leftPanel.SuspendLayout();
            outputLevelGroupBox.SuspendLayout();
            inputLevelGroupBox.SuspendLayout();
            virtualCableGroupBox.SuspendLayout();
            mainPanel.SuspendLayout();
            pluginChainGroupBox.SuspendLayout();
            foundPluginsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.FromArgb(246, 247, 249);
            leftPanel.Controls.Add(outputLevelGroupBox);
            leftPanel.Controls.Add(inputLevelGroupBox);
            leftPanel.Controls.Add(virtualCableGroupBox);
            leftPanel.Controls.Add(stopButton);
            leftPanel.Controls.Add(startButton);
            leftPanel.Controls.Add(refreshButton);
            leftPanel.Controls.Add(outputDeviceComboBox);
            leftPanel.Controls.Add(outputDeviceLabel);
            leftPanel.Controls.Add(inputDeviceComboBox);
            leftPanel.Controls.Add(inputDeviceLabel);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Margin = new Padding(3, 4, 3, 4);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(16, 19, 16, 19);
            leftPanel.Size = new Size(366, 610);
            leftPanel.TabIndex = 0;
            // 
            // outputLevelGroupBox
            // 
            outputLevelGroupBox.Controls.Add(outputLevelStatusLabel);
            outputLevelGroupBox.Controls.Add(outputLevelMeter);
            outputLevelGroupBox.Location = new Point(16, 459);
            outputLevelGroupBox.Margin = new Padding(3, 4, 3, 4);
            outputLevelGroupBox.Name = "outputLevelGroupBox";
            outputLevelGroupBox.Padding = new Padding(3, 4, 3, 4);
            outputLevelGroupBox.Size = new Size(334, 119);
            outputLevelGroupBox.TabIndex = 9;
            outputLevelGroupBox.TabStop = false;
            outputLevelGroupBox.Text = "Output signal";
            // 
            // outputLevelStatusLabel
            // 
            outputLevelStatusLabel.AutoEllipsis = true;
            outputLevelStatusLabel.ForeColor = Color.FromArgb(98, 103, 112);
            outputLevelStatusLabel.Location = new Point(91, 31);
            outputLevelStatusLabel.Name = "outputLevelStatusLabel";
            outputLevelStatusLabel.Size = new Size(217, 27);
            outputLevelStatusLabel.TabIndex = 1;
            outputLevelStatusLabel.Text = "No output selected";
            // 
            // outputLevelMeter
            // 
            outputLevelMeter.BackColor = Color.FromArgb(28, 30, 32);
            outputLevelMeter.ForeColor = SystemColors.ControlDarkDark;
            outputLevelMeter.Location = new Point(16, 31);
            outputLevelMeter.Name = "outputLevelMeter";
            outputLevelMeter.Size = new Size(74, 76);
            outputLevelMeter.TabIndex = 0;
            // 
            // 
            // inputLevelGroupBox
            // 
            inputLevelGroupBox.Controls.Add(inputLevelStatusLabel);
            inputLevelGroupBox.Controls.Add(inputLevelMeter);
            inputLevelGroupBox.Location = new Point(16, 333);
            inputLevelGroupBox.Margin = new Padding(3, 4, 3, 4);
            inputLevelGroupBox.Name = "inputLevelGroupBox";
            inputLevelGroupBox.Padding = new Padding(3, 4, 3, 4);
            inputLevelGroupBox.Size = new Size(334, 119);
            inputLevelGroupBox.TabIndex = 8;
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
            inputLevelMeter.Location = new Point(16, 31);
            inputLevelMeter.Name = "inputLevelMeter";
            inputLevelMeter.Size = new Size(74, 76);
            inputLevelMeter.TabIndex = 0;
            // 
            // virtualCableGroupBox
            // 
            virtualCableGroupBox.Controls.Add(routingStatusValueLabel);
            virtualCableGroupBox.Controls.Add(routingStatusLabel);
            virtualCableGroupBox.Controls.Add(cableStateValueLabel);
            virtualCableGroupBox.Controls.Add(cableStateLabel);
            virtualCableGroupBox.Controls.Add(cableInputValueLabel);
            virtualCableGroupBox.Controls.Add(cableInputLabel);
            virtualCableGroupBox.Controls.Add(cableOutputValueLabel);
            virtualCableGroupBox.Controls.Add(cableOutputLabel);
            virtualCableGroupBox.Location = new Point(16, 185);
            virtualCableGroupBox.Margin = new Padding(3, 4, 3, 4);
            virtualCableGroupBox.Name = "virtualCableGroupBox";
            virtualCableGroupBox.Padding = new Padding(3, 4, 3, 4);
            virtualCableGroupBox.Size = new Size(334, 136);
            virtualCableGroupBox.TabIndex = 7;
            virtualCableGroupBox.TabStop = false;
            virtualCableGroupBox.Text = "Virtual cable";
            // 
            // routingStatusValueLabel
            // 
            routingStatusValueLabel.AutoEllipsis = true;
            routingStatusValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            routingStatusValueLabel.ForeColor = Color.FromArgb(98, 103, 112);
            routingStatusValueLabel.Location = new Point(99, 99);
            routingStatusValueLabel.Name = "routingStatusValueLabel";
            routingStatusValueLabel.Size = new Size(217, 23);
            routingStatusValueLabel.TabIndex = 7;
            routingStatusValueLabel.Text = "Stopped";
            // 
            // routingStatusLabel
            // 
            routingStatusLabel.AutoSize = true;
            routingStatusLabel.Location = new Point(16, 99);
            routingStatusLabel.Name = "routingStatusLabel";
            routingStatusLabel.Size = new Size(47, 20);
            routingStatusLabel.TabIndex = 6;
            routingStatusLabel.Text = "Route";
            // 
            // cableStateValueLabel
            // 
            cableStateValueLabel.AutoEllipsis = true;
            cableStateValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cableStateValueLabel.Location = new Point(99, 75);
            cableStateValueLabel.Name = "cableStateValueLabel";
            cableStateValueLabel.Size = new Size(217, 23);
            cableStateValueLabel.TabIndex = 5;
            cableStateValueLabel.Text = "Scanning";
            // 
            // cableStateLabel
            // 
            cableStateLabel.AutoSize = true;
            cableStateLabel.Location = new Point(16, 75);
            cableStateLabel.Name = "cableStateLabel";
            cableStateLabel.Size = new Size(49, 20);
            cableStateLabel.TabIndex = 4;
            cableStateLabel.Text = "Status";
            // 
            // cableInputValueLabel
            // 
            cableInputValueLabel.AutoEllipsis = true;
            cableInputValueLabel.Location = new Point(99, 51);
            cableInputValueLabel.Name = "cableInputValueLabel";
            cableInputValueLabel.Size = new Size(217, 23);
            cableInputValueLabel.TabIndex = 3;
            cableInputValueLabel.Text = "CABLE Output";
            // 
            // cableInputLabel
            // 
            cableInputLabel.AutoSize = true;
            cableInputLabel.Location = new Point(16, 51);
            cableInputLabel.Name = "cableInputLabel";
            cableInputLabel.Size = new Size(32, 20);
            cableInputLabel.TabIndex = 2;
            cableInputLabel.Text = "Mic";
            // 
            // cableOutputValueLabel
            // 
            cableOutputValueLabel.AutoEllipsis = true;
            cableOutputValueLabel.Location = new Point(99, 27);
            cableOutputValueLabel.Name = "cableOutputValueLabel";
            cableOutputValueLabel.Size = new Size(217, 23);
            cableOutputValueLabel.TabIndex = 1;
            cableOutputValueLabel.Text = "CABLE Input";
            // 
            // cableOutputLabel
            // 
            cableOutputLabel.AutoSize = true;
            cableOutputLabel.Location = new Point(16, 27);
            cableOutputLabel.Name = "cableOutputLabel";
            cableOutputLabel.Size = new Size(57, 20);
            cableOutputLabel.TabIndex = 0;
            cableOutputLabel.Text = "Output";
            // 
            // stopButton
            // 
            stopButton.Enabled = false;
            stopButton.Location = new Point(211, 145);
            stopButton.Margin = new Padding(3, 4, 3, 4);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(91, 28);
            stopButton.TabIndex = 6;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += StopButton_Click;
            // 
            // startButton
            // 
            startButton.Location = new Point(114, 145);
            startButton.Margin = new Padding(3, 4, 3, 4);
            startButton.Name = "startButton";
            startButton.Size = new Size(91, 28);
            startButton.TabIndex = 5;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += StartButton_Click;
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(16, 145);
            refreshButton.Margin = new Padding(3, 4, 3, 4);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(91, 28);
            refreshButton.TabIndex = 4;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            // 
            // outputDeviceComboBox
            // 
            outputDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            outputDeviceComboBox.FormattingEnabled = true;
            outputDeviceComboBox.Location = new Point(16, 104);
            outputDeviceComboBox.Margin = new Padding(3, 4, 3, 4);
            outputDeviceComboBox.Name = "outputDeviceComboBox";
            outputDeviceComboBox.Size = new Size(334, 28);
            outputDeviceComboBox.TabIndex = 3;
            // 
            // outputDeviceLabel
            // 
            outputDeviceLabel.AutoSize = true;
            outputDeviceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            outputDeviceLabel.Location = new Point(16, 79);
            outputDeviceLabel.Name = "outputDeviceLabel";
            outputDeviceLabel.Size = new Size(105, 20);
            outputDeviceLabel.TabIndex = 2;
            outputDeviceLabel.Text = "OutputDevice";
            // 
            // inputDeviceComboBox
            // 
            inputDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            inputDeviceComboBox.FormattingEnabled = true;
            inputDeviceComboBox.Location = new Point(16, 40);
            inputDeviceComboBox.Margin = new Padding(3, 4, 3, 4);
            inputDeviceComboBox.Name = "inputDeviceComboBox";
            inputDeviceComboBox.Size = new Size(334, 28);
            inputDeviceComboBox.TabIndex = 1;
            // 
            // inputDeviceLabel
            // 
            inputDeviceLabel.AutoSize = true;
            inputDeviceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            inputDeviceLabel.Location = new Point(16, 15);
            inputDeviceLabel.Name = "inputDeviceLabel";
            inputDeviceLabel.Size = new Size(93, 20);
            inputDeviceLabel.TabIndex = 0;
            inputDeviceLabel.Text = "InputDevice";
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.White;
            mainPanel.Controls.Add(pluginChainGroupBox);
            mainPanel.Controls.Add(addPluginButton);
            mainPanel.Controls.Add(foundPluginsGroupBox);
            mainPanel.Controls.Add(pluginStatusLabel);
            mainPanel.Controls.Add(scanPluginsButton);
            mainPanel.Controls.Add(browsePluginFolderButton);
            mainPanel.Controls.Add(pluginFolderTextBox);
            mainPanel.Controls.Add(pluginFolderLabel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ForeColor = SystemColors.ControlText;
            mainPanel.Location = new Point(366, 0);
            mainPanel.Margin = new Padding(3, 4, 3, 4);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(23, 27, 23, 27);
            mainPanel.Size = new Size(957, 610);
            mainPanel.TabIndex = 1;
            // 
            // pluginChainGroupBox
            // 
            pluginChainGroupBox.Controls.Add(pluginChainListBox);
            pluginChainGroupBox.Controls.Add(openPluginEditorButton);
            pluginChainGroupBox.Controls.Add(removePluginButton);
            pluginChainGroupBox.Location = new Point(23, 333);
            pluginChainGroupBox.Name = "pluginChainGroupBox";
            pluginChainGroupBox.Size = new Size(911, 250);
            pluginChainGroupBox.TabIndex = 7;
            pluginChainGroupBox.TabStop = false;
            pluginChainGroupBox.Text = "Plugin chain";
            // 
            // pluginChainListBox
            // 
            pluginChainListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pluginChainListBox.FormattingEnabled = true;
            pluginChainListBox.ItemHeight = 20;
            pluginChainListBox.Location = new Point(16, 31);
            pluginChainListBox.Name = "pluginChainListBox";
            pluginChainListBox.Size = new Size(770, 184);
            pluginChainListBox.TabIndex = 0;
            pluginChainListBox.SelectedIndexChanged += pluginChainListBox_SelectedIndexChanged;
            // 
            // openPluginEditorButton
            // 
            openPluginEditorButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            openPluginEditorButton.Enabled = false;
            openPluginEditorButton.Location = new Point(800, 68);
            openPluginEditorButton.Name = "openPluginEditorButton";
            openPluginEditorButton.Size = new Size(94, 31);
            openPluginEditorButton.TabIndex = 2;
            openPluginEditorButton.Text = "Editor";
            openPluginEditorButton.UseVisualStyleBackColor = true;
            openPluginEditorButton.Click += openPluginEditorButton_Click;
            // 
            // removePluginButton
            // 
            removePluginButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            removePluginButton.Enabled = false;
            removePluginButton.Location = new Point(800, 31);
            removePluginButton.Name = "removePluginButton";
            removePluginButton.Size = new Size(94, 31);
            removePluginButton.TabIndex = 1;
            removePluginButton.Text = "Remove";
            removePluginButton.UseVisualStyleBackColor = true;
            removePluginButton.Click += removePluginButton_Click;
            // 
            // addPluginButton
            // 
            addPluginButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addPluginButton.Enabled = false;
            addPluginButton.Location = new Point(801, 292);
            addPluginButton.Name = "addPluginButton";
            addPluginButton.Size = new Size(133, 31);
            addPluginButton.TabIndex = 6;
            addPluginButton.Text = "Add plugin";
            addPluginButton.UseVisualStyleBackColor = true;
            addPluginButton.Click += addPluginButton_Click;
            // 
            // foundPluginsGroupBox
            // 
            foundPluginsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            foundPluginsGroupBox.Controls.Add(foundPluginsListBox);
            foundPluginsGroupBox.Location = new Point(23, 104);
            foundPluginsGroupBox.Name = "foundPluginsGroupBox";
            foundPluginsGroupBox.Size = new Size(911, 178);
            foundPluginsGroupBox.TabIndex = 5;
            foundPluginsGroupBox.TabStop = false;
            foundPluginsGroupBox.Text = "Found VST3 plugins";
            // 
            // foundPluginsListBox
            // 
            foundPluginsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            foundPluginsListBox.FormattingEnabled = true;
            foundPluginsListBox.ItemHeight = 20;
            foundPluginsListBox.Location = new Point(16, 31);
            foundPluginsListBox.Name = "foundPluginsListBox";
            foundPluginsListBox.Size = new Size(878, 124);
            foundPluginsListBox.TabIndex = 0;
            foundPluginsListBox.SelectedIndexChanged += foundPluginsListBox_SelectedIndexChanged;
            // 
            // pluginStatusLabel
            // 
            pluginStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pluginStatusLabel.AutoEllipsis = true;
            pluginStatusLabel.ForeColor = Color.FromArgb(98, 103, 112);
            pluginStatusLabel.Location = new Point(23, 295);
            pluginStatusLabel.Name = "pluginStatusLabel";
            pluginStatusLabel.Size = new Size(772, 25);
            pluginStatusLabel.TabIndex = 4;
            pluginStatusLabel.Text = "No VST3 plugins found";
            // 
            // scanPluginsButton
            // 
            scanPluginsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scanPluginsButton.Location = new Point(840, 62);
            scanPluginsButton.Name = "scanPluginsButton";
            scanPluginsButton.Size = new Size(94, 31);
            scanPluginsButton.TabIndex = 3;
            scanPluginsButton.Text = "Scan";
            scanPluginsButton.UseVisualStyleBackColor = true;
            scanPluginsButton.Click += scanPluginsButton_Click;
            // 
            // browsePluginFolderButton
            // 
            browsePluginFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browsePluginFolderButton.Location = new Point(740, 62);
            browsePluginFolderButton.Name = "browsePluginFolderButton";
            browsePluginFolderButton.Size = new Size(94, 31);
            browsePluginFolderButton.TabIndex = 2;
            browsePluginFolderButton.Text = "Browse";
            browsePluginFolderButton.UseVisualStyleBackColor = true;
            browsePluginFolderButton.Click += BrowsePluginFolderButton_Click;
            // 
            // pluginFolderTextBox
            // 
            pluginFolderTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pluginFolderTextBox.Location = new Point(23, 64);
            pluginFolderTextBox.Name = "pluginFolderTextBox";
            pluginFolderTextBox.Size = new Size(711, 27);
            pluginFolderTextBox.TabIndex = 1;
            // 
            // pluginFolderLabel
            // 
            pluginFolderLabel.AutoSize = true;
            pluginFolderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            pluginFolderLabel.ForeColor = SystemColors.ControlText;
            pluginFolderLabel.Location = new Point(23, 37);
            pluginFolderLabel.Name = "pluginFolderLabel";
            pluginFolderLabel.Size = new Size(97, 20);
            pluginFolderLabel.TabIndex = 0;
            pluginFolderLabel.Text = "Plugin folder";
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
            outputLevelGroupBox.ResumeLayout(false);
            inputLevelGroupBox.ResumeLayout(false);
            virtualCableGroupBox.ResumeLayout(false);
            virtualCableGroupBox.PerformLayout();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            pluginChainGroupBox.ResumeLayout(false);
            foundPluginsGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel leftPanel;
        private Label inputDeviceLabel;
        private ComboBox inputDeviceComboBox;
        private Label outputDeviceLabel;
        private ComboBox outputDeviceComboBox;
        private Button refreshButton;
        private Button startButton;
        private Button stopButton;
        private GroupBox virtualCableGroupBox;
        private Label cableOutputLabel;
        private Label cableOutputValueLabel;
        private Label cableInputLabel;
        private Label cableInputValueLabel;
        private Label cableStateLabel;
        private Label cableStateValueLabel;
        private Label routingStatusLabel;
        private Label routingStatusValueLabel;
        private GroupBox inputLevelGroupBox;
        private Label inputLevelStatusLabel;
        private AudioLevelMeterControl inputLevelMeter;
        private GroupBox outputLevelGroupBox;
        private Label outputLevelStatusLabel;
        private AudioLevelMeterControl outputLevelMeter;
        private Panel mainPanel;
        private Label pluginFolderLabel;
        private TextBox pluginFolderTextBox;
        private Button browsePluginFolderButton;
        private Button scanPluginsButton;
        private Label pluginStatusLabel;
        private GroupBox foundPluginsGroupBox;
        private ListBox foundPluginsListBox;
        private Button addPluginButton;
        private GroupBox pluginChainGroupBox;
        private ListBox pluginChainListBox;
        private Button removePluginButton;
        private Button openPluginEditorButton;
    }
}
