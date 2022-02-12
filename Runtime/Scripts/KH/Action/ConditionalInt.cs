using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

namespace KH.Actions {
    [CreateAssetMenu(menuName = "Actions/Conditional/Int", fileName = "ConditionalInt")]
    public class ConditionalInt : Conditional {

        [SerializeField]
        public IntReference IntReference1;

        [SerializeField]
        public Comparator Comparison;

        [SerializeField]
        [Tooltip("Other reference to compare it to.")]
        public IntReference IntReference2;
        [Tooltip("Constant to compare IntReference1 to. Only used if IntReference2 is null.")]
        public int Int2;

        public override bool isTrue() {
            switch (Comparison) {
                case Comparator.LessThan:
                    return IntReference1.Value < Int2Value();
                case Comparator.LessThanEqual:
                    return IntReference1.Value <= Int2Value();
                case Comparator.Equal:
                    return IntReference1.Value == Int2Value();
                case Comparator.GreaterThanEqual:
                    return IntReference1.Value >= Int2Value();
                case Comparator.GreaterThan:
                    return IntReference1.Value > Int2Value();
                case Comparator.NotEqual:
                    return IntReference1.Value != Int2Value();
            }
            throw new System.NotImplementedException();
        }

        private float Int2Value() {
            return IntReference2 != null ? IntReference2.Value : Int2;
        }
    }
}