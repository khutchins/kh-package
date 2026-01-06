using UnityEngine;

namespace KH.Audio {
    public struct PlaybackConfig {
        public float VolumeMod;
        public float PitchMod;
        public float SpatialBlend;
        public Transform FollowTarget;
        public Vector3? Position;

        public static PlaybackConfig Default => new PlaybackConfig {
            VolumeMod = 1f,
            PitchMod = 1f,
            SpatialBlend = 0f
        };
    }
}