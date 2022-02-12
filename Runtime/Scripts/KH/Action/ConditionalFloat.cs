using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

namespace KH.Actions {
    [CreateAssetMenu(menuName = "Actions/Conditional/Float", fileName = "ConditionalFloat")]
    public class ConditionalFloat : Conditional {

        public FloatReference FloatReference1;

        public Comparator Comparison;

        [Tooltip("Other reference to compare it to.")]
        public FloatReference FloatReference2;
        [Tooltip("Constant to compare FloatReference1 to. Only used if FloatReference2 is null.")]
        public float Float2;

        public override bool isTrue() {
            switch (Comparison) {
                case Comparator.LessThan:
                    return FloatReference1.Value < Float2Value();
                case Comparator.LessThanEqual:
                    return FloatReference1.Value <= Float2Value();
                case Comparator.Equal:
                    return FloatReference1.Value == Float2Value();
                case Comparator.GreaterThanEqual:
                    return FloatReference1.Value >= Float2Value();
                case Comparator.GreaterThan:
                    return FloatReference1.Value > Float2Value();
                case Comparator.NotEqual:
                    return FloatReference1.Value != Float2Value();
            }
            throw new System.NotImplementedException();
        }

        private float Float2Value() {
            return FloatReference2 != null ? FloatReference2.Value : Float2;
		}
    }
}