using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    [SerializeField] private List<Mesh> Meshes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(i).GetComponent<MeshFilter>().mesh = LionsHelper.HelperFunctions.GetWeightedRandom(Meshes.ToArray());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
