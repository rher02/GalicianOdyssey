using UnityEngine;
using System.Collections;

public class AssetRotation : MonoBehaviour
{
    void Start()
    {
        float[] rotations = { 0f, 90f, 10f, 270f };
        float rotation = rotations[Random.Range(0, rotations.Length)];

        transform.rotation = Quaternion.Euler(0f,rotation, 0f);

        float scale = Random.Range(0.9992f, 1.0008f);
        transform.localScale *= scale;
    }
}