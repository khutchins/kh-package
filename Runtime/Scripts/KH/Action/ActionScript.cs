using KH.Actions;
using KH.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
    public class ActionScript : Action {
        [TextAssetPreview(200)][SerializeField] KVBSAsset ScriptAsset;
        [TextArea][SerializeField] string Script;

        public override void Begin() {
            StartCoroutine(Run());
        }

        IEnumerator Run() {
            var script = ScriptAsset != null ? ScriptAsset.Text : Script;
            yield return ScriptingEngine.INSTANCE.RunAndAwait(script);
            Finished();
        }
    }
}