using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

namespace KH.Actions {
    [CreateAssetMenu(menuName = "Actions/Conditional/Float", fileName = "ConditionalFloat")]
    public class ConditionalFloat : Conditional {

        public FloatReference FloatReference1;

        public Comparator Comparison;

        public FloatReference FloatReference2;

        public override bool isTrue() {
            switch (Comparison) {
                case Comparator.LessThan:
                    return FloatReference1.Value < FloatReference2.Value;
                case Comparator.LessThanEqual:
                    return FloatReference1.Value <= FloatReference2.Value;
                case Comparator.Equal:
                    return FloatReference1.Value == FloatReference2.Value;
                case Comparator.GreaterThanEqual:
                    return FloatReference1.Value >= FloatReference2.Value;
                case Comparator.GreaterThan:
                    return FloatReference1.Value > FloatReference2.Value;
                case Comparator.NotEqual:
                    return FloatReference1.Value != FloatReference2.Value;
            }
            throw new System.NotImplementedException();
        }
    }
}