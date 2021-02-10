using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
	public class WorldState {
		static WorldState _sharedInstance;
		readonly Dictionary<string, object> _settings = new Dictionary<string, object>();
		readonly Dictionary<string, object> _worldState = new Dictionary<string, object>();
		readonly Dictionary<string, Dictionary<string, object>> _levelStates = new Dictionary<string, Dictionary<string, object>>();
		string _activeLevel = null;

		public static WorldState Shared {
			get {
				if (_sharedInstance == null) {
					_sharedInstance = new WorldState();
				}
				return _sharedInstance;
			}
		}

		public void NewGame() {
			_sharedInstance = new WorldState();
		}

		public void SetSetting(string key, object value) {
			_settings[key] = value;
		}

		public object GetSetting(string key, object defaultValue) {
			return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
		}

		public bool HasWorldState(string key) {
			return _worldState.ContainsKey(key);
		}

		public void SetWorldState(string key, object value) {
			_worldState[key] = value;
		}

		public object GetWorldState(string key, object defaultValue) {
			return _worldState.ContainsKey(key) ? _worldState[key] : defaultValue;
		}

		public void SetActiveLevel(string level) {
			_activeLevel = level;
			MakeLevelState(level);
		}

		private void MakeLevelState(string level) {
			if (!_levelStates.ContainsKey(level)) {
				_levelStates[level] = new Dictionary<string, object>();
			}
		}

		public void SetActiveLevelState(string key, object value) {
			if (_activeLevel == null) {
				Debug.LogWarning("No active level set!");
				return;
			}
			SetLevelState(_activeLevel, key, value);
		}

		public object GetActiveLevelState(string key, object defaultValue) {
			if (_activeLevel == null) {
				Debug.LogWarning("No active level set!");
				return null;
			}
			return GetLevelState(_activeLevel, key, defaultValue);
		}

		public void SetLevelState(string level, string key, object value) {
			MakeLevelState(level);
			_levelStates[level][key] = value;
		}

		public object GetLevelState(string level, string key, object defaultValue) {
			if (!_levelStates.ContainsKey(level)) {
				Debug.LogWarning("Invalid level: " + level);
				return defaultValue;
			}
			return _levelStates[level].ContainsKey(key) ? _levelStates[level][key] : defaultValue;
		}

	}
}