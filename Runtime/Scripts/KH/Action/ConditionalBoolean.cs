using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

namespace KH.Actions {
    [CreateAssetMenu(menuName = "Actions/Conditional/Boolean", fileName = "ConditionalBoolean")]
    public class ConditionalBoolean : Conditional {

        public BoolReference BoolCondition;

        public bool Not;

        public override bool isTrue() {
            return BoolCondition.Value != Not;
        }
    }
}