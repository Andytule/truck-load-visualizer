using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadInstructionsManager : MonoBehaviour
{
    public List<Bundle> bundleList;
    public LoadVisualizerManager LVM;
    public Text instructionsTxt;
    public Text stepsTxt;
    public Button nextButton;
    public Button prevButton;
    public Button loadViewButton;
    public Text showLoadTxt;
    public Text showLayerTxt;

    public GameObject layerViewObj;
    public Button layerViewButton;

    private int counter = 0;
    public bool drawBundlesOnFinish;
    static private bool doOnce = true;
    private bool showLoad = false;
    private bool showLayer = false;
    bool beginInstructions = false;

    void Update()
    {
        if (drawBundlesOnFinish)
        {
            if (doOnce && LVM.loadGenerationComplete)
            {
                bundleList = Bundle.getAllBundles();
                doOnce = false;
                foreach (Bundle b in bundleList)
                {
                    if (b.isActiveAndEnabled)
                    {
                        counter++;
                    }
                }
                counter--;
                //print($"Inital counter: {counter}");
            }

        }
        else
        {
            if (doOnce && LVM.loadGenerationComplete)
            {
                bundleList = Bundle.getAllBundles();
                doOnce = false;
                showAllBundles(false);
            }
        }
        if (LVM.loadGenerationComplete)
        {
            if (showLoad)
            {
                stepsTxt.text = $"{bundleList.Count()}/{bundleList.Count()}";
            }
            else
            {
                stepsTxt.text = $"{counter + 1}/{bundleList.Count()}";
            }
        }

    }

    //Place next bundle in the load
    public void nextBundle()
    {
        if (LVM.loadGenerationComplete)
        {
            if (isValidIndex(counter + 1))
            {
                counter++;
                //print(counter);
                bundleList[counter].setBundleVisible(true, true);

                foreach (var b in Bundle.getAllBundles())
                {
                    b.highlightBundle(false);
                }

                bundleList[counter].highlightBundle(true);

                instructionsTxt.text = $"Added {bundleList[counter].materialDescription}";
            }
        }
    }

    //Remove the latest bundle in the load
    public void prevBundle()
    {
        if (LVM.loadGenerationComplete)
        {
            if (isValidIndex(counter - 1))
            {
                counter--;
                bundleList[counter + 1].setBundleVisible(false, true);
                foreach (var b in Bundle.getAllBundles())
                {
                    b.highlightBundle(false);
                }
                if (counter >= 0)
                {
                    bundleList[counter].highlightBundle(true);
                    instructionsTxt.text = $"Added {bundleList[counter].materialDescription}";

                }
                else
                {
                    instructionsTxt.text = "";
                }

            }
        }
    }

    //Checks if the provided index is valid in the bundles array
    private bool isValidIndex(int idx)
    {
        return (idx >= -1) && (idx < bundleList.Count());
    }

    //Shows or hides all the bundles
    public void showAllBundles(bool state)
    {
        foreach (Bundle b in bundleList)
        {
            b.setBundleVisible(state);
        }
    }

    //Shows or hides all bundles in the requested layer
    public void showLayerBundles (bool state, int layer)
    {
        foreach (Bundle b in LVM.getLayerBundles(layer))
        {
            b.setBundleVisible(state);
            b.highlightBundle(state, true);
        }
    }

    //Toggles whether the full final load is visible.
    //If the load was just created, the first time this runs begins the load breakdown
    public void toggleFinalLoad()
    {
        if (LVM.loadGenerationComplete)
        {
            if (!beginInstructions)
            {
                showLoadTxt.text = "Show Final Load";
                beginInstructions = true;
                counter = -1;
                layerViewObj.SetActive(true);
                showAllBundles(false);
                nextBundle();

                return;
            }
            if (!showLoad)
            {
                showAllBundles(true);
                showLoad = true;
                showLoadTxt.text = "Return";
                nextButton.interactable = false;
                prevButton.interactable = false;
                layerViewButton.interactable = false;

                foreach (var b in Bundle.getAllBundles())
                {
                    b.highlightBundle(false);
                }

                if (counter >= 0)
                    bundleList[counter].highlightBundle(true);
            }
            else
            {
                showAllBundles(false);
                showLoad = false;
                showLoadTxt.text = "Show Final Load";
                nextButton.interactable = true;
                prevButton.interactable = true;
                layerViewButton.interactable = true;

                for (int i = 0; i <= counter; i++)
                {
                    bundleList[i].setBundleVisible(true);
                }
                if (counter >= 0)
                    bundleList[counter].highlightBundle(true);
            }
        }
    }

    //Toggles whether the layer view is visible
    public void toggleLayerView()
    {
       
        if (!showLayer)
        {
            showLayerBundles(true, 0);
            showLayer = true;
            showLayerTxt.text = "Hide First Layer";
            nextButton.interactable = false;
            prevButton.interactable = false;
            loadViewButton.interactable = false;

        }
        else
        {
            showLayerBundles(false, 0);
            showLayer = false;
            showLayerTxt.text = "Show First Layer";
            nextButton.interactable = true;
            prevButton.interactable = true;
            loadViewButton.interactable = true;


            for (int i = 0; i <= counter; i++)
            {
                bundleList[i].setBundleVisible(true);
                bundleList[i].highlightBundle(false);
            }

        }
        if (counter >= 0)
            bundleList[counter].highlightBundle(true);
    }

    //Resets all variables and UI for new load generation
    public void resetLoadInstructions()
    {
        counter = 0;
        doOnce = true;
        beginInstructions = false;
        instructionsTxt.text = "";
        showLoadTxt.text = "Begin Breakdown";
        stepsTxt.text = "";
        layerViewObj.SetActive(false);

    }
}