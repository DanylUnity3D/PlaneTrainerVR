﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class Selector : MonoBehaviour
{
    public bool isActive;

    private Vector3 startingPosition;
    private Renderer myRenderer;

    public Material defaultMaterial;
    public Material gazedActiveMaterial;
    public Material gazedInactiveMaterial;

    //private Transform textMeshTransform;

    void Start()
    {
        startingPosition = transform.localPosition;
        myRenderer = GetComponent<Renderer>();
        //textMeshTransform = GetComponentInChildren<TextMesh>().transform;
    }

    private void Update()
    {
        //textMeshTransform.LookAt(SequenceManager.Instance.playerCamera.transform);
    }

    private Coroutine gazedAtActiveCoroutine;

    public void SetGazedAt(bool gazedAt)
    {
        if (defaultMaterial != null && gazedActiveMaterial != null && gazedInactiveMaterial != null)
        {
            if (gazedAt)
            {
                defaultMaterial = myRenderer.materials[0];

                if (isActive)
                {
                    myRenderer.material = gazedActiveMaterial;
                    gazedAtActiveCoroutine = StartCoroutine(GazedAtRoutine(1f));
                }
                else
                {
                    myRenderer.material = gazedInactiveMaterial;
                }
            }
            else
            {
                if (gazedAtActiveCoroutine != null)
                {
                    StopCoroutine(gazedAtActiveCoroutine);
                }

                myRenderer.material = defaultMaterial;
            }
        }
    }

    private IEnumerator GazedAtRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SequenceManager.Instance.MakeNextSelectorActive();
    }

    public void Reset()
    {
        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        for (int i = 0; i < numSibs; i++)
        {
            GameObject sib = transform.parent.GetChild(i).gameObject;
            sib.transform.localPosition = startingPosition;
            sib.SetActive(i == sibIdx);
        }
    }

    public void Recenter()
    {
#if !UNITY_EDITOR
      GvrCardboardHelpers.Recenter();
#else
        if (GvrEditorEmulator.Instance != null)
        {
            GvrEditorEmulator.Instance.Recenter();
        }
#endif // !UNITY_EDITOR
    }

    public void SetClicked(BaseEventData eventData)
    {
        // Only trigger on left input button, which maps to
        // Daydream controller TouchPadButton and Trigger buttons.
        PointerEventData ped = eventData as PointerEventData;
        if (ped != null)
        {
            if (ped.button != PointerEventData.InputButton.Left)
            {
                return;
            }
        }

        // Pick a random sibling, move them somewhere random, activate them,
        // deactivate ourself.
        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
        GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

        // Move to random new location ±90˚ horzontal.
        Vector3 direction = Quaternion.Euler(
                                0,
                                Random.Range(-90, 90),
                                0) * Vector3.forward;
        // New location between 1.5m and 3.5m.
        float distance = 2 * Random.value + 1.5f;
        Vector3 newPos = direction * distance;
        // Limit vertical position to be fully in the room.
        newPos.y = Mathf.Clamp(newPos.y, -1.2f, 4f);
        randomSib.transform.localPosition = newPos;

        randomSib.SetActive(true);
        gameObject.SetActive(false);
        SetGazedAt(false);
    }
}