using NAudio.CoreAudioApi;

namespace WledSRServer.Audio
{
    internal class AudioDeviceEventWatcher : IDisposable
    {
        private readonly MMDeviceEnumerator _deviceEnumerator = new();
        private readonly MMDeviceNotificationClient _notificationClient;
        private bool _isDisposed = false;

        public delegate void DeviceStateChangedHandler(string deviceId, DeviceState newState);
        public delegate void DeviceAddedHandler(string pwstrDeviceId);
        public delegate void DeviceRemovedHandler(string deviceId);
        public delegate void DefaultDeviceChangedHandler(DataFlow flow, Role role, string defaultDeviceId);
        public delegate void PropertyValueChangedHandler(string pwstrDeviceId, PropertyKey key);

        public event DeviceStateChangedHandler? DeviceStateChanged;
        public event DeviceAddedHandler? DeviceAdded;
        public event DeviceRemovedHandler? DeviceRemoved;
        public event DefaultDeviceChangedHandler? DefaultDeviceChanged;
        public event PropertyValueChangedHandler? PropertyValueChanged;

        public AudioDeviceEventWatcher()
        {
            // Events are raised on the Core Audio notification thread (the watcher lives on a background thread without a SynchronizationContext)
            _notificationClient = _deviceEnumerator.CreateNotificationClient(useSynchronizationContext: false);
            _notificationClient.DeviceStateChanged += (_, e) => DeviceStateChanged?.Invoke(e.DeviceId, e.NewState);
            _notificationClient.DeviceAdded += (_, e) => DeviceAdded?.Invoke(e.DeviceId);
            _notificationClient.DeviceRemoved += (_, e) => DeviceRemoved?.Invoke(e.DeviceId);
            _notificationClient.DefaultDeviceChanged += (_, e) => DefaultDeviceChanged?.Invoke(e.Flow, e.Role, e.DeviceId);
            _notificationClient.PropertyValueChanged += (_, e) => PropertyValueChanged?.Invoke(e.DeviceId, e.PropertyKey);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _notificationClient.Dispose();
            _deviceEnumerator.Dispose();
            _isDisposed = true;
        }
    }
}
