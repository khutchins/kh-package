using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
    /// <summary>
    /// Class that provides a mechanism for changing a scriptable object's value through Unity callbacks.
    /// Subclass for any ValueReference subclass you want to support.
    /// <typeparam name="T">Underlying type, e.g. int in IntReference (or ValueReference\<int\>).</typeparam>
    /// <typeparam name="U">Value reference type, e.g. IntReference</typeparam>
    /// </summary>
    public class ValueReceptor<T, U> : MonoBehaviour where U : ValueReference<T> {

        public U Reference;

        public void UpdateValue(T newValue) {
            Reference?.SetValue(newValue);
		}
    }
}