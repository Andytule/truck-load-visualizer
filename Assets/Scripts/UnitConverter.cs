using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitConverter : MonoBehaviour
{
    //Returns inches converted from meteres
    public float M2I(float meter)
    {
        return meter * 39.3700787f;
    }

    //Returns meteres converted from inches
    public float I2M(float inch)
    {
        return inch / 39.3700787f;
    }
    
    //Returns meteres converted from millimeteres
    public float mm2M(float mm)
    {
        return mm / 1000;
    }

    //Returns millimeteres converted from meteres
    public float M2mm(float meter)
    {
        return meter * 1000;
    }

    //Returns meteres converted from feet
    public float ft2M(float ft)
    {
        return ft / 3.28084f;
    }

    //Returns feet converted from meteres
    public float M2ft(float meter)
    {
        return meter * 3.28084f;
    }
    
    //Returns feet converted from inches
    public float I2ft(float inch)
    {
        return inch / 12;
    }
   
    //Returns inches converted from feet
    public float ft2I(float ft)
    {
        return ft * 12;
    }
}
