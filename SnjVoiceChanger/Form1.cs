namespace SnjVoiceChanger
{
    public partial class MainForm : Form
    {
        private readonly AudioInputDeviceScanner _audioInputDeviceScanner = new();
        private readonly VirtualMicrophoneService _virtualMicrophoneService = new();
        private readonly System.Windows.Forms.Timer _inputLevelTimer = new();
        private AudioInputLevelMonitor? _inputLevelMonitor;

        public MainForm()
        {
            InitializeComponent();
            inputDeviceComboBox.SelectedIndexChanged += InputDeviceComboBox_SelectedIndexChanged;
            _inputLevelTimer.Interval = 33;
            _inputLevelTimer.Tick += InputLevelTimer_Tick;
            RefreshAudioInputs();
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            RefreshAudioInputs();
        }

        private void InputDeviceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            StartInputLevelMonitor(inputDeviceComboBox.SelectedItem as AudioInputDevice);
        }

        private void InputLevelTimer_Tick(object? sender, EventArgs e)
        {
            inputLevelMeter.Level = _inputLevelMonitor?.GetPeakLevel() ?? 0;
        }

        private void RefreshAudioInputs()
        {
            var selectedDeviceId = (inputDeviceComboBox.SelectedItem as AudioInputDevice)?.Id;
            var inputDevices = _audioInputDeviceScanner.GetInputDevices();

            inputDeviceComboBox.BeginUpdate();
            inputDeviceComboBox.DataSource = null;
            inputDeviceComboBox.Items.Clear();

            if (inputDevices.Count == 0)
            {
                inputDeviceComboBox.Items.Add("No input devices found");
                inputDeviceComboBox.SelectedIndex = 0;
                inputDeviceComboBox.Enabled = false;
            }
            else
            {
                var inputDeviceList = inputDevices.ToList();
                inputDeviceComboBox.DataSource = inputDeviceList;
                inputDeviceComboBox.DropDownWidth = GetPreferredDropDownWidth(inputDeviceList);
                var selectedIndex = Math.Max(0, inputDeviceList.FindIndex(device => device.Id == selectedDeviceId));
                inputDeviceComboBox.SelectedIndex = selectedIndex;
                inputDeviceComboBox.Enabled = true;
            }

            inputDeviceComboBox.EndUpdate();

            var virtualMicStatus = _virtualMicrophoneService.GetStatus(inputDevices);
            virtualMicNameValueLabel.Text = virtualMicStatus.DeviceName;
            virtualMicStateValueLabel.Text = virtualMicStatus.Message;
            virtualMicStateValueLabel.ForeColor = virtualMicStatus.IsAvailable
                ? Color.FromArgb(34, 139, 34)
                : Color.FromArgb(178, 34, 34);

            StartInputLevelMonitor(inputDeviceComboBox.SelectedItem as AudioInputDevice);
        }

        private int GetPreferredDropDownWidth(IReadOnlyList<AudioInputDevice> inputDevices)
        {
            if (inputDevices.Count == 0)
            {
                return inputDeviceComboBox.Width;
            }

            var maxTextWidth = inputDevices
                .Select(device => TextRenderer.MeasureText(device.Name, inputDeviceComboBox.Font).Width)
                .Max();

            return Math.Max(inputDeviceComboBox.Width, maxTextWidth + SystemInformation.VerticalScrollBarWidth + 24);
        }

        private void StartInputLevelMonitor(AudioInputDevice? inputDevice)
        {
            _inputLevelTimer.Stop();
            _inputLevelMonitor?.Dispose();
            _inputLevelMonitor = null;
            inputLevelMeter.Level = 0;

            if (inputDevice is null)
            {
                inputLevelStatusLabel.Text = "No input selected";
                return;
            }

            try
            {
                _inputLevelMonitor = new AudioInputLevelMonitor(inputDevice);
                inputLevelStatusLabel.Text = inputDevice.Name;
                _inputLevelTimer.Start();
            }
            catch
            {
                inputLevelStatusLabel.Text = "Meter unavailable";
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _inputLevelTimer.Stop();
            _inputLevelMonitor?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
