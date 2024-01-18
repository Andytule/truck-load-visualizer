using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BundlePreviewManager : MonoBehaviour
{
    public GameObject previewPanel;
    public Transform objectParent;
    public Text title;
    public Text infoBodyText;
    public Transform pickListContent;
    public ScrollRect scrollRect;
    public RectTransform contentPanel;


    GameObject obj;
    Button clickedButton;
    Bundle selectedBundle;
   

    private void Update()
    {
        if (previewPanel.activeSelf)
        {
            clickedButton.Select();
        }
    }

    //Hide the bundle preveiw window
    public void closePreview()
    {
        previewPanel.SetActive(false);
        Destroy(obj);
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
    }

    //Find the pick list button for the selected button
    public void findSelectedButton()
    {
        clickedButton = null;
        foreach (Transform t in pickListContent)
        {
            string text = t.gameObject.GetComponentInChildren<TMP_Text>().text;
            string name = selectedBundle.name;
            text = text.Substring(text.IndexOf(". ") + 2);
            if (text == name)
            {
                clickedButton = t.gameObject.GetComponent<Button>();
                SnapTo(t.gameObject.GetComponent<RectTransform>());

            }
        }
    }

    //Scroll the picklist so that the selected button is in view
    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        contentPanel.anchoredPosition =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position) - new Vector2(0,target.sizeDelta.y / 2);
    }

    //Show the bundle preview window
    public void previewBundle(Bundle b, Button btn = null)
    {
        selectedBundle = b;

        if (btn == null)
            findSelectedButton();
        else
            clickedButton = btn;

        Destroy(obj);

        previewPanel.SetActive(true);

        obj = Instantiate(b.gameObject, objectParent);
        obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, 1);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localPosition -= new Vector3(0, 0, (0 - (b.size.z / 2)) + 11.11f);

        setLayerRecursively(obj);
        title.text = b.name + " Preview";
        obj.tag = "PreviewBundle";

        Bundle previewBundle = obj.GetComponent<Bundle>();
        previewBundle.setBundleVisible(true);
        previewBundle.highlightBundle(false);

        infoBodyText.text = previewBundle.getInfo();

    }

    //Sets the layer for a gameobject and all its children
    public void setLayerRecursively(GameObject obj)
    {
        obj.layer = 5;
        foreach (Transform child in obj.transform)
        {
            setLayerRecursively(child.gameObject);
        }       
    }
}
