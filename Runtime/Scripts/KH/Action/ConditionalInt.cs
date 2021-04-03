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
        public IntReference IntReference2;

        public override bool isTrue() {
            switch (Comparison) {
                case Comparator.LessThan:
                    return IntReference1.Value < IntReference2.Value;
                case Comparator.LessThanEqual:
                    return IntReference1.Value <= IntReference2.Value;
                case Comparator.Equal:
                    return IntReference1.Value == IntReference2.Value;
                case Comparator.GreaterThanEqual:
                    return IntReference1.Value >= IntReference2.Value;
                case Comparator.GreaterThan:
                    return IntReference1.Value > IntReference2.Value;
                case Comparator.NotEqual:
                    return IntReference1.Value != IntReference2.Value;
            }
            throw new System.NotImplementedException();
        }
    }
}