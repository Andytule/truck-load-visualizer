using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceConfig : MonoBehaviour
{
    public Bundle.PieceType pieceType;
    public int pieceCount;
    public Vector2 size;

    public void construct(string type, float x, float y, int count)
    {
        switch (type.ToLower())
        {
            case "shape":
                pieceType = Bundle.PieceType.Shape;
                break;
            case "round":
                pieceType = Bundle.PieceType.Round;
                break;
        }
        size = new Vector2(x,y);
        pieceCount = count;
    }
}
