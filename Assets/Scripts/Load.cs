using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Load : MonoBehaviour
{
    public int BOL;
    public int LoadID;
    public float TotalWeight;

    public List<Stack> stacks;

    public void construct(float weight, int id, int bol)
    {
        TotalWeight = weight;
        LoadID = id;
        BOL = bol;
    }

    //Delete all of this load's stacks
    public void clearLoad()
    {
        if (stacks.Count == 0) return;

        for(int i = stacks.Count - 1; i >= 0; i--)
        {
            Destroy(stacks[i].gameObject);
        }
        stacks.Clear();
    }

    //Returns all the bundles in the requested layer
    public List<Bundle> getLayerBundles(int layer)
    {
        List<Bundle> layerBundles = new List<Bundle>();
        foreach(var s in stacks)
        {
            foreach(var b in s.bundles)
            {
                if(b.stackOrder == layer)
                    layerBundles.Add(b);
            }
        }

        return layerBundles;
    }

}
