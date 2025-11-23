using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Anim {
    /// <summary>
    /// Way of adding some life to procedural animations.
    /// Taken from: https://www.youtube.com/watch?v=KPoeNZZ6H4s
    /// 
    /// T must be multiplicable by a scalar and be able to be added
    /// and subtracted with itself.
    /// </summary>
    public abstract class SecondOrderDynamics<T> {
        private T _xp;
        private T _y, _yd;

        private float _k1;
        private float _k2;
        private float _k3;

        /// <summary>
        /// Parameters for a SOD with a f of 2, a z of 0.35, and a r of 1.2.
        /// It provides a fairly nice feeling bounce.
        /// </summary>
        public static (float f, float z, float r) StandardBounce() {
            return (2f, 0.35f, 1.2f);
        }

        /// <summary>
        /// Instantiates a second order dynamics system. Uses Vector3
        /// but Vector2 or floats can be packed in instead.
        /// </summary>
        /// <param name="f">Frequency. Speed at which system responds to change. Higher frequencies will result in harsher changes.</param>
        /// <param name="z">Zeta. The damping coefficient. 0 means vibration never dies down. 0-1 underdamped. 1 damped.</param>
        /// <param name="r">Initial response. Negative will cause anticipation, > 1 will cause overshoot.</param>
        /// <param name="x0">Starting state</param>
        public SecondOrderDynamics(float f, float z, float r, T x0) {
            _xp = x0;
            _y = x0;
            _yd = DefaultValue();
            SetParameters(f, z, r);
        }

        public void SetParameters(float f, float z, float r) {
            float pif = Mathf.PI * f;
            _k1 = z / pif;
            _k2 = 1f / ((2 * pif) * (2 * pif));
            _k3 = r * z / (2 * pif);
        }

        public virtual T Update(float dt, T x) {
            T xd = Divide(Minus(x, _xp), Mathf.Max(dt, 0.001f));
            _xp = x;
            return Update(dt, x, xd);
        }

        public virtual T Update(float dt, T x, T xd) {
            // clamp k2 to guarantee stability
            float k2Stable = Mathf.Max(_k2, 1.1f * (dt * dt / 4 + dt * _k1 / 2));
            _y = Plus(_y, Times(_yd, dt)); // integrate position by velocity
            // _yd = _yd + dt * (x + _k3 * xd - _y - _k1 * _yd) / k2Stable; // integrate velocity by acceleration
            _yd = Plus(_yd, Divide(Times(Minus(Minus(Plus(x, Times(xd, _k3)), _y), Times(_yd, _k1)), dt), k2Stable));
            return _y;
        }

        public void AddVelocity(T v) {
            _yd = Plus(_yd, v);
        }

        public void SetState(T offset) {
            _y = offset;
            _yd = DefaultValue();
        }

        abstract protected T Times(T obj, float scalar);
        abstract protected T Divide(T obj, float scalar);
        abstract protected T Plus(T obj1, T obj2);
        abstract protected T Minus(T obj1, T obj2);
        abstract protected T DefaultValue();
    }

    public class SecondOrderDynamicsV1 : SecondOrderDynamics<float> {
        public SecondOrderDynamicsV1(float f, float z, float r, float x0) : base(f, z, r, x0) { }

        protected override float DefaultValue() {
            return 0;
        }

        protected override float Divide(float obj, float scalar) {
            return obj / scalar;
        }

        protected override float Minus(float obj1, float obj2) {
            return obj1 - obj2;
        }

        protected override float Plus(float obj1, float obj2) {
            return obj1 + obj2;
        }

        protected override float Times(float obj, float scalar) {
            return obj * scalar;
        }
    }

    public class SecondOrderDynamicsV2 : SecondOrderDynamics<Vector2> {
        public SecondOrderDynamicsV2(float f, float z, float r, Vector2 x0) : base(f, z, r, x0) { }

        protected override Vector2 DefaultValue() {
            return Vector2.zero;
        }

        protected override Vector2 Divide(Vector2 obj, float scalar) {
            return obj / scalar;
        }

        protected override Vector2 Minus(Vector2 obj1, Vector2 obj2) {
            return obj1 - obj2;
        }

        protected override Vector2 Plus(Vector2 obj1, Vector2 obj2) {
            return obj1 + obj2;
        }

        protected override Vector2 Times(Vector2 obj, float scalar) {
            return obj * scalar;
        }
    }

    public class SecondOrderDynamicsV3 : SecondOrderDynamics<Vector3> {
        public SecondOrderDynamicsV3(float f, float z, float r, Vector3 x0) : base(f, z, r, x0) { }

        protected override Vector3 DefaultValue() {
            return Vector3.zero;
        }

        protected override Vector3 Divide(Vector3 obj, float scalar) {
            return obj / scalar;
        }

        protected override Vector3 Minus(Vector3 obj1, Vector3 obj2) {
            return obj1 - obj2;
        }

        protected override Vector3 Plus(Vector3 obj1, Vector3 obj2) {
            return obj1 + obj2;
        }

        protected override Vector3 Times(Vector3 obj, float scalar) {
            return obj * scalar;
        }
    }

    /// <summary>
    /// A version of the SecondOrderDynamics that uses some hacks to make
    /// rotations not react poorly when they wrap from 360 -> 0 or vice versa.
    /// </summary>
    public class SecondOrderDynamicsV3Euler : SecondOrderDynamicsV3 {

        private Vector3 _prevTarget;

        public SecondOrderDynamicsV3Euler(float f, float z, float r, Vector3 x0) : base(f, z, r, x0) { }

        private Vector3 EulerHack(Vector3 prev, Vector3 target) {
            return new Vector3(
                Mathf.DeltaAngle(prev.x, target.x) + prev.x,
                Mathf.DeltaAngle(prev.y, target.y) + prev.y,
                Mathf.DeltaAngle(prev.z, target.z) + prev.z
            );
        }

        public override Vector3 Update(float dt, Vector3 x) {
            x = EulerHack(_prevTarget, x);
            _prevTarget = x;
            return base.Update(dt, x);
        }

        public override Vector3 Update(float dt, Vector3 x, Vector3 xd) {
            x = EulerHack(_prevTarget, x);
            _prevTarget = x;
            return base.Update(dt, x, xd);
        }
    }
}