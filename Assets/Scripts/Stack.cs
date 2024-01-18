using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour
{

    public Vector3 size;
    public List<Bundle> bundles;

    UnitConverter uc = new UnitConverter();
   
    public void construct(float width, float height, float length)
    {
        size = new Vector3(uc.I2M(width), uc.I2M(height), uc.I2M(length));
    }

    //Returns the bundle below the passed in stackOrder.
    //This is becuase end-to-end bundles will have the same stack order so we need to loop to find the correct bundle
    public Bundle getBundleBelow(int stackOrder)
    {
        foreach (var b in bundles)
        {
            if(b.stackOrder == stackOrder - 1)
            {
                return b;
            }
        }
        return null;
    }
}


