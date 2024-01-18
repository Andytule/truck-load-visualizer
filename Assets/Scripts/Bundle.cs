using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Bundle : MonoBehaviour
{
    public enum PieceType
    {
        Shape,
        Round
    }

    public string materialDescription;
    public string itemID;
    public int loadOrder;
    public int stackOrder;
    public int bolLineNumber;
    public float bundleWeight;
    public bool isBundleVisible;
    public Vector3 size;
    public Vector2 position;
    public PieceDimensions pieceDimensions;
    public PieceConfig pieceConfig;
    public List<Dunnage> dunnage;
    public Stack stack;
    public GameObject shapeObj;
    public GameObject roundObj;
    public GameObject squarePipe;
    public GameObject roundPipe;
    public LoadVisualizerManager lvm;
    public Material dunnageMat;
    public Material defaultPipeMat;
    public Material highlightMat;
    public Material highlightLayerMat;

    UnitConverter uc = new UnitConverter();

    Vector3 actualPosition;
    Vector3 toPosition;

    bool goToPosition = false;

    float smoothMoveScale = 0.1f;
    float showAnimationFinishThreshold = 0.001f;
    float hideAnimationFinishThreshold = 0.5f;
    float animationFinishThreshold;


    void Update()
    {
        if (goToPosition)
        {
            if (Vector3.Distance(transform.position, toPosition) > animationFinishThreshold)
            {
                transform.position = Vector3.Lerp(transform.position, toPosition, smoothMoveScale);
            }
            else
            {
                goToPosition = false;

                if (!isBundleVisible)
                {
                    setBundleRendered(false);
                    transform.position = actualPosition;
                }
            }
        }

    }


    public void construct(string id, int loadOrder, int stackOrder, int bol, float weight, float width, float height, float length, float x, float y, Stack stack, LoadVisualizerManager lvm)
    {
        this.lvm = lvm;
        this.stack = stack;
        itemID = id;
        this.loadOrder = loadOrder;
        this.stackOrder = stackOrder;
        bolLineNumber = bol;
        bundleWeight = weight;
        size = new Vector3(width, height, length);
        position = new Vector2(-x, y);

        materialDescription = $"{uc.M2I(size.z)}-{uc.M2I(size.x)}-{uc.M2I(size.y)}-{pieceConfig.size.x}x{pieceConfig.size.y}-{pieceConfig.pieceType}";


        //offset bundle based on its size so that the top left corner is at zero
        transform.position += new Vector3(0, size.y / 2, size.z / 2);

        //then move the bundle based on the x and z provided
        transform.position += new Vector3(position.x, 0, position.y);

        transform.name = "Bundle - " + id;

        //un-parent the bundle from the zero object and reset the scale
        transform.parent = null;
        transform.localScale = new Vector3(1, 1, 1);

        //set the collider to the size of the bundle
        gameObject.GetComponent<BoxCollider>().size = size;

        if (dunnage.Count > 0)
            setDunnage();

        setPipes();
        setPositionHeight();
        setBundleVisible(false);
        actualPosition = transform.position;
    }

    //Generates and positions the individual pipes
    public void setPipes()
    {
        //create and size the pipe according to the piece config and piece dimensions of this bundle
        GameObject pipeObj = Instantiate(pieceConfig.pieceType == PieceType.Shape ? squarePipe : roundPipe, transform.Find("Pipes"));
        pipeObj.name = "Pipe";
        Vector3 pSize = new Vector3(pieceDimensions.size.x, pieceConfig.pieceType == PieceType.Shape ? pieceDimensions.size.y : pieceDimensions.size.x, size.z);

        pipeObj.transform.localScale = pSize;

        if (pieceConfig.pieceType == PieceType.Shape)
        {
            //move first pipe to top left corner of the bundle
            pipeObj.transform.position += new Vector3((size.x / 2) - (pSize.x / 2), (size.y / 2) - (pSize.y / 2), 0);

            for (int i = 0; i < pieceConfig.size.y; i++)
            {
                for (int j = 0; j < pieceConfig.size.x; j++)
                {
                    if (i == 0 && j == 0)//skip the first as we made it before the for loops
                        continue;

                    //duplicate and shift the pipe by its row(i) and col(j) indexes
                    GameObject temp = Instantiate(pipeObj, transform.Find("Pipes"));
                    temp.name = "Pipe";
                    temp.transform.position += new Vector3(-(pSize.x * j), -(pSize.y * i), 0);
                }
            }
        }
        else
        {
            float diameter = pSize.x;
            float shiftX = -(diameter / 2);

            //finds the amount to adjust the Y of the pipe so that it touches the pipes below once it's X has been shifted 
            float adjustY = diameter - Mathf.Sqrt(Mathf.Pow(diameter, 2) - Mathf.Pow(shiftX, 2));

            if (float.IsNaN(adjustY))
                adjustY = 0;

            int numPipeLayers = (int)(pieceConfig.size.y / 2 + 0.5);// +0.5: round up

            GameObject empty = new GameObject();
            empty.name = "empty";

            for (int i = 0; i < numPipeLayers; i++)
            {
                GameObject pipeLayer = Instantiate(empty, transform.Find("Pipes"));
                pipeLayer.name = "Pipe Layer - " + i;

                int numPipeColumns = (int)((pieceConfig.size.x - i) / 2 + 0.5);// +0.5: round up

                //each consecutive layer of pipes will have 1 less pipe
                for (int j = 0; j < numPipeColumns; j++)
                {
                    GameObject pipe = Instantiate(pipeObj, pipeLayer.transform);
                    pipe.name = "Pipe - " + j;

                    float shiftXAmount = pSize.x * j;

                    if ((pieceConfig.size.x - i) % 2 != 0)//if number of pipes in this row is not even, we need a middle pipe
                    {
                        if (j == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        shiftXAmount += pSize.x / 2;
                    }

                    //duplicate the pipe
                    GameObject pipe2 = Instantiate(pipeObj, pipeLayer.transform);
                    pipe2.name = "Pipe - " + j;

                    //move 1 pipe left of the center, and 1 right of the center
                    pipe.transform.position += new Vector3(-shiftXAmount, 0, 0);
                    pipe2.transform.position += new Vector3(shiftXAmount, 0, 0);
                }

                float shiftYAmount = pSize.x * i;

                if (pieceConfig.size.y % 2 != 0)
                {
                    if (i == 0) continue;
                }
                else
                {
                    shiftYAmount += pSize.x;
                    shiftYAmount /= 2;
                }

                GameObject pipeLayer2 = null;

                if (pieceConfig.size.y > 2)
                {
                    //duplicate the layer
                    pipeLayer2 = Instantiate(pipeLayer, transform.Find("Pipes"));
                    pipeLayer2.name = "Pipe Layer - " + i;

                    //move 1 layer up and 1 layer down from the center layer (reduce the amount to shift using the adjustY value so that the layers will touch their preceding layers)
                    pipeLayer.transform.position += new Vector3(0, -shiftYAmount + (adjustY * i), 0);
                    pipeLayer2.transform.position += new Vector3(0, shiftYAmount - (adjustY * i), 0);
                }
                else//special pipe configuration for bundles with only 2 layers
                {
                    pipeLayer2 = Instantiate(new GameObject(), transform.Find("Pipes"));
                    pipeLayer2.name = "Pipe Layer - " + i;

                    //for bundles with only 2 rows, the second row is 1 less pipe than the first
                    for (int j = 0; j < pieceConfig.size.x - i - 1; j++)
                    {
                        GameObject pipe = Instantiate(pipeObj, pipeLayer2.transform);
                        pipe.name = "Pipe - " + j;

                        float shiftXAmount = pSize.x * j;

                        if ((pieceConfig.size.x - i - 1) % 2 != 0)//if number of pipes in this row is not even, we need a middle pipe
                        {
                            if (j == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            shiftXAmount += pSize.x / 2;
                            adjustY /= 2;
                        }

                        //duplicate the pipe
                        GameObject pipe2 = Instantiate(pipeObj, pipeLayer2.transform);
                        pipe2.name = "Pipe - " + j;

                        //move 1 pipe left of the center, and 1 right of the center
                        pipe.transform.position += new Vector3(shiftXAmount, 0, 0);
                        pipe2.transform.position += new Vector3(-shiftXAmount, 0, 0);

                        //manually increment the pipe index since we just made 2 pipes this iteration of the for loop
                        j++;
                    }

                    //move 1 layer up and 1 layer down from the center layer (reduce the amount to shift using the adjustY value so that the layers will touch their preceding layers)
                    pipeLayer.transform.position += new Vector3(0, -shiftYAmount + (adjustY), 0);
                    pipeLayer2.transform.position += new Vector3(0, shiftYAmount - (adjustY), 0);
                }


            }
            Destroy(empty);

            Destroy(pipeObj);
        }
    }

    //Sets the position of this bundle based on the position of the bundle before this one in the Stack
    public void setPositionHeight()
    {
        float baseOffset = 0;

        if (stackOrder == 0)
        {
            if (dunnage.Count > 0)
            {
                baseOffset += dunnage.ToArray()[0].size.y;
            }

            transform.position += new Vector3(0, baseOffset, 0);
            return;
        }

        Bundle below = stack.getBundleBelow(stackOrder);

        transform.position = new Vector3(transform.position.x, below.transform.position.y, transform.position.z);

        baseOffset = (below.size.y / 2) + size.y / 2;

        if (dunnage.Count > 0)
        {
            baseOffset += dunnage.ToArray()[0].size.y;
        }

        transform.position += new Vector3(0, baseOffset, 0);
    }

    //Generate dunnage every x feet if dunnage is required.
    public void setDunnage()
    {

        float dunnageSpacer = 3;
        Dunnage temp = dunnage[0];
        GameObject dObj = temp.gameObject;
        temp.transform.parent = transform.Find("Dunnage");

        float endOffset = size.z / 2 - temp.size.z / 2;

        GameObject dObj2 = Instantiate(dObj, transform.Find("Dunnage"));
        dObj2.name = "Dunnage";
        Dunnage dunnage2 = dObj2.GetComponent<Dunnage>();

        int dunnageCount = Convert.ToInt32((size.z / 2) / uc.ft2M(dunnageSpacer));

        for (int i = 1; i < dunnageCount; i++)
        {
            GameObject temp1 = Instantiate(dObj, transform.Find("Dunnage"));
            Dunnage dunnageTemp1 = temp1.GetComponent<Dunnage>();

            GameObject temp2 = Instantiate(dObj, transform.Find("Dunnage"));
            Dunnage dunnageTemp2 = temp2.GetComponent<Dunnage>();

            dunnageTemp1.offset(endOffset - (uc.ft2M(3) * i));
            dunnageTemp2.offset(-(endOffset - (uc.ft2M(3) * i)));

            temp1.name = "Dunnage";
            temp2.name = "Dunnage";

            dunnage.Add(dunnageTemp1);
            dunnage.Add(dunnageTemp2);
        }

        temp.offset(endOffset);
        dunnage2.offset(-endOffset);

        dunnage.Add(dunnage2);
    }

    //Returns all bundles in the load sorted by the bundle sort order
    public static List<Bundle> getAllBundles()
    {
        List<Bundle> bundles = new List<Bundle>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Bundle");

        foreach (var o in objs)
        {
            bundles.Add(o.GetComponent<Bundle>());
        }

        return bundles.OrderBy(w => w.loadOrder).ToList();
    }

    //Animate or instantly remove/place a bundle
    public void setBundleVisible(bool state, bool animate = false)
    {
        isBundleVisible = state;

        if (animate)
        {

            if (state)
            {
                setBundleRendered(state);
                toPosition = actualPosition;
                transform.position = new Vector3(actualPosition.x, actualPosition.y + 5, actualPosition.z);
                goToPosition = true;

                animationFinishThreshold = showAnimationFinishThreshold;
            }
            else
            {
                toPosition = new Vector3(actualPosition.x, actualPosition.y + 5, actualPosition.z);
                goToPosition = true;

                animationFinishThreshold = hideAnimationFinishThreshold;
            }
        }
        else
        {
            setBundleRendered(state);
        }
    }

    //Enables or disabled the pipes and dunnage of a bundle(this will show or hide the bundle)
    void setBundleRendered(bool state)
    {
        transform.Find("Dunnage").gameObject.SetActive(state);
        transform.Find("Pipes").gameObject.SetActive(state);
        GetComponent<BoxCollider>().enabled = state;
    }

    //Highlights the bundle with a colored material
    public void highlightBundle(bool highlight, bool layer = false)
    {
        MeshRenderer[] rendereres = GetComponentsInChildren<MeshRenderer>();

        if (highlight)
        {
            foreach (var r in rendereres)
            {
                if(layer)
                    r.material = highlightLayerMat;
                else
                    r.material = highlightMat;
            }
        }
        else
        {
            foreach (var r in rendereres)
            {
                r.material = defaultPipeMat;

                if (r.gameObject.tag == "Dunnage")
                {
                    r.material = dunnageMat;
                }
            }
        }
    }

    //Returns info about the bundle
    public string getInfo()
    {
        return $"Weight: {bundleWeight} | Bol Line Number: {bolLineNumber} | Load Sequence: {loadOrder + 1} | Level: {stackOrder + 1}\nLength: {uc.M2I(size.z)} | Width: {uc.M2I(size.x)} | Height: {uc.M2I(size.y)} | Type: {pieceConfig.pieceType} | Piece Config: {pieceConfig.size.x} x {pieceConfig.size.y}";
    }
}
