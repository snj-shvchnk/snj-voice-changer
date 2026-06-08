namespace SnjVoiceChanger
{
    public partial class MainForm : Form
    {
        private readonly AudioInputDeviceScanner _audioInputDeviceScanner = new();
        private readonly AudioOutputDeviceScanner _audioOutputDeviceScanner = new();
        private readonly VirtualCableService _virtualCableService = new();
        private readonly AudioRoutingService _audioRoutingService = new();
        private readonly VstPluginScanner _vstPluginScanner = new();
        private readonly System.Windows.Forms.Timer _levelTimer = new();
        private AudioInputLevelMonitor? _inputLevelMonitor;
        private readonly Dictionary<VstPluginChainItem, Form> _pluginEditorForms = new();
        private string _nativeVstStatus = "Native VST host unchecked";
        private string _lastAudioProcessingStatus = string.Empty;
        private bool _isRefreshingDevices;

        public MainForm()
        {
            InitializeComponent();
            inputDeviceComboBox.SelectedIndexChanged += InputDeviceComboBox_SelectedIndexChanged;
            outputDeviceComboBox.SelectedIndexChanged += OutputDeviceComboBox_SelectedIndexChanged;
            InitializeBufferSizeComboBox();
            _levelTimer.Interval = 33;
            _levelTimer.Tick += LevelTimer_Tick;
            _levelTimer.Start();
            pluginFolderTextBox.Text = GetDefaultPluginFolder();
            UpdateNativeVstStatus();
            RefreshPluginList();
            RefreshAudioDevices();
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            StopAudioRoute("Stopped: devices refreshed");
            RefreshAudioDevices();
        }

        private void InputDeviceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isRefreshingDevices)
            {
                return;
            }

            StopAudioRoute("Stopped: input changed");
            StartInputLevelMonitor(inputDeviceComboBox.SelectedItem as AudioInputDevice);
        }

        private void OutputDeviceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isRefreshingDevices)
            {
                return;
            }

            StopAudioRoute("Stopped: output changed");
            UpdateOutputSelectionStatus();
        }

        private void LevelTimer_Tick(object? sender, EventArgs e)
        {
            inputLevelMeter.Level = _audioRoutingService.IsRunning
                ? _audioRoutingService.GetInputPeakLevel()
                : _inputLevelMonitor?.GetPeakLevel() ?? 0;
            outputLevelMeter.Level = _audioRoutingService.GetOutputPeakLevel();
            UpdateRunningRouteStatus();
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            TryStartAudioRouteFromCurrentSelection();
        }

        private void StopButton_Click(object? sender, EventArgs e)
        {
            StopAudioRoute("Stopped");
        }

        private void BrowsePluginFolderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select VST3 plugin folder",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(pluginFolderTextBox.Text)
                    ? pluginFolderTextBox.Text
                    : GetDefaultPluginFolder(),
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            pluginFolderTextBox.Text = dialog.SelectedPath;
            RefreshPluginList();
        }

        private void scanPluginsButton_Click(object? sender, EventArgs e)
        {
            RefreshPluginList();
        }

        private void addPluginButton_Click(object? sender, EventArgs e)
        {
            if (foundPluginsListBox.SelectedItem is not VstPluginCandidate plugin)
            {
                pluginStatusLabel.Text = "Select a plugin first";
                return;
            }

            NativeVstHost? host = null;
            var wasRunning = _audioRoutingService.IsRunning;
            var stoppedForRestart = false;

            try
            {
                host = new NativeVstHost();
                host.LoadPlugin(plugin.Path);

                if (wasRunning)
                {
                    StopAudioRouteForInternalRestart();
                    stoppedForRestart = true;
                }

                pluginChainListBox.Items.Add(new VstPluginChainItem(plugin.Name, plugin.Path, host));
                host = null;

                if (wasRunning)
                {
                    TryRestartAudioRouteAfterChainChange($"Added: {plugin.Name}");
                }
                else
                {
                    pluginStatusLabel.Text = $"Added: {plugin.Name}";
                }
            }
            catch (NativeVstHostException ex)
            {
                RecoverFromFailedChainChange($"Add failed: {ex.Message}", stoppedForRestart);
            }
            catch (Exception ex)
            {
                RecoverFromFailedChainChange($"Add failed: {ex.Message}", stoppedForRestart);
            }
            finally
            {
                host?.Dispose();
            }

            UpdatePluginChainButtons();
        }

        private void removePluginButton_Click(object? sender, EventArgs e)
        {
            var selectedIndex = pluginChainListBox.SelectedIndex;
            if (selectedIndex < 0)
            {
                pluginStatusLabel.Text = "Select a chain plugin first";
                return;
            }

            var chainItem = pluginChainListBox.SelectedItem as VstPluginChainItem;
            var pluginName = chainItem?.Name ?? pluginChainListBox.SelectedItem?.ToString() ?? "plugin";
            var wasRunning = _audioRoutingService.IsRunning;

            if (wasRunning)
            {
                StopAudioRouteForInternalRestart();
            }

            if (chainItem is not null)
            {
                ClosePluginEditor(chainItem);
                chainItem.Dispose();
            }

            pluginChainListBox.Items.RemoveAt(selectedIndex);

            if (pluginChainListBox.Items.Count > 0)
            {
                pluginChainListBox.SelectedIndex = Math.Min(selectedIndex, pluginChainListBox.Items.Count - 1);
            }

            if (wasRunning)
            {
                TryRestartAudioRouteAfterChainChange($"Removed: {pluginName}");
            }
            else
            {
                pluginStatusLabel.Text = $"Removed: {pluginName}";
            }

            UpdatePluginChainButtons();
        }

        private void movePluginUpButton_Click(object? sender, EventArgs e)
        {
            MoveSelectedPlugin(-1);
        }

        private void movePluginDownButton_Click(object? sender, EventArgs e)
        {
            MoveSelectedPlugin(1);
        }

        private void openPluginEditorButton_Click(object? sender, EventArgs e)
        {
            if (pluginChainListBox.SelectedItem is not VstPluginChainItem plugin)
            {
                pluginStatusLabel.Text = "Select a chain plugin first";
                return;
            }

            OpenPluginEditor(plugin);
        }

        private void OpenPluginEditor(VstPluginChainItem plugin)
        {
            if (_pluginEditorForms.TryGetValue(plugin, out var existingEditor))
            {
                if (!existingEditor.IsDisposed)
                {
                    existingEditor.Activate();
                    return;
                }

                _pluginEditorForms.Remove(plugin);
            }

            var editorForm = new Form
            {
                Text = $"{plugin.Name} editor",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(900, 650),
                MinimumSize = new Size(480, 320),
                MinimizeBox = false,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MaximizeBox = false,
                ShowInTaskbar = false,
            };

            var editorOpened = false;
            _pluginEditorForms[plugin] = editorForm;

            editorForm.Shown += (_, _) =>
            {
                try
                {
                    plugin.OpenEditor(editorForm.Handle);
                    editorOpened = true;
                    pluginStatusLabel.Text = $"Editor opened: {plugin.Name}";
                }
                catch (NativeVstHostException ex)
                {
                    ShowEditorError(editorForm, ex.Message, plugin.Path);
                    pluginStatusLabel.Text = ex.Message;
                }
                catch (Exception ex)
                {
                    ShowEditorError(editorForm, ex.Message, plugin.Path);
                    pluginStatusLabel.Text = ex.Message;
                }
            };

            editorForm.FormClosed += (_, _) =>
            {
                _pluginEditorForms.Remove(plugin);

                if (!editorOpened)
                {
                    return;
                }

                try
                {
                    plugin.CloseEditor();
                }
                catch (NativeVstHostException ex)
                {
                    pluginStatusLabel.Text = ex.Message;
                }
                finally
                {
                    editorOpened = false;
                }
            };

            editorForm.Show(this);
        }

        private static void ShowEditorError(Form editorForm, string message, string pluginPath)
        {
            editorForm.Controls.Clear();
            editorForm.Controls.Add(new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                Text = $"{message}\r\n\r\n{pluginPath}",
            });
        }

        private void pluginChainListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdatePluginChainButtons();
        }

        private void pluginChainListBox_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            var clickedIndex = pluginChainListBox.IndexFromPoint(e.Location);
            if (clickedIndex < 0)
            {
                return;
            }

            pluginChainListBox.SelectedIndex = clickedIndex;
            openPluginEditorButton_Click(sender, EventArgs.Empty);
        }

        private void foundPluginsListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdatePluginChainButtons();
        }

        private void foundPluginsListBox_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            var clickedIndex = foundPluginsListBox.IndexFromPoint(e.Location);
            if (clickedIndex < 0)
            {
                return;
            }

            foundPluginsListBox.SelectedIndex = clickedIndex;
            addPluginButton_Click(sender, EventArgs.Empty);
        }

        private void RefreshAudioDevices()
        {
            var selectedInputDeviceId = (inputDeviceComboBox.SelectedItem as AudioInputDevice)?.Id;
            var selectedOutputDeviceId = (outputDeviceComboBox.SelectedItem as AudioOutputDevice)?.Id;
            var inputDevices = _audioInputDeviceScanner.GetInputDevices();
            var outputDevices = _audioOutputDeviceScanner.GetOutputDevices();

            _isRefreshingDevices = true;
            PopulateInputDevices(inputDevices, selectedInputDeviceId);
            PopulateOutputDevices(outputDevices, selectedOutputDeviceId);
            _isRefreshingDevices = false;

            UpdateVirtualCableStatus(inputDevices, outputDevices);
            StartInputLevelMonitor(inputDeviceComboBox.SelectedItem as AudioInputDevice);
            UpdateOutputSelectionStatus();
        }

        private void InitializeBufferSizeComboBox()
        {
            bufferSizeComboBox.Items.Clear();
            foreach (var size in new[] { 128, 256, 512, 1024, 2048, 4096 })
            {
                bufferSizeComboBox.Items.Add(size);
            }

            bufferSizeComboBox.SelectedItem = 512;
        }

        private int GetSelectedBufferSize()
        {
            return bufferSizeComboBox.SelectedItem is int size
                ? size
                : 512;
        }

        private void PopulateInputDevices(IReadOnlyList<AudioInputDevice> inputDevices, string? selectedDeviceId)
        {
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
                inputDeviceComboBox.DropDownWidth = GetPreferredDropDownWidth(inputDeviceComboBox, inputDeviceList);
                var selectedIndex = Math.Max(0, inputDeviceList.FindIndex(device => device.Id == selectedDeviceId));
                inputDeviceComboBox.SelectedIndex = selectedIndex;
                inputDeviceComboBox.Enabled = true;
            }

            inputDeviceComboBox.EndUpdate();
        }

        private void PopulateOutputDevices(IReadOnlyList<AudioOutputDevice> outputDevices, string? selectedDeviceId)
        {
            outputDeviceComboBox.BeginUpdate();
            outputDeviceComboBox.DataSource = null;
            outputDeviceComboBox.Items.Clear();

            if (outputDevices.Count == 0)
            {
                outputDeviceComboBox.Items.Add("No output devices found");
                outputDeviceComboBox.SelectedIndex = 0;
                outputDeviceComboBox.Enabled = false;
            }
            else
            {
                var outputDeviceList = outputDevices.ToList();
                outputDeviceComboBox.DataSource = outputDeviceList;
                outputDeviceComboBox.DropDownWidth = GetPreferredDropDownWidth(outputDeviceComboBox, outputDeviceList);
                var selectedIndex = outputDeviceList.FindIndex(device => device.Id == selectedDeviceId);

                if (selectedIndex < 0)
                {
                    var preferredOutputDevice = _virtualCableService.FindPreferredOutputDevice(outputDeviceList);
                    selectedIndex = preferredOutputDevice is null
                        ? 0
                        : outputDeviceList.FindIndex(device => device.Id == preferredOutputDevice.Id);
                }

                outputDeviceComboBox.SelectedIndex = Math.Max(0, selectedIndex);
                outputDeviceComboBox.Enabled = true;
            }

            outputDeviceComboBox.EndUpdate();
        }

        private void UpdateVirtualCableStatus(
            IReadOnlyList<AudioInputDevice> inputDevices,
            IReadOnlyList<AudioOutputDevice> outputDevices)
        {
            var virtualCableStatus = _virtualCableService.GetStatus(inputDevices, outputDevices);
            cableOutputValueLabel.Text = virtualCableStatus.OutputDeviceName;
            cableInputValueLabel.Text = virtualCableStatus.InputDeviceName;
            cableStateValueLabel.Text = virtualCableStatus.Message;
            cableStateValueLabel.ForeColor = virtualCableStatus.IsReady
                ? Color.FromArgb(34, 139, 34)
                : Color.FromArgb(178, 34, 34);
        }

        private static int GetPreferredDropDownWidth<T>(ComboBox comboBox, IReadOnlyList<T> devices)
        {
            if (devices.Count == 0)
            {
                return comboBox.Width;
            }

            var maxTextWidth = devices
                .Select(device => TextRenderer.MeasureText(device?.ToString() ?? string.Empty, comboBox.Font).Width)
                .Max();

            return Math.Max(comboBox.Width, maxTextWidth + SystemInformation.VerticalScrollBarWidth + 24);
        }

        private void StartInputLevelMonitor(AudioInputDevice? inputDevice)
        {
            StopInputLevelMonitor();

            if (inputDevice is null)
            {
                inputLevelStatusLabel.Text = "No input selected";
                return;
            }

            try
            {
                _inputLevelMonitor = new AudioInputLevelMonitor(inputDevice);
                inputLevelStatusLabel.Text = inputDevice.Name;
            }
            catch
            {
                inputLevelStatusLabel.Text = "Meter unavailable";
            }
        }

        private void StopInputLevelMonitor()
        {
            _inputLevelMonitor?.Dispose();
            _inputLevelMonitor = null;
            inputLevelMeter.Level = 0;
        }

        private void UpdateOutputSelectionStatus()
        {
            var outputDevice = outputDeviceComboBox.SelectedItem as AudioOutputDevice;
            outputLevelStatusLabel.Text = outputDevice?.Name ?? "No output selected";
        }

        private void StopAudioRoute(string status)
        {
            _audioRoutingService.Stop();
            outputLevelMeter.Level = 0;
            startButton.Enabled = true;
            stopButton.Enabled = false;
            _lastAudioProcessingStatus = string.Empty;
            routingStatusValueLabel.Text = status;
            routingStatusValueLabel.ForeColor = Color.FromArgb(98, 103, 112);
            UpdateOutputSelectionStatus();
            StartInputLevelMonitor(inputDeviceComboBox.SelectedItem as AudioInputDevice);
        }

        private void StopAudioRouteForInternalRestart()
        {
            _audioRoutingService.Stop();
            outputLevelMeter.Level = 0;
            _lastAudioProcessingStatus = string.Empty;
        }

        private bool TryRestartAudioRouteAfterChainChange(string actionStatus)
        {
            var restarted = TryStartAudioRouteFromCurrentSelection();
            pluginStatusLabel.Text = restarted
                ? actionStatus
                : $"Restart failed after chain change: {pluginStatusLabel.Text}";

            return restarted;
        }

        private void RecoverFromFailedChainChange(string failureStatus, bool routeWasStoppedForRestart)
        {
            if (!routeWasStoppedForRestart)
            {
                pluginStatusLabel.Text = failureStatus;
                return;
            }

            var restarted = TryStartAudioRouteFromCurrentSelection();
            pluginStatusLabel.Text = restarted
                ? failureStatus
                : $"{failureStatus}. Route restart failed: {pluginStatusLabel.Text}";
        }

        private bool TryStartAudioRouteFromCurrentSelection()
        {
            var inputDevice = inputDeviceComboBox.SelectedItem as AudioInputDevice;
            var outputDevice = outputDeviceComboBox.SelectedItem as AudioOutputDevice;

            if (inputDevice is null || outputDevice is null)
            {
                routingStatusValueLabel.Text = "Select input and output";
                routingStatusValueLabel.ForeColor = Color.FromArgb(178, 34, 34);
                startButton.Enabled = true;
                stopButton.Enabled = false;
                return false;
            }

            try
            {
                StopInputLevelMonitor();
                _audioRoutingService.Start(
                    inputDevice,
                    outputDevice,
                    GetPluginChainSnapshot(),
                    GetSelectedBufferSize());
                startButton.Enabled = false;
                stopButton.Enabled = true;
                UpdateRunningRouteStatus(force: true);
                outputLevelStatusLabel.Text = outputDevice.Name;
                return true;
            }
            catch (Exception ex)
            {
                StopAudioRoute("Error");
                routingStatusValueLabel.Text = ex.Message;
                routingStatusValueLabel.ForeColor = Color.FromArgb(178, 34, 34);
                pluginStatusLabel.Text = ex.Message;
                return false;
            }
        }

        private void UpdateRunningRouteStatus(bool force = false)
        {
            if (!_audioRoutingService.IsRunning)
            {
                return;
            }

            var processingStatus = _audioRoutingService.ProcessingStatus;
            if (!force && processingStatus == _lastAudioProcessingStatus)
            {
                return;
            }

            _lastAudioProcessingStatus = processingStatus;
            routingStatusValueLabel.Text = $"Running - {processingStatus}";
            routingStatusValueLabel.ForeColor = _audioRoutingService.IsVstProcessingActive
                ? Color.FromArgb(34, 139, 34)
                : Color.FromArgb(184, 117, 28);
            pluginStatusLabel.Text = processingStatus;
        }

        private void RefreshPluginList()
        {
            var plugins = _vstPluginScanner.Scan(pluginFolderTextBox.Text);

            foundPluginsListBox.BeginUpdate();
            foundPluginsListBox.Items.Clear();

            foreach (var plugin in plugins)
            {
                foundPluginsListBox.Items.Add(plugin);
            }

            foundPluginsListBox.EndUpdate();
            pluginStatusLabel.Text = GetPluginStatusText(plugins.Count);
            UpdatePluginChainButtons();
        }

        private void UpdateNativeVstStatus()
        {
            try
            {
                _nativeVstStatus = $"Native VST API v{NativeVstHost.ApiVersion}";
            }
            catch (NativeVstHostException ex)
            {
                _nativeVstStatus = ex.Message;
            }
            catch (Exception ex)
            {
                _nativeVstStatus = $"Native VST host unavailable: {ex.Message}";
            }
        }

        private string GetPluginStatusText(int pluginCount)
        {
            var pluginScanStatus = pluginCount == 0
                ? "No VST3 plugins found"
                : $"Found {pluginCount} VST3 plugin(s)";

            return $"{_nativeVstStatus}. {pluginScanStatus}";
        }

        private static string GetDefaultPluginFolder()
        {
            var outputPluginFolder = Path.Combine(AppContext.BaseDirectory, "common", "VST");
            if (Directory.Exists(outputPluginFolder))
            {
                return outputPluginFolder;
            }

            var repoPluginFolder = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "common",
                "VST"));

            return Directory.Exists(repoPluginFolder)
                ? repoPluginFolder
                : outputPluginFolder;
        }

        private void UpdatePluginChainButtons()
        {
            var selectedIndex = pluginChainListBox.SelectedIndex;
            var hasChainSelection = pluginChainListBox.SelectedItem is VstPluginChainItem;
            movePluginUpButton.Enabled = hasChainSelection && selectedIndex > 0;
            movePluginDownButton.Enabled = hasChainSelection && selectedIndex < pluginChainListBox.Items.Count - 1;
            removePluginButton.Enabled = hasChainSelection;
            openPluginEditorButton.Enabled = hasChainSelection;
            addPluginButton.Enabled = foundPluginsListBox.SelectedItem is VstPluginCandidate;
        }

        private void MoveSelectedPlugin(int direction)
        {
            var selectedIndex = pluginChainListBox.SelectedIndex;
            var targetIndex = selectedIndex + direction;

            if (selectedIndex < 0 ||
                targetIndex < 0 ||
                targetIndex >= pluginChainListBox.Items.Count ||
                pluginChainListBox.SelectedItem is not VstPluginChainItem chainItem)
            {
                UpdatePluginChainButtons();
                return;
            }

            var wasRunning = _audioRoutingService.IsRunning;
            if (wasRunning)
            {
                StopAudioRouteForInternalRestart();
            }

            pluginChainListBox.Items.RemoveAt(selectedIndex);
            pluginChainListBox.Items.Insert(targetIndex, chainItem);
            pluginChainListBox.SelectedIndex = targetIndex;

            if (wasRunning)
            {
                var directionName = direction < 0 ? "up" : "down";
                TryRestartAudioRouteAfterChainChange($"Moved {directionName}: {chainItem.Name}");
            }
            else
            {
                var directionName = direction < 0 ? "up" : "down";
                pluginStatusLabel.Text = $"Moved {directionName}: {chainItem.Name}";
            }

            UpdatePluginChainButtons();
        }

        private IReadOnlyList<VstPluginChainItem> GetPluginChainSnapshot()
        {
            var pluginChain = new List<VstPluginChainItem>();

            foreach (var item in pluginChainListBox.Items)
            {
                if (item is VstPluginChainItem chainItem)
                {
                    pluginChain.Add(chainItem);
                }
            }

            return pluginChain;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _levelTimer.Stop();
            _audioRoutingService.Dispose();
            _inputLevelMonitor?.Dispose();
            CloseAllPluginEditors();
            DisposePluginChain();
            base.OnFormClosed(e);
        }

        private void ClosePluginEditor(VstPluginChainItem plugin)
        {
            if (!_pluginEditorForms.TryGetValue(plugin, out var editorForm))
            {
                return;
            }

            if (editorForm.IsDisposed)
            {
                _pluginEditorForms.Remove(plugin);
                return;
            }

            editorForm.Close();
        }

        private void CloseAllPluginEditors()
        {
            foreach (var editorForm in _pluginEditorForms.Values.ToArray())
            {
                if (!editorForm.IsDisposed)
                {
                    editorForm.Close();
                }
            }

            _pluginEditorForms.Clear();
        }

        private void DisposePluginChain()
        {
            foreach (var item in pluginChainListBox.Items)
            {
                if (item is VstPluginChainItem chainItem)
                {
                    chainItem.Dispose();
                }
            }

            pluginChainListBox.Items.Clear();
        }
    }
}
