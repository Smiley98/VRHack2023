using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    public SelectionOutline selectionOutline;

    void Start()
    {
        selectionOutline = FindObjectOfType<SelectionOutline>();
    }

    public void OnHoverEnter()
    {
        Ray ray = selectionOutline.camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            selectionOutline.targetRenderer = GetComponent<Renderer>();
            if (selectionOutline.lastTarget == null)
            {
                selectionOutline.lastTarget = selectionOutline.targetRenderer;
            }
            if (selectionOutline.selectionMode == SelectionMode.PARENT_AND_CHILDREN)
            {
                if (selectionOutline.childRenderers != null)
                {
                    Array.Clear(selectionOutline.childRenderers, 0, selectionOutline.childRenderers.Length);
                }
                selectionOutline.childRenderers = GetComponentsInChildren<Renderer>();
            }

            if (selectionOutline.targetRenderer != selectionOutline.lastTarget || !selectionOutline.isSelected)
            {
                selectionOutline.SetTarget();
            }
            selectionOutline.lastTarget = selectionOutline.targetRenderer;
        }
        else
        {
            selectionOutline.targetRenderer = null;
            selectionOutline.lastTarget = null;
            if (selectionOutline.isSelected)
            {
                selectionOutline.ClearTarget();
            }
        }
    }

    public void OnHoverExit()
    {
        if (selectionOutline.isSelected)
        {
            selectionOutline.ClearTarget();
        }
    }
}
