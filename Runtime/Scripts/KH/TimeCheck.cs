using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public class TimeCheck {
        public enum Type {
            Time,
            UnscaledTime
        }

        private Type _type;
        private System.Func<float> _timeGetter;
        private float _time;

        public TimeCheck(Type type = Type.Time) : this(type == Type.Time ? EZTween.TimeGetter.Scaled : EZTween.TimeGetter.Unscaled) {
        }

        public TimeCheck(System.Func<float> timeGetter) {
            _timeGetter = timeGetter;
            UpdateTime();
        }

        public void UpdateTime() {
            _time = GetTime();
        }

        private float GetTime() {
            return _type == Type.Time ? Time.time : Time.unscaledTime;
        }

        public bool HasTimeElapsed(float newDuration) {
            return _timeGetter.Invoke() > _time + newDuration;
        }
    }
}