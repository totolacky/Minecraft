﻿using System.Collections;
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

    private bool buildModeOn = false;
    private bool destroyModeOn = false;
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

    [SerializeField]
    private Material temp_del_Material;

    [HideInInspector]
    public Dictionary<Vector3, GameObject> created_block = new Dictionary<Vector3, GameObject>();

    private int blockSelectCounter = 0;

    //changed!
    private Boolean scaled_large = false;

    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        bSys = GetComponent<BlockSystem>();
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        if (buildModeOn || destroyModeOn)
        {
            RaycastHit buildPosHit;

            if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out buildPosHit, 10, buildableSurfacesLayer))
            {
                Vector3 point = buildPosHit.point;
                //Debug.Log("Camera position : "+playerCamera.transform.position);
                //Debug.Log("Raycast position : "+point);
                float pos_x = Mathf.Round(point.x);
                float pos_y = Mathf.Round(point.y + (float)0.05);
                float pos_z = Mathf.Round(point.z);
                if (Mathf.Round(point.x + (float)0.05) != Mathf.Round(point.x - (float)0.05))
                {
                    if ((playerCamera.transform.position.x - point.x) > 0)
                    {
                        if (buildModeOn) pos_x = Mathf.Round(point.x + (float)0.05);
                        else pos_x = Mathf.Round(point.x - (float)0.05);
                    }
                    else
                    {
                        if (buildModeOn) pos_x = Mathf.Round(point.x - (float)0.05);
                        else pos_x = Mathf.Round(point.x + (float)0.05);
                    }
                }
                if (Mathf.Round(point.y + (float)0.05) != Mathf.Round(point.y - (float)0.05))
                {
                    if ((playerCamera.transform.position.y - point.y) > 0)
                    {
                        if (buildModeOn) pos_y = Mathf.Round(point.y + (float)0.05);
                        else pos_y = Mathf.Round(point.y - (float)0.05);
                    }
                    else
                    {
                        if (buildModeOn) pos_y = Mathf.Round(point.y - (float)0.05);
                        else pos_y = Mathf.Round(point.y + (float)0.05);
                    }
                }
                if (Mathf.Round(point.z + (float)0.05) != Mathf.Round(point.z - (float)0.05))
                {
                    if ((playerCamera.transform.position.z - point.z) > 0)
                    {
                        if (buildModeOn) pos_z = Mathf.Round(point.z + (float)0.05);
                        else pos_z = Mathf.Round(point.z - (float)0.05);
                    }
                    else
                    {
                        if (buildModeOn) pos_z = Mathf.Round(point.z - (float)0.05);
                        else pos_z = Mathf.Round(point.z + (float)0.05);
                    }
                }
                buildPos = new Vector3(pos_x, pos_y, pos_z);
                //Debug.Log("Block position : "+buildPos);
                canBuild = true;
            }
            else
            {
                if(currentTemplateBlock != null) Destroy(currentTemplateBlock.gameObject);
                canBuild = false;
            }
        }

        if (!(buildModeOn || destroyModeOn) && currentTemplateBlock != null)
        {
            Destroy(currentTemplateBlock.gameObject);
            canBuild = false;
        }

        if (canBuild && currentTemplateBlock == null)
        {
            currentTemplateBlock = Instantiate(blockTemplatePrefab, buildPos, Quaternion.identity);
            if (buildModeOn) currentTemplateBlock.GetComponent<MeshRenderer>().material = templateMaterial;
            else currentTemplateBlock.GetComponent<MeshRenderer>().material = temp_del_Material;
        }
        if (canBuild && currentTemplateBlock != null)
        {
            currentTemplateBlock.transform.position = buildPos;
        }

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (!isGroundMade) {
            var position = new Vector3(Mathf.Round(placementPose.position.x), Mathf.Round(placementPose.position.y) - (float)1, Mathf.Round(placementPose.position.z));
            Debug.Log("placementPose position: ("+ placementPose.position.x +"," + placementPose.position.y + "," + placementPose.position.z + ")");
            Debug.Log("position: (" + position.x + "," + position.y + "," + position.z + ")");
            Instantiate(objectToPlace, position, placementPose.rotation);
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
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void PlaceBlock()
    {
        GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
        Block tempBlock = bSys.allBlocks[blockSelectCounter];
        newBlock.name = tempBlock.blockName + "-Block";
        newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        created_block.Add(buildPos, newBlock);
    }
    private void DeleteBlock()
    {
        GameObject newBlock = created_block[buildPos];
        Destroy(newBlock);
        created_block.Remove(buildPos);
    }
    public void create_mode()
    {
        buildModeOn = !buildModeOn;
        currentTemplateBlock.GetComponent<MeshRenderer>().material = templateMaterial;
        if (buildModeOn) destroyModeOn = false;
    }

    public void delete_mode() // Destroy Mode
    {
        destroyModeOn = !destroyModeOn;
        currentTemplateBlock.GetComponent<MeshRenderer>().material = temp_del_Material;
        if (destroyModeOn) buildModeOn = false;
    }
    public void change_block() // Change block material
    {
        blockSelectCounter++;
        if (blockSelectCounter >= bSys.allBlocks.Count) blockSelectCounter = 0;
    }
    public void screen_touch()
    {
        if (canBuild && currentTemplateBlock != null)
        {
            currentTemplateBlock.transform.position = buildPos;
            if (buildModeOn) PlaceBlock();
            else DeleteBlock();
        }
    }

    //changed!
    public void change_scale()
    {
        if(scaled_large)
        {
            // 작은 사이즈로 축소하기
            arOrigin.transform.localScale = Vector3.one * 10;
            scaled_large = !scaled_large;
        }
        else
        {
            // 큰 사이즈로 확대하기
            arOrigin.transform.localScale = Vector3.one * 1;
            scaled_large = !scaled_large;
        }
    }
}