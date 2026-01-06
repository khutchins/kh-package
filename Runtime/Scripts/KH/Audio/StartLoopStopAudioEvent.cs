using UnityEngine;

namespace KH.Audio {
    [CreateAssetMenu(menuName = "KH/Audio Events/StartLoopStop")]
    public class StartLoopStopAudioEvent : AudioEvent {
        public AudioEvent StartSound;
        public AudioEvent LoopSound;
        public AudioEvent StopSound;

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool managed) {
            return new StartLoopStopPlaybackHandle(this, source, runner, managed);
        }
    }

}