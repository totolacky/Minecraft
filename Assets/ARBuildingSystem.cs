using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARBuildingSystem : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    private bool buildModeOn = false;
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

    private void Start()
    {
        bSys = GetComponent<BlockSystem>();
    }

    private void Update()
    {
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
                Debug.Log("x: "+point.x+" y: "+point.y+" z: "+point.z);
                buildPos = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y+(float)0.01), Mathf.Round(point.z));
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
    }

    private void PlaceBlock()
    {
        Debug.Log("Plcae Block");
        GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
        Block tempBlock = bSys.allBlocks[blockSelectCounter];
        newBlock.name = tempBlock.blockName + "-Block";
        newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
    }
}