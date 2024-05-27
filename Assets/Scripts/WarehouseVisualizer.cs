using System.Collections.Generic;
using UnityEngine;

public class WarehouseVisualizer : MonoBehaviour
{
    public GameObject cubePrefab; // Assign a cube prefab in the Unity Editor
    public List<Product> products; // This will be populated with the best chromosome's products
    public Material A, B, C;
    // Call this method to visualize the product locations
    public void Visualize(Transform parent)
    {
        double turnA = 0.215;
        double turnB = 0.023;
        foreach (var product in products)
        {
            Vector3 position = new Vector3(product.row * 3, product.level, product.column);
            var go = Instantiate(cubePrefab, position + parent.position, Quaternion.identity, parent);
            if (product.turnoverRate == turnA)
            {
                go.GetComponent<MeshRenderer>().sharedMaterial = A;
   
            }
            else if (product.turnoverRate == turnB)
            {
                go.GetComponent<MeshRenderer>().sharedMaterial = B;
              
            }
            else
            {
                go.GetComponent<MeshRenderer>().sharedMaterial = C;
            }
        }
    }
}