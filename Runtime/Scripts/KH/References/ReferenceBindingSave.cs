using KH.References;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
	/// <summary>
	/// Interface that manages saving the actual options. This is normally the struct that holds the save information.
	/// </summary>
	public interface ISavable {
		void Save();
	}

	/// <summary>
	/// Class that manages the binding of value references to some saveable state.
	/// </summary>
	/// <typeparam name="R"></typeparam>
	public abstract class ReferenceBindingSave<R> : MonoBehaviour where R : ISavable {

		public enum SaveType {
			Immediate,
			AfterDelay,
			DoesntTrigger
		}

		protected class Binding<T, U, V> where U : ValueReference<T> {
			public Action<T, V> Save;
			public Action<U, V> Load;
			public ValueReference<T>.OnValueChanged OnValueChanged;
			public SaveType SaveType;

			public Binding(Action<T, V> saveToOptions, Action<U, V> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
				Save = saveToOptions;
				Load = loadFromOptions;
				SaveType = saveType;
			}
		}

		[Tooltip("How long after the last reference change to wait until saving the game. Useful if options are changed a lot in a small amount of time, like a user messing with a slider.")]
		public float SaveDelay = 1f;
		[Tooltip("Maximum time to wait to save the game. Useful if you're constantly changing some scriptable objects and want to minimize the worst-case save delay scenario. NOTE: In this case, you should probably be disabling triggersSave on the binding.")]
		public float MaxSaveDelay = 15f;

		private readonly Dictionary<FloatReference, Binding<float, FloatReference, R>> _floatBindings = new Dictionary<FloatReference, Binding<float, FloatReference, R>>();
		private readonly Dictionary<BoolReference, Binding<bool, BoolReference, R>> _boolBindings = new Dictionary<BoolReference, Binding<bool, BoolReference, R>>();
		private readonly Dictionary<IntReference, Binding<int, IntReference, R>> _intBindings = new Dictionary<IntReference, Binding<int, IntReference, R>>();
		private readonly Dictionary<StringReference, Binding<string, StringReference, R>> _stringBindings = new Dictionary<StringReference, Binding<string, StringReference, R>>();
		private readonly Dictionary<Vector3Reference, Binding<Vector3, Vector3Reference, R>> _vector3Bindings = new Dictionary<Vector3Reference, Binding<Vector3, Vector3Reference, R>>();
		private float _delayableSaveTime;
		private float _undelayableSaveTime;
		private Coroutine _saveCoroutine = null;
		// Theoretically this variable is redundant because I *should* be able
		// to check if (_saveCoroutine != null), but a null coroutine was still
		// going into the block. Probably Unity doing its weird fake null thing.
		private bool _isCoroutineActive = false;

		/// <summary>
		/// Loads the initial state, creating a new instance if necessary.
		/// </summary>
		/// <returns></returns>
		protected abstract R GetSave();

		private void GetInitial<A, B>(Dictionary<B, Binding<A, B, R>> bindings) where B : ValueReference<A> {
			foreach (B reference in bindings.Keys) {
				bindings[reference].Load(reference, GetSave());
			}
		}

		private void Bind<A, B>(Dictionary<B, Binding<A, B, R>> bindings) where B : ValueReference<A> {
			foreach (B reference in bindings.Keys) {
				Binding<A, B, R> binding = bindings[reference];
				if (binding.OnValueChanged == null) {
					binding.OnValueChanged = (A newValue) => {
						binding.Save(newValue, GetSave());
						switch (binding.SaveType) {
							case SaveType.AfterDelay:
								ScheduleSave();
								break;
							case SaveType.Immediate:
								SaveNow();
								break;
							case SaveType.DoesntTrigger:
								break;

						}
					};
				}
				reference.ValueChanged += binding.OnValueChanged;
			}
		}

		private void Unbind<A, B>(Dictionary<B, Binding<A, B, R>> bindings) where B : ValueReference<A> {
			foreach (B reference in bindings.Keys) {
				Binding<A, B, R> binding = bindings[reference];
				if (binding.OnValueChanged != null) {
					reference.ValueChanged -= binding.OnValueChanged;
				}
			}
		}

		public void SetInitialSOValues() {
			GetInitial(_floatBindings);
			GetInitial(_boolBindings);
			GetInitial(_intBindings);
			GetInitial(_stringBindings);
			GetInitial(_vector3Bindings);
			AdditionalInitialSOValues();
		}

		protected virtual void AdditionalInitialSOValues() { }
		protected virtual void AdditionalBindings() { }
		protected virtual void AdditionalUnbindings() { }

		private void OnEnable() {
			Bind(_floatBindings);
			Bind(_boolBindings);
			Bind(_intBindings);
			Bind(_stringBindings);
			Bind(_vector3Bindings);
			AdditionalBindings();
		}

		private void OnDisable() {
			Unbind(_floatBindings);
			Unbind(_boolBindings);
			Unbind(_intBindings);
			Unbind(_stringBindings);
			Unbind(_vector3Bindings);
			AdditionalUnbindings();
		}

		public void ScheduleSave() {
			_delayableSaveTime = Time.unscaledTime + SaveDelay;
			_undelayableSaveTime = Mathf.Min(Time.unscaledTime + MaxSaveDelay, _undelayableSaveTime);
			if (!_isCoroutineActive) {
				_isCoroutineActive = true;
				_saveCoroutine = StartCoroutine(SaveOptionsAfterDelay());
			}
		}

		IEnumerator SaveOptionsAfterDelay() {
			while (Time.unscaledTime < _delayableSaveTime && Time.unscaledTime < _undelayableSaveTime) {
				yield return null;
			}
			SaveNow();
		}

		public void SaveNow() {
			if (_isCoroutineActive) {
				// Theoretically these should be in alignment, but just in case.
				if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
				_saveCoroutine = null;
				_isCoroutineActive = false;
			}
			_undelayableSaveTime = float.MaxValue;
			GetSave().Save();
		}

		protected void AddFloatBinding(FloatReference reference, Action<float, R> saveToOptions, Action<FloatReference, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
			AddBinding(_floatBindings, reference, saveToOptions, loadFromOptions, saveType);
		}

		protected void AddBoolBinding(BoolReference reference, Action<bool, R> saveToOptions, Action<BoolReference, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
			AddBinding(_boolBindings, reference, saveToOptions, loadFromOptions, saveType);
		}

		protected void AddIntBinding(IntReference reference, Action<int, R> saveToOptions, Action<IntReference, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
			AddBinding(_intBindings, reference, saveToOptions, loadFromOptions, saveType);
		}

		protected void AddStringBinding(StringReference reference, Action<string, R> saveToOptions, Action<StringReference, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
			AddBinding(_stringBindings, reference, saveToOptions, loadFromOptions, saveType);
		}

		protected void AddVector3Binding(Vector3Reference reference, Action<Vector3, R> saveToOptions, Action<Vector3Reference, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) {
			AddBinding(_vector3Bindings, reference, saveToOptions, loadFromOptions, saveType);
		}

		protected void AddBinding<A, B>(Dictionary<B, Binding<A, B, R>> bindings, B reference, Action<A, R> saveToOptions, Action<B, R> loadFromOptions, SaveType saveType = SaveType.AfterDelay) where B : ValueReference<A> {
			bindings[reference] = new Binding<A, B, R>(saveToOptions, loadFromOptions, saveType);
		}
	}
}