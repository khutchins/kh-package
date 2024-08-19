using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pomerandomian;
using UnityEngine;

namespace KH.Infinite {
    [DefaultExecutionOrder(-2)]
    public abstract class InfFollower : MonoBehaviour {

        [SerializeField] Transform ObjectToFollow;
        [Tooltip("Distance to generate tiles out to.")]
        [SerializeField] int GenerateRadius = 150;
        [Tooltip("Distance to clean up tiles beyond. Should be larger than the generate radius to avoid thrashing.")]
        [SerializeField] int ClearRadius = 300;

        private IChunkManager[] _chunkManagers;
        private Vector3 _lastCheck = new Vector3(0, -100, 0);
        private IRandom _random;

        virtual protected IChunkManager CreateSingleManager() { return null; }
        virtual protected IEnumerable<IChunkManager> CreateManagers() {
            return new IChunkManager[] { CreateSingleManager() };
        }

        private void Awake() {
            _chunkManagers = CreateManagers().ToArray();
            if (_chunkManagers == null || _chunkManagers.Length == 0 || _chunkManagers[0] == null) {
                Debug.LogError("No chunk generator exists. Override either CreateManager or CreateMultipleManagers.");
                this.enabled = false;
                return;
            }
            foreach (var chunkManager in _chunkManagers) {
                chunkManager.SetRadius(GenerateRadius, ClearRadius);
            }
            OnAwake();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }

        private void OnValidate() {
            if (_chunkManagers != null) {
                foreach (var chunkManager in _chunkManagers) {
                    chunkManager.SetRadius(GenerateRadius, ClearRadius);
                }
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
            foreach (var chunkManager in _chunkManagers) {
                StartCoroutine(chunkManager.CleanupCoroutine());
            }
            OnStart();
        }

        private void Update() {
            if (Vector3.Distance(ObjectToFollow.position, _lastCheck) > 1) {
                Regenerate(ObjectToFollow.position);
                _lastCheck = ObjectToFollow.position;
            }
        }

        /// <summary>
        /// Provides a simple seed that you can use for deterministic chunk generation.
        /// You don't have to use this, it's optional.
        /// </summary>
        /// <param name="baseSeed">The base seed. Can either be a random value or a determinstic one, depending on whether you want the game to play the same each run or not.</param>
        /// <param name="loc">Location of the chunk you want the seed for.</param>
        /// <returns></returns>
        protected static string SeedForChunkLocation(string baseSeed, ChunkLocation loc) {
            return $"{baseSeed}_{loc.Key.x}_{loc.Key.y}";
        }

        public void SetGenerateRadius(int radius) {
            GenerateRadius = radius;
            foreach (var chunkManager in _chunkManagers) {
                chunkManager.SetRadius(GenerateRadius, ClearRadius);
            }
            Regenerate(ObjectToFollow.position);
        }

        public void SetClearRadius(int radius) {
            ClearRadius = radius;
            foreach (var chunkManager in _chunkManagers) {
                chunkManager.SetRadius(GenerateRadius, ClearRadius);
            }
            Regenerate(ObjectToFollow.position);
        }

        public void SetGenerateAndClearRadius(int generate, int clear) {
            GenerateRadius = generate;
            ClearRadius = clear;
            foreach (var chunkManager in _chunkManagers) {
                chunkManager.SetRadius(GenerateRadius, ClearRadius);
            }
            Regenerate(ObjectToFollow.position);
        }

        void Regenerate(Vector3 position) {
            foreach (var chunkManager in _chunkManagers) {
                chunkManager.Update(Vector2Long.Get2DPosition(position, Vector2Long.zero));
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(ObjectToFollow.transform.position, GenerateRadius);
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(ObjectToFollow.transform.position, ClearRadius);
        }
    }

    /// <summary>
    /// This exists for compatibility purposes. Use InfFollower (with no type) instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use InfFollower (no type) instead.")]
    public abstract class InfFollower<T> : InfFollower where T : ChunkInfo {
        protected abstract ChunkManager<T> CreateManager();

        protected override IChunkManager CreateSingleManager() {
            return CreateManager();
        }
    }
}