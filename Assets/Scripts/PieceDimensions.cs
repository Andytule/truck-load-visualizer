using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceDimensions : MonoBehaviour
{

    public Vector2 size;


    public void construct(float x, float y)
    {
        if(x == 0)
            x = y;
        if (y == 0)
            y = x;
        size = new Vector2(x, y);
    }
}
