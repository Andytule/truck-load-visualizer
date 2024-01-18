using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using SFB;

public class LoadVisualizerManager : MonoBehaviour
{
    public LoadInstructionsManager lim;
    public BundlePreviewManager bpm;
    public Transform pickListContent;

    public Truck truck;
    public Load load;

    public GameObject stackObj;
    public GameObject bundleObj;
    public GameObject dunnageObj;
    public GameObject loadObj;
    public GameObject pieceDimObj;
    public GameObject pieceConfigObj;
    public GameObject pickListObj;
    public GameObject loadInstructionsUIObject;
    public RectTransform popupPanel;
    public GameObject errorPopup;

    public Text infoBodyText;
    public Text popupTitle;
    public Text popupBody;
    public InputField errorMessage;

    public Transform zero;

    public PivotCam pivotCam;

    public int targetFrameRate = 60;

    public bool loadGenerationComplete = false;
    
    UnitConverter uc = new UnitConverter();
    
    float singleTouchStartTime = 0;

    string filePath = "";

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate; 
    }

    private void Update()
    {
        /*
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            if (hit.transform.tag == "Bundle")
            {
                Bundle hitBundle = hit.collider.GetComponent<Bundle>();

                if (hitBundle.isBundleVisible)
                {
                    popupPanel.gameObject.SetActive(true);
                    popupPanel.transform.position = Input.mousePosition;

                    popupPanel.transform.position += new Vector3(popupPanel.sizeDelta.x / 7, popupPanel.sizeDelta.y / 7, 0);

                    popupTitle.text = hitBundle.name;
                    popupBody.text = hitBundle.getInfo();
                }
            }
            else
            {
                popupPanel.gameObject.SetActive(false);

            }
        }*/


        if (Input.GetMouseButtonDown(0))
        {
            singleTouchStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (Time.time - singleTouchStartTime < 0.2f)
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray.origin, ray.direction, out hit))
                {
                    if (hit.transform.tag == "Bundle")
                    {
                        Bundle hitBundle = hit.collider.GetComponent<Bundle>();

                        if (hitBundle.isBundleVisible)
                        {
                            /*foreach (var b in Bundle.getAllBundles())
                            {
                                b.highlightBundle(false);
                            }
                            hitBundle.highlightBundle(true);*/
                            
                            pivotCam.setFocus(hit.transform);

                            bpm.previewBundle(hitBundle);
                            infoBodyText.text = hitBundle.getInfo();
                        }
                    }
                    else if (hit.transform.tag == "Truck")
                    {
                        pivotCam.setFocus(truck.transform);
                    }

                }
            }
        }
    }

    //Only in Unity Editor: Open file select dialogue
    public void selectFile()
    {
        // Open file with filter
        var extensions = new[] {
            new ExtensionFilter("XML Files", "xml"),
        };
        var filePaths = StandaloneFileBrowser.OpenFilePanel("Select Load", "", extensions, false);

        if (filePaths.Length > 0)
            filePath = filePaths[0];
        else return;

        if(filePath != "")
            StartCoroutine("parseXML");
    }

    //Loops through all bundles in sequence and makes them visible
    //Also creates an item in the pick list UI
    IEnumerator showBundles()
    {
        int counter = 1;
        foreach (var b in Bundle.getAllBundles())
        {
            try
            {
                b.setBundleVisible(true, true);
                GameObject p = Instantiate(pickListObj, pickListContent);
                p.GetComponentInChildren<TMP_Text>().SetText(counter + ". " + b.name);
                p.GetComponent<Button>().onClick.AddListener(() => bpm.previewBundle(b, p.GetComponent<Button>()));
                counter++;

            }
            catch (Exception e)
            {
                print(e.Message);
            }
            yield return new WaitForSeconds(.2f);
        }
        loadGenerationComplete = true;
    }

    //Reads the selected load file and creates all appropriate stacks and bundles
    IEnumerator parseXML()
    {
        resetApplication();
        yield return new WaitForSeconds(0.2f);

        StreamReader reader = new StreamReader(filePath);

        string elXML = "";
        string bXML = "";
        string pXML = "";

        try
        {
            var doc = XDocument.Parse(reader.ReadToEnd());
            reader.Close();

            var loadbuild = doc.Elements("LoadBuild");
            var truckEl = loadbuild.Elements("Truck");
            var loadEl = loadbuild.Elements("Load");

            truck.construct(getAtt<int>(truckEl.Attributes("MaxWeight")), uc.I2M(getAtt<int>(truckEl.Attributes("BedWidth"))), uc.I2M(getAtt<int>(truckEl.Attributes("MaxHeight"))), uc.I2M(getAtt<int>(truckEl.Attributes("BedLength"))));
            load.construct(getAtt<float>(loadEl.Attributes("TotalWeight")), getAtt<int>(loadEl.Attributes("LoadId")), getAtt<int>(loadEl.Attributes("Bol")));
       

            foreach (var el in loadEl.Elements())
            {
                elXML = el.ToString();

                if (el.Name == "Stack")
                {
                    GameObject s = Instantiate(stackObj, zero);
                    s.transform.parent = loadObj.transform;
                    Stack stack = s.GetComponent<Stack>();
                    stack.construct(
                        uc.I2M(getAtt<float>(el.Attributes("Width"))),
                        uc.I2M(getAtt<float>(el.Attributes("Height"))),
                        uc.I2M(getAtt<float>(el.Attributes("Length"))));

                    s.name = "Stack";

                    foreach (var b in el.Elements())
                    {
                        bXML = b.ToString();


                        GameObject b1 = Instantiate(bundleObj, zero);
                        Bundle bundle = b1.GetComponent<Bundle>();


                        foreach (var p in b.Elements())
                        {
                            pXML = p.ToString();

                            if (p.Name == "PieceDimensions")
                            {
                                GameObject p1 = Instantiate(pieceDimObj, b1.transform);
                                PieceDimensions pieceDim = p1.GetComponent<PieceDimensions>();
                                pieceDim.construct(uc.I2M(getAtt<float>(p.Attributes("Width"))),
                                    uc.I2M(getAtt<float>(p.Attributes("Height"))));
                                p1.name = "Piece Dimensions";
                                bundle.pieceDimensions = pieceDim;
                            }

                            if (p.Name == "PieceConfig")
                            {
                                GameObject p2 = Instantiate(pieceConfigObj, b1.transform);
                                PieceConfig pieceConfig = p2.GetComponent<PieceConfig>();
                                pieceConfig.construct(getAtt<string>(p.Attributes("PieceType")),
                                    getAtt<float>(p.Attributes("Width")),
                                    getAtt<float>(p.Attributes("Height")),
                                    getAtt<int>(p.Attributes("PieceCount")));
                                p2.name = "Piece Config";
                                bundle.pieceConfig = pieceConfig;
                            }
                            if (p.Name == "Dunnage")
                            {
                                GameObject dObj = Instantiate(dunnageObj, b1.transform);
                                dObj.name = "Dunnage";
                                Dunnage dunnage = dObj.GetComponent<Dunnage>();
                                dunnage.construct(uc.I2M(getAtt<float>(b.Attributes("Width"))),
                                    uc.I2M(getAtt<float>(p.Attributes("Height"))),
                                    uc.I2M(getAtt<float>(p.Attributes("Length"))),
                                    uc.I2M(getAtt<float>(b.Attributes("Height"))));

                                bundle.dunnage.Add(dunnage);
                            }
                            pXML = "";
                        }

                        bundle.construct(getAtt<string>(b.Attributes("ItemId")),
                            getAtt<int>(b.Attributes("LoadOrder")),
                            getAtt<int>(b.Attributes("StackOrder")),
                            getAtt<int>(b.Attributes("BolLineNumber")),
                            getAtt<float>(b.Attributes("Weight")),
                            uc.I2M(getAtt<float>(b.Attributes("Width"))),
                            uc.I2M(getAtt<float>(b.Attributes("Height"))),
                            uc.I2M(getAtt<float>(b.Attributes("Length"))),
                            uc.mm2M(getAtt<float>(b.Attributes("X"))),
                            uc.mm2M(getAtt<float>(b.Attributes("Z"))),
                            stack, this);

                        b1.transform.parent = stack.transform;
                        stack.bundles.Add(bundle);
                        
                        bXML = "";

                    }
                    load.stacks.Add(stack);
                }

                elXML = "";
            }
            StartCoroutine("showBundles");
        }
        catch (Exception e)
        {
            errorPopup.SetActive(true);

            errorMessage.text = "Error at element: \n\n";
            if (pXML != "")
                errorMessage.text += pXML;
            else if (bXML != "")
                errorMessage.text += bXML;
            else if (elXML != "")
                errorMessage.text += elXML;
            else
                errorMessage.text +=  "Unkown error";
        }
    }

    public void closeErrorPopup()
    {

        errorPopup.SetActive(false);
    }

    //Returns the value of the XML Attribute passed in
    private T getAtt<T>(IEnumerable<XAttribute> el)
    {
        foreach (var att in el){
            if(typeof(T) == Type.GetType("int"))
            {
                return (T)Convert.ChangeType(int.Parse(att.Value), typeof(T));
            }

            if (typeof(T) == Type.GetType("float"))
            {
                return (T)Convert.ChangeType(float.Parse(att.Value), typeof(T));
            }

            return (T)Convert.ChangeType(att.Value, typeof(T));

        }
        return default;
    }

    //Resets all variables for new load generation
    void resetApplication()
    {
        loadObj.GetComponent<Load>().clearLoad();
        lim.resetLoadInstructions();
        loadGenerationComplete = false;

        //Clear the pick list
        foreach (Transform child in pickListContent)
        {
            Destroy(child.gameObject);
        }
    }

    //returns a list of bundles in the layer corresponding to the passed in stackorder
    public List<Bundle> getLayerBundles(int stackOrder)
    {
        return load.getLayerBundles(stackOrder);
    }

}
