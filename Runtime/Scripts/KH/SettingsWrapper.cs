using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public static class SettingsWrapper {
        static readonly string LOOK_SPEED = "_ls";
        static readonly string TEXT_SPEED = "_ts";

		public static float LookSpeed { 
			get => (float)WorldState.Shared.GetSetting(LOOK_SPEED, 1.0f); 
			set => WorldState.Shared.SetSetting(LOOK_SPEED, value); 
		}

		public static float TextSpeed {
			get => (float)WorldState.Shared.GetSetting(TEXT_SPEED, 1.0f);
			set => WorldState.Shared.SetSetting(TEXT_SPEED, value);
		}
	}
}