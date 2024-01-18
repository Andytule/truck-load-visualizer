using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dunnage : MonoBehaviour
{
    public Vector3 size;
    public void construct(float width, float height, float length, float parentHeight)
    {
        size = new Vector3(width, height, length);
        transform.localScale = size;
        transform.position += new Vector3(0, -((parentHeight / 2) + (height / 2)), 0);
    }

    public void offset(float offset)
    {
        transform.position += new Vector3(0, 0, offset);
    }
}
