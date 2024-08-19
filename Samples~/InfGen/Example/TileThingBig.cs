using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileThingBig : MonoBehaviour {
    [SerializeField] MeshRenderer Quad;
    [SerializeField] TMP_Text Text;
    public void Initialize(BiggerSampleChunkInfo info) {
        Text.text = $"{info.Location.Key}";
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", info.color);
        Quad.SetPropertyBlock(materialPropertyBlock);
    }
}
