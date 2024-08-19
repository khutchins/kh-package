using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileThing : MonoBehaviour {
    [SerializeField] MeshRenderer Quad;
    [SerializeField] TMP_Text Text;
    public void Initialize(SampleChunkInfo info, SampleChunkInfo leftChunk, BiggerSampleChunkInfo bigChunk) {
        Text.text = $"{info.Location.Key}\n{info.SomethingRandom}\n< {leftChunk.SomethingRandom}\n{bigChunk.Location.Key}";
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", bigChunk.color);
        Quad.SetPropertyBlock(materialPropertyBlock);
    }
}
