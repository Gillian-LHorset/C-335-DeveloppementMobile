namespace FlashCard.Services {
    internal class ShakeDetectionService {
        private bool _isMonitoring;

        public event EventHandler ShakeDetected;

        public bool IsAvailable => Accelerometer.Default.IsSupported;

        public bool IsMonitoring => _isMonitoring;

        public void StartMonitoring() {
            if (!IsAvailable || _isMonitoring)
                return;

            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.Game);
            _isMonitoring = true;
        }

        public void StopMonitoring() {
            if (!IsAvailable || !_isMonitoring)
                return;

            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
            _isMonitoring = false;
        }

        private void OnShakeDetected(object sender, EventArgs e) {
            ShakeDetected?.Invoke(this, EventArgs.Empty);
        }
    }
}
