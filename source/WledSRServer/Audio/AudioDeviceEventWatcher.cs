using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace WledSRServer.Audio
{
    // https://stackoverflow.com/questions/6163119/handling-changed-audio-device-event-in-c-sharp

    internal class AudioDeviceEventWatcher : IMMNotificationClient, IDisposable
    {
        private MMDeviceEnumerator _deviceEnumerator = new();
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
            _deviceEnumerator.RegisterEndpointNotificationCallback(this);
        }

        ~AudioDeviceEventWatcher()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _deviceEnumerator.UnregisterEndpointNotificationCallback(this);
            _isDisposed = true;
        }

        void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
           => DeviceStateChanged?.Invoke(deviceId, newState);

        void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
            => DeviceAdded?.Invoke(pwstrDeviceId);

        void IMMNotificationClient.OnDeviceRemoved(string deviceId)
            => DeviceRemoved?.Invoke(deviceId);

        void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            => DefaultDeviceChanged?.Invoke(flow, role, defaultDeviceId);

        void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            => PropertyValueChanged?.Invoke(pwstrDeviceId, key);
    }
}
