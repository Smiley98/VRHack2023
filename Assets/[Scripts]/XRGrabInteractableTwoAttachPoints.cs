using SerializableCallback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableTwoAttachPoints : XRGrabInteractable
{
    public Transform leftAttachTransform;
    public Transform rightAttachTransform;
    public Hand hand;

    void Update()
    {
        switch (hand)
        {
            case Hand.LEFT:
                attachTransform = leftAttachTransform;
                break;
            case Hand.RIGHT:
                attachTransform = rightAttachTransform;
                break;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("Left Hand"))
        {
            hand = Hand.LEFT;
            attachTransform = leftAttachTransform;
        }
        else if (args.interactorObject.transform.CompareTag("Right Hand"))
        {
            hand = Hand.RIGHT;
            attachTransform = rightAttachTransform;
        }

        base.OnSelectEntered(args);
    }
}
