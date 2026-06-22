using System;

namespace Haptic.Core
{
    public enum HapticIntensity { Low, Medium, High }
    public enum VisualAssist { Off, Minimal, Full }

    [Serializable]
    public sealed class GameSettings
    {
        public HapticIntensity hapticIntensity = HapticIntensity.Medium;
        public VisualAssist visualAssist = VisualAssist.Minimal;
        public bool soundEnabled = true;
        public bool leftHanded;
    }
}

