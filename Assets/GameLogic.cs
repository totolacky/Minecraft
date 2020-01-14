using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private bool bombModeOn = false;
    private bool canBuild = false;
    private bool large = false;

    private BlockSystem bSys;

    [SerializeField]
    private LayerMask buildableSurfacesLayer;

    [SerializeField]
    private LayerMask bomb_scale;

    private Vector3 buildPos;
    private Vector3 bombPos;

    private GameObject currentTemplateBlock;

    [SerializeField]
    private GameObject blockTemplatePrefab;
    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private GameObject blockPrefab_big;

    [SerializeField]
    private Material templateMaterial;

    [SerializeField]
    private Material temp_del_Material;

    [HideInInspector]
    public Dictionary<Vector3, GameObject> created_block = new Dictionary<Vector3, GameObject>();

    [HideInInspector]
    public Dictionary<Vector3, GameObject> created_block_big = new Dictionary<Vector3, GameObject>();

    [HideInInspector]
    public Dictionary<Vector3, int> created_block_mat = new Dictionary<Vector3, int>();

    private int blockSelectCounter = 0;

    private Vector3 ground_pos;

    public AudioSource bomb_src;
    public AudioSource bomb_src2;
    public AudioSource place;
    public AudioClip place_1;
    public AudioClip place_2;
    public AudioClip place_3;
    public AudioClip place_4;
    public GameObject bomb_obj;

    [SerializeField]
    public GameObject canvas;
    private Boolean UIVisible = true;

    [SerializeField]
    private static string worldName;
    
    private GameObject ground;
    [Serializable]
    public class World
    {
        //public Dictionary<int, Block> blockType;
        public string worldName;
        public List<BlockInfo> blocks;
        public World(string name)
        {
            //blockType = new Dictionary<int, Block>();
            worldName = name;
            blocks = new List<BlockInfo>();
        }
    }
    [Serializable]
    public class BlockInfo
    {
        public float x;
        public float y;
        public float z;
        public int mat;
        public BlockInfo(Vector3 _pos, int _mat)
        {
            x = _pos.x;
            y = _pos.y;
            z = _pos.z;
            mat = _mat;
        }
    }

    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        bSys = GetComponent<BlockSystem>();
        playerCamera.farClipPlane = 200;
    }

    void Update()
    {
        if(!UIVisible && Input.GetMouseButtonDown(0))
        {
            show_UI();
            return;
        }

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (!large)
        {
            if (buildModeOn || destroyModeOn || bombModeOn)
            {
                RaycastHit buildPosHit;

                if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out buildPosHit, 10, buildableSurfacesLayer))
                {
                    Vector3 point = buildPosHit.point;
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
                    canBuild = true;
                }
                else
                {
                    if (currentTemplateBlock != null) Destroy(currentTemplateBlock.gameObject);
                    canBuild = false;
                }
            }

            if (!(buildModeOn || destroyModeOn || bombModeOn) && currentTemplateBlock != null)
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
        }

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceGround();
        }
    }

    private void PlaceGround()
    {
        if (!isGroundMade)
        {
            var position = new Vector3(Mathf.Round(placementPose.position.x), Mathf.Round(placementPose.position.y) - (float)1, Mathf.Round(placementPose.position.z));
            ground = Instantiate(objectToPlace, position, placementPose.rotation);
            ground_pos = position;
            isGroundMade = true;

            load_block();
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
        if(Camera.current == null)
        {
            return;
        }
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        arOrigin.Raycast(screenCenter, hits, UnityEngine.Experimental.XR.TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.identity;
        }
    }

    private void PlaceBlock()
    {
        int sound = UnityEngine.Random.Range(0, 4);
        switch (sound)
        {
            case 0:
                place.clip = place_1;
                break;
            case 1:
                place.clip = place_2;
                break;
            case 2:
                place.clip = place_3;
                break;
            case 3:
                place.clip = place_4;
                break;
        }
        place.Play();
        GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
        Block tempBlock = bSys.allBlocks[blockSelectCounter];
        newBlock.name = tempBlock.blockName + "-Block";
        newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        created_block.Add(buildPos, newBlock);
        created_block_mat.Add(buildPos, blockSelectCounter);

        Debug.Log("Minecraft PlaceBlock > Total block : " + created_block.Count + ", created_block" + buildPos);
    }
    public void screen_touch()
    {
        if (!large)
        {
            if (canBuild && currentTemplateBlock != null)
            {
                currentTemplateBlock.transform.position = buildPos;
                if (buildModeOn) PlaceBlock();
                else if (destroyModeOn) DeleteBlock();
                else BombBlock();
            }
        }
    }
    private void DeleteBlock()
    {
        int sound = UnityEngine.Random.Range(0, 4);
        switch (sound)
        {
            case 0:
                place.clip = place_1;
                break;
            case 1:
                place.clip = place_2;
                break;
            case 2:
                place.clip = place_3;
                break;
            case 3:
                place.clip = place_4;
                break;
        }
        place.Play();
        GameObject newBlock = created_block[buildPos];
        Destroy(newBlock);
        created_block.Remove(buildPos);
        created_block_mat.Remove(buildPos);
    }

    private void BombBlock()
    {
        StartCoroutine("bomb");
    }
    IEnumerator bomb()
    {
        Vector3 bomb_pos = buildPos;
        bomb_src.Play();
        yield return new WaitForSeconds(2);
        bomb_src2.Play();
        GameObject bomb = Instantiate(bomb_obj, bomb_pos, Quaternion.identity);
        Destroy(bomb, 2);
        float radius = 2f;
        Debug.Log("Minecraft Bomb > start");
        Collider[] colliders = Physics.OverlapSphere(bomb_pos, radius, bomb_scale);
        Debug.Log("Minecraft Bomb > " + colliders);
        int i = 0;
        while (i < colliders.Length)
        {
            Debug.Log("Minecraft Bomb > " + colliders[i].transform.position);
            GameObject newBlock = created_block[colliders[i].transform.position];
            Destroy(newBlock);
            created_block.Remove(colliders[i].transform.position);
            created_block_mat.Remove(colliders[i].transform.position);
            i++;
        }

    }

    public void create_mode()
    {
        buildModeOn = true;
        destroyModeOn = false;
        bombModeOn = false;
        currentTemplateBlock.GetComponent<MeshRenderer>().material = templateMaterial;
    }

    public void delete_mode() // Destroy Mode
    {
        buildModeOn = false;
        destroyModeOn = true;
        bombModeOn = false;
        currentTemplateBlock.GetComponent<MeshRenderer>().material = temp_del_Material;
    }

    public void bomb_mode()
    {
        buildModeOn = false;
        destroyModeOn = false;
        bombModeOn = true;
        currentTemplateBlock.GetComponent<MeshRenderer>().material = temp_del_Material;
    }
    public void change_block() // Change block material
    {
        blockSelectCounter++;
        if (blockSelectCounter >= bSys.allBlocks.Count) blockSelectCounter = 0;
    }

    public void save_block()
    {
        Debug.Log("Saved");
        if (large) scale_block();
        PlayerPrefs.DeleteKey("W:" + worldName);
        List<Vector3> block_list = new List<Vector3>(created_block.Keys);
        var world = new World(worldName);
        for (int i = 0; i < created_block.Count; i++)
        {
            world.blocks.Add(new BlockInfo(new Vector3(block_list[i].x - ground_pos.x, block_list[i].y - ground_pos.y, block_list[i].z - ground_pos.z), created_block_mat[block_list[i]]));
        }
        PlayerPrefs.SetString("W:" + worldName, MyUtil.ObjectToString(world));
        var worlds = (ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds"));
        if (!worlds.Contains(worldName))
        {
            worlds.Add(worldName);
            PlayerPrefs.DeleteKey("worlds");
            PlayerPrefs.SetString("worlds", MyUtil.ObjectToString(worlds));
        }
        PlayerPrefs.Save();
        MyUtil.PrintArrayList((ArrayList)MyUtil.StringToObject(PlayerPrefs.GetString("worlds")));
    }
    public void load_block()
    {
        large = false;
        if (created_block.Count == 0)
        {
            var world = (World)MyUtil.StringToObject(PlayerPrefs.GetString("W:" + worldName));
            for (int i = 0; i < world.blocks.Count; i++)
            {
                buildPos = new Vector3(
                    world.blocks[i].x + ground_pos.x,
                    world.blocks[i].y + ground_pos.y,
                    world.blocks[i].z + ground_pos.z);
                GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
                Block tempBlock = bSys.allBlocks[world.blocks[i].mat];
                newBlock.name = tempBlock.blockName + "-Block";
                newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
                created_block.Add(buildPos, newBlock);
                created_block_mat.Add(buildPos, world.blocks[i].mat);
            }
        }
    }
    public void clear_block()
    {
        Debug.Log("Minecraft clear_Block > Total block : " + created_block.Count);
        List<Vector3> block_list = new List<Vector3>(created_block.Keys);


        for (int i = 0; i < block_list.Count; i++)
        {
            Debug.Log("Minecraft clear_Block > for Delete block : " + i);
            GameObject newBlock = created_block[block_list[i]];
            Destroy(newBlock);
            created_block.Remove(block_list[i]);
            created_block_mat.Remove(block_list[i]);
        };

    }
    public void scale_block()
    {
        if (!isGroundMade)
        {
            return;
        }
        float scale = 5;
        if (large)
        {
            large = false;
            List<Vector3> block_list = new List<Vector3>(created_block_big.Keys);
            for (int i = 0; i < block_list.Count; i++)
            {
                //Get new position of block
                Vector3 new_pos = block_list[i];
                GameObject oldBlock = created_block_big[block_list[i]];

                new_pos.x = (new_pos.x + ((scale - (float)1) * ground_pos.x)) / scale;

                if (new_pos.y > ground_pos.y) new_pos.y = (new_pos.y + ((scale - (float)1) * ground_pos.y) + ((scale - (float)1) / (float)2)) / scale;
                else new_pos.y = (new_pos.y + ((scale - (float)1) * ground_pos.y) - ((scale - (float)1) / (float)2)) / scale;

                new_pos.z = (new_pos.z + ((scale - (float)1) * ground_pos.z)) / scale;


                //Make small block
                GameObject newBlock = Instantiate(blockPrefab, new_pos, Quaternion.identity);
                Block tempBlock = bSys.allBlocks[created_block_mat[new_pos]];
                newBlock.name = tempBlock.blockName + "-Block";
                newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
                created_block.Add(new_pos, newBlock);

                //Delete big block
                Destroy(oldBlock);
                created_block_big.Remove(block_list[i]);
            }
            ground.transform.localScale = new Vector3(ground.transform.localScale.x / scale, 1, ground.transform.localScale.z / scale);
        }
        else
        {
            large = true;
            List<Vector3> block_list = new List<Vector3>(created_block.Keys);
            for (int i = 0; i < block_list.Count; i++)
            {
                //Get new position of block
                Vector3 new_pos = block_list[i];
                GameObject oldBlock = created_block[block_list[i]];

                new_pos.x = scale * (new_pos.x - ground_pos.x) + ground_pos.x;

                if (new_pos.y > ground_pos.y) new_pos.y = scale * (new_pos.y - ground_pos.y) + (float)0.5 + ground_pos.y - (scale / (float)2);
                else new_pos.y = scale * (new_pos.y - ground_pos.y) - (float)0.5 + ground_pos.y + (scale / (float)2);

                new_pos.z = scale * (new_pos.z - ground_pos.z) + ground_pos.z;

                //Make big block
                GameObject newBlock = Instantiate(blockPrefab_big, new_pos, Quaternion.identity);
                Block tempBlock = bSys.allBlocks[created_block_mat[block_list[i]]];
                newBlock.name = tempBlock.blockName + "-Block";
                newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
                created_block_big.Add(new_pos, newBlock);

                //Delete small block
                Destroy(oldBlock);
                created_block.Remove(block_list[i]);
            }
            ground.transform.localScale = new Vector3(ground.transform.localScale.x * scale, 1, ground.transform.localScale.z * scale);
        }
    }
    public void hide_UI()
    {
        canvas.SetActive(false);
        UIVisible = false;
    }
    public void show_UI()
    {
        canvas.SetActive(true);
        UIVisible = true;
    }
    public void to_home()
    {
        SceneManager.LoadScene("World choice scene");
    }
    public void setWorldName(string newName)
    {
        worldName = newName;
    }

    public void mat_01()
    {
        blockSelectCounter = 00;
    }
    public void mat_02()
    {
        blockSelectCounter = 01;
    }
    public void mat_03()
    {
        blockSelectCounter = 02;
    }
    public void mat_04()
    {
        blockSelectCounter = 03;
    }
    public void mat_05()
    {
        blockSelectCounter = 04;
    }
    public void mat_06()
    {
        blockSelectCounter = 05;
    }
    public void mat_07()
    {
        blockSelectCounter = 06;
    }
    public void mat_08()
    {
        blockSelectCounter = 07;
    }
    public void mat_09()
    {
        blockSelectCounter = 08;
    }
    public void mat_10()
    {
        blockSelectCounter = 09;
    }
    public void mat_11()
    {
        blockSelectCounter = 10;
    }
}