using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

public class GameLogic : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject placementIndicator;

    private ARSessionOrigin arOrigin;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private bool isGroundMade = false;

    [SerializeField]
    private Camera playerCamera;

    private bool buildModeOn = true;
    private bool canBuild = false;

    private BlockSystem bSys;

    [SerializeField]
    private LayerMask buildableSurfacesLayer;

    private Vector3 buildPos;

    private GameObject currentTemplateBlock;

    [SerializeField]
    private GameObject blockTemplatePrefab;
    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private Material templateMaterial;

    private int blockSelectCounter = 0;

    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        bSys = GetComponent<BlockSystem>();
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (Input.GetKeyDown("e"))
        {
            Debug.Log("Key Down : e, check1");
            buildModeOn = !buildModeOn;

            Debug.Log("Key Down : e, check2");
            if (buildModeOn)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            Debug.Log("Key Down : e, check3");
        }

        if (Input.GetKeyDown("r"))
        {
            Debug.Log("Key Down : r, check1");
            blockSelectCounter++;
            if (blockSelectCounter >= bSys.allBlocks.Count) blockSelectCounter = 0;
        }

        if (buildModeOn)
        {
            Debug.Log("Build Mode On, check1");
            RaycastHit buildPosHit;

            if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out buildPosHit, 10, buildableSurfacesLayer))
            {
                Debug.Log("Build Mode On, check2");
                Vector3 point = buildPosHit.point;
                Debug.Log("x: " + point.x + " y: " + point.y + " z: " + point.z);
                buildPos = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y + (float)0.01), Mathf.Round(point.z));
                canBuild = true;
            }
            else
            {
                Debug.Log("Build Mode On, Destroy 1");
                if (currentTemplateBlock != null) Destroy(currentTemplateBlock.gameObject);
                canBuild = false;
            }
        }

        if (!buildModeOn && currentTemplateBlock != null)
        {
            Debug.Log("Build Mode Off, Destroy template Block");
            Destroy(currentTemplateBlock.gameObject);
            canBuild = false;
        }

        if (canBuild && currentTemplateBlock == null)
        {
            currentTemplateBlock = Instantiate(blockTemplatePrefab, buildPos, Quaternion.identity);
            currentTemplateBlock.GetComponent<MeshRenderer>().material = templateMaterial;
        }

        if (canBuild && currentTemplateBlock != null)
        {
            currentTemplateBlock.transform.position = buildPos;

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBlock();
            }
        }

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (!isGroundMade) {
            Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
            isGroundMade = true;
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
        if (isGroundMade)
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        arOrigin.Raycast(screenCenter, hits, UnityEngine.Experimental.XR.TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(Mathf.Round(cameraForward.x), 0, Mathf.Round(cameraForward.z)).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void PlaceBlock()
    {
        if (isGroundMade)
        {
            Debug.Log("Plcae Block");
            GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
            Block tempBlock = bSys.allBlocks[blockSelectCounter];
            newBlock.name = tempBlock.blockName + "-Block";
            newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        }
    }
}