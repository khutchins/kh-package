using System.Collections;
using System.Collections.Generic;
using Pomerandomian;
using UnityEngine;

namespace KH.Infinite {
    [DefaultExecutionOrder(-2)]
    public abstract class InfFollower<T> : MonoBehaviour where T : ChunkInfo {

        [SerializeField] Transform ObjectToFollow;
        [SerializeField] int GenerateRadius = 150;
        [SerializeField] int ClearRadius = 300;

        private ChunkManager<T> _chunkManager;
        private Vector3 _lastCheck = new Vector3(0, -100, 0);
        private IRandom _random;

        abstract protected ChunkManager<T> CreateManager();

        private void Awake() {
            _chunkManager = CreateManager();
            _chunkManager.SetRadius(GenerateRadius, ClearRadius);
            OnAwake();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }

        private void OnValidate() {
            if (_chunkManager != null) {
                _chunkManager.SetRadius(GenerateRadius, ClearRadius);
                Regenerate(ObjectToFollow.position);
            }
        }

        private IRandom Random {
            get {
                if (_random == null) {
                    _random = new SystemRandom();
                }
                return _random;
            }
        }

        private void Start() {
            Regenerate(ObjectToFollow.position);
            StartCoroutine(_chunkManager.CleanupCoroutine());
            OnStart();
        }

        private void Update() {
            if (Vector3.Distance(ObjectToFollow.position, _lastCheck) > 1) {
                Regenerate(ObjectToFollow.position);
                _lastCheck = ObjectToFollow.position;
            }
        }

        public void SetGenerateRadius(int radius) {
            GenerateRadius = radius;
            _chunkManager.SetRadius(GenerateRadius, ClearRadius);
            Regenerate(ObjectToFollow.position);
        }

        public void SetClearRadius(int radius) {
            ClearRadius = radius;
            _chunkManager.SetRadius(GenerateRadius, ClearRadius);
            Regenerate(ObjectToFollow.position);
        }

        public void SetGenerateAndClearRadius(int generate, int clear) {
            GenerateRadius = generate;
            ClearRadius = clear;
            _chunkManager.SetRadius(GenerateRadius, ClearRadius);
            Regenerate(ObjectToFollow.position);
        }

        void Regenerate(Vector3 position) {
            _chunkManager.Update(Vector2Long.Get2DPosition(position, Vector2Long.zero));
        }
    }
}