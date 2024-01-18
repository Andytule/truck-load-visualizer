using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
    public float MaxWeight;
    public Vector2 BedSize;
    public float MaxHeight;

    UnitConverter uc = new UnitConverter();

    public void construct(int weight, float width, float height, float length)
    {
        MaxWeight = weight;
        MaxHeight = height;
        BedSize = new Vector2(width, length);

        transform.localScale = new Vector3(width, uc.I2M(40), length);

    }

}
