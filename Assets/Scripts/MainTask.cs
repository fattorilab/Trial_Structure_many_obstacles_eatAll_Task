using System;
using System.Collections;
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using PupilLabs;

/*
NOTICE:
- Position is not reset after collecting single target, but only when all are collected;
- Number of trials is increased only after collecting all target; 
 */



public class MainTask : MonoBehaviour
{
    #region Variables Declaration

    #region GameObjects and components

    [SerializeField]

    [HideInInspector]
    [Header("GameObjects and components")]

    // Cams
    [System.NonSerialized] Camera camM;
    [System.NonSerialized] Camera camL;
    [System.NonSerialized] Camera camR;

    // Pupil
    [System.NonSerialized] public PupilDataStream PupilDataStreamScript;
    private RequestController RequestControllerScript;
    private bool PupilDataConnessionStatus;

    // Game
    Rigidbody player_rb;
    [HideInInspector] GameObject environment;
    [HideInInspector] GameObject experiment;
    [HideInInspector] GameObject player;

    // Black pixels (for scripts syncing)
    private GameObject markerObject_M;
    private GameObject markerObject_R;
    private GameObject markerObject_L;

    #endregion

    #region Saving Info

    [Header("Saving Info")]
    public string MEF;
    public string path_to_data = "C:/Users/admin/Desktop/Registrazioni_VR/";
    [System.NonSerialized] public int lastIDFromDB;
    private string identifier;
    [HideInInspector] public int seed;
    [HideInInspector] public long starttime = 0;
    [HideInInspector] public int frame_number = 0;

    #endregion

    #region Reward Info

    [Header("Reward")]
    public int RewardLength = 50;
    private float RewardLength_in_sec;
    public int reward_counter = 0;

    #endregion

    #region Trials Info

    [Header("Trials Info")]

    // Trials
    public int trials_win;
    public int taken_targets;
    public int trials_lose;
    [System.NonSerialized] public int current_trial;

    // States
    public int current_state;
    [System.NonSerialized] public int last_state;
    [System.NonSerialized] public string error_state;

    // Conditions
    [System.NonSerialized] public int current_condition = -1;

    // Tracking events
    private float lastevent;
    private bool first_frame;

    // Moving timer
    private static bool isMoving = false;
    private static Diagnostics.Stopwatch stopwatch = new Diagnostics.Stopwatch();

    #endregion

    #region Target and Obstacles Info

    [Header("Targets and Obstacles")]

    public int numTargets = 8;
    public int obst_perTarget = 1;
    public int numObstacles;
    private Vector2 location_grid_limitX = new Vector2(-10f, 10f);
    private Vector2 location_grid_limitZ = new Vector2(0, 20);
    private int numCellsinGrid = 4;
    private Vector2 forbiddenZoneX = new Vector2(-1, 1);
    private Vector2 forbiddenZoneZ = new Vector2(0, 2);
    public List<Vector2> possibleLocations;
    private bool randomizePosition = true;

    [System.NonSerialized] public GameObject TargetPrefab;
    GameObject[] targets;
    public int rewardedTargets = 0;

    // Materials
    [System.NonSerialized] public Material initial_grey;
    [System.NonSerialized] public Material red;
    [System.NonSerialized] public Material green_dot;
    [System.NonSerialized] public Material red_dot;
    [System.NonSerialized] public Material final_grey;
    [System.NonSerialized] public Material white;

    #endregion

    #region Epochs Info

    [Header("Epochs Info")]
    public float BASELINE_duration = 2f;
    public float INTERTRIAL_duration = 2f;
    public float MOVEMENT_maxduration = 6f;
    public float RT_maxduration = 2f;

    #endregion

    #region Arduino Info

    [Header("Arduino Info")]
    [System.NonSerialized] public Ardu ardu;
    [System.NonSerialized] public float arduX;
    [System.NonSerialized] public float arduY;

    #endregion

    #region PupilLab Info

    [Header("PupilLab Info")]
    [System.NonSerialized] public Vector2 centerRightPupilPx = new Vector2(float.NaN, float.NaN);
    [System.NonSerialized] public Vector2 centerLeftPupilPx = new Vector2(float.NaN, float.NaN);
    [System.NonSerialized] public float diameterRight = float.NaN;
    [System.NonSerialized] public float diameterLeft = float.NaN;
    [System.NonSerialized] public bool pupilconnection;

    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Generate random seed
        System.Random rand = new System.Random();
        seed = rand.Next();

        // Setup
        UnityEngine.Random.InitState(seed);
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        first_frame = true;

        // States
        current_state = -2;
        last_state = -2;
        error_state = "";

        // Trials
        current_trial = 0;
        trials_win = 0;
        taken_targets = 0;
        trials_lose = 0;
   
        // GameObjects
        ardu = GetComponent<Ardu>(); 
        player = GameObject.Find("Player");
        player_rb = player.GetComponent<Rigidbody>();
        experiment = GameObject.Find("Experiment");
        environment = GameObject.Find("Environment");

        // Init cameras
        camM = GameObject.Find("Main Camera").GetComponent<Camera>();
        camL = GameObject.Find("Left Camera").GetComponent<Camera>();
        camR = GameObject.Find("Right Camera").GetComponent<Camera>();

        // PupilLab
        PupilDataStreamScript = GameObject.Find("PupilDataManagment").GetComponent<PupilDataStream>();
        RequestControllerScript = GameObject.Find("PupilDataManagment").GetComponent<RequestController>();

        // Materials
        initial_grey = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/neutralgrey.mat");
        red = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/red_fruit.mat");
        green_dot = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/green_dot.mat");
        red_dot = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/reddot.mat");
        final_grey = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/grey_fruit.mat");
        white = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/fruit/white_fruit.mat");

        // Target Prefab
        TargetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Material/Fruit_Prefab.prefab");

        // Black pixels (markers for scripts syncing)
        markerObject_M = GameObject.CreatePrimitive(PrimitiveType.Quad);
        CreateMarkerBlack(markerObject_M, camM);
        markerObject_R = GameObject.CreatePrimitive(PrimitiveType.Quad);
        CreateMarkerBlack(markerObject_R, camL);
        markerObject_L = GameObject.CreatePrimitive(PrimitiveType.Quad);
        CreateMarkerBlack(markerObject_L, camR);

    }

    void Update()
    {
        frame_number++;

        RewardLength_in_sec = RewardLength / 1000f;

        // Start on first operating frame
        if (first_frame) 
        {
            Debug.Log("START TASK");
            // Start time main task unity
            starttime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            // Send START trigger
            ardu.SendStartRecordingOE();           
            first_frame = false;

            // Disable movement
            player.GetComponent<Movement>().allow_backwards = 0;
            player.GetComponent<Movement>().allow_forwards = 0;
            player.GetComponent<Movement>().allow_horizontal = 0;
        }

        // Check if the player is moving the joystick
        isMoving = player.GetComponent<Movement>().keypressed;

        // Manual reward
        if (Input.GetKeyDown("space")) { ardu.SendReward(RewardLength); }
        reward_counter = ardu.reward_counter;

        #region StateMachine

        switch (current_state)
        {
            case -2: // TASK BEGIN

                if (PupilDataStreamScript.subsCtrl.IsConnected || RequestControllerScript.ans)
                {
                    current_state = -1;
                }

                break;

            case -1: // INTERTRIAL

                #region State Beginning (executed once upon entering)

                if (last_state != current_state)
                {
                    Debug.Log($"Current state: {current_state}");

                    current_condition = -1;

                    // Switch ON black pixels objects
                    showMarkerBlack(markerObject_M);
                    showMarkerBlack(markerObject_R);
                    showMarkerBlack(markerObject_L);

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";

                }
                #endregion

                #region State Body (executed every frame while in state)

                current_condition = -1;

                #endregion

                #region State End (executed once upon exiting)
                if ((Time.time - lastevent) > INTERTRIAL_duration)
                {
                    // Switch OFF black pixels objects
                    hideMarkerBlack(markerObject_M);
                    hideMarkerBlack(markerObject_R);
                    hideMarkerBlack(markerObject_L);

                    // Instantiate rocks
                    // if desired, randomize disposition of rocks and targets
                    if (randomizePosition)
                    {
                        RandomizeEnvWithGrid();
                    }

                    // Change target material 
                    for (int i = 0; i < targets.Length; i++)
                    {
                        changeTargetMaterial(targets[i], red);
                    }

                    // Move to state 0
                    current_state = 0;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 0: // BASELINE

                #region State Beginning (executed once upon entering)

                if (last_state != current_state)
                {
                    Debug.Log($"Current state: {current_state}");

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";

                    // Make targets and obstacles visible
                    showTargets(targets);
                    environment.GetComponent<Rocks>().showRocks();

                }
                #endregion

                #region State Body (executed every frame while in state)

                #endregion

                #region State End (executed once upon exiting)
                if (!isMoving && ((Time.time - lastevent) > BASELINE_duration))
                {
                    // Enable movement
                    player.GetComponent<Movement>().allow_backwards = 1;
                    player.GetComponent<Movement>().allow_forwards = 1;
                    player.GetComponent<Movement>().allow_horizontal = 1;

                    // Go to MOVEMENT state
                    current_state = 1;

                    // Trial starts
                    current_trial++;

                }
                #endregion

                break;


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 1: // MOVEMENT

                #region State Beginning
                if (last_state != current_state)
                {
                    Debug.Log($"Current state: {current_state}");

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";

                }
                #endregion

                #region State Body
                if (player.GetComponent<Movement>().HasCollided) // If collision happened
                {

                    // Check if collided object is a target or not
                    bool isTarget = Regex.IsMatch(player.GetComponent<Movement>().CollidedObject.name, @"Fruit_Prefab_\d+"); ;
                    if (isTarget)
                    {
                        // Change target material
                        for (int i = 0; i < targets.Length; i++)
                        {
                            if (targets[i].name == player.GetComponent<Movement>().CollidedObject.name)
                            {
                                // Not already taken
                                if (targets[i].GetComponent<MeshRenderer>().material.mainTexture != white.mainTexture)
                                {

                                    changeTargetMaterial(targets[i], final_grey);

                                    // Go to RT state
                                    current_state = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        error_state = $"ERR: touched obstacle at {player.GetComponent<Movement>().CollidedObject.transform.position}";
                        current_state = -99;
                    }

                }
                #endregion

                #region State End
                if ((Time.time - lastevent) >= MOVEMENT_maxduration)
                {
                    error_state = "ERR: Not Finding Target in MOVEMENT";
                    current_state = -99;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 2: // RT

                #region State Beginning
                if (last_state != current_state)
                {
                    Debug.Log($"Current state: {current_state}");

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";
                }
                #endregion

                #region State Body
                // MEF stops moving
                if (!isMoving)
                {
                    // Change state

                    // If this is the last target to collect
                    if (rewardedTargets == (targets.Length - 1))
                    {
                        // Go to WIN ALL state
                        current_state = 99;
                    }
                    else
                    {
                        // Go to WIN ONE state
                        current_state = 98;
                    }
                }


                // If player exits the collision (i.e. contact time lower than reaction time)
                if (!player.GetComponent<Movement>().HasCollided)
                {
                    error_state = "ERR: Collision ended early in 2nd RT";
                    current_state = -99;
                }

                #endregion

                #region State End

                if ((Time.time - lastevent) >= RT_maxduration)
                {
                    error_state = "ERR: Keeps moving in 2nd RT";
                    current_state = -99;
                }
         
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case -99: //ERROR

                #region State Beginning
                if (last_state != current_state)
                {

                    Debug.Log($"Current state: {current_state}");
                    Debug.Log(error_state);

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                }
                #endregion

                #region State Body

                #endregion

                #region State End
                if (true)
                {
                    // Reset
                    reset_lose();

                    // Go to BASELINE
                    current_state = 0;

                    // Reset error string
                    error_state = "";

                    // Disable targets and obstacles
                    hideTargets(targets);
                    environment.GetComponent<Rocks>().hideRocks();

                    // Do not randomize rocks' and targets' positions
                    randomizePosition = false;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 98: // WIN FOR SINGLE TARGET

                #region State Beginning
                if (last_state != current_state)
                {
                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;

                    // Change target material to the material of reward
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i].name == player.GetComponent<Movement>().CollidedObject.name)
                        {
                            changeTargetMaterial(targets[i], white);
                        }
                    }

                    // Send reward for the single target
                    ardu.SendReward(RewardLength);

                    // Increase number of collected targets
                    taken_targets += 1;
                    rewardedTargets += 1;

                }
                #endregion

                #region State Body

                #endregion

                #region State End
                if ((Time.time - lastevent) >= RewardLength_in_sec)
                {
                    // Go to MOVEMENT state, keep playing
                    current_state = 1;

                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 99: // WIN FOR ALL TARGETS

                #region State Beginning
                if (last_state != current_state)
                {
                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;

                    // Change target material to the material of reward
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i].name == player.GetComponent<Movement>().CollidedObject.name)
                        {
                            changeTargetMaterial(targets[i], white);
                        }
                    }

                    // Send reward for the single target
                    ardu.SendReward(RewardLength);

                    // Increase number of collected targets
                    taken_targets += 1;

                }
                #endregion

                #region State Body

                #endregion

                #region State End
                if ((Time.time - lastevent) >= RewardLength_in_sec)
                {
                    // All target taken, trial ends
                    Debug.Log("TRIAL DONE");

                    // Reset win
                    reset_win();

                    // Randomize rocks' and targets' positions
                    randomizePosition = true;

                    // Go to INTERTRIAL state
                    current_state = -1;

                }
                #endregion

                break;

        }

        #endregion

    }

    #region Methods

    #region Quit

    void OnApplicationQuit()
    {
        // Destroy black pixels objects
        Destroy(markerObject_M);
        Destroy(markerObject_R);
        Destroy(markerObject_L);

        ardu.SendStopRecordingOE();
        Debug.Log("END OF SESSION");
        QuitGame();
    }

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Reset

    void reset_win()
    {
        // Reset collision
        player.GetComponent<Movement>().resetCollision();

        // Reset position
        reset_position();

        // Disable targets
        hideTargets(targets);

        // Delete current rocks
        environment.GetComponent<Rocks>().deleteRocks();

        // Reset number of collected targets
        rewardedTargets = 0;

        // Count trial
        trials_win++;
    }

    void reset_lose()
    {
        // Reset collision
        player.GetComponent<Movement>().resetCollision();

        // Reset position
        reset_position();

        // Count trial
        trials_lose++;
    }

    void reset_position()
    {
        // Move rigidbody back to initial position
        player_rb.position = Vector3.zero;
        player_rb.rotation = Quaternion.identity;

        // Enable player movement 
        player.GetComponent<Movement>().allow_backwards = 0;
        player.GetComponent<Movement>().allow_forwards = 0;
        player.GetComponent<Movement>().allow_horizontal = 0;
    }

    #endregion

    #region Scene and obstacles

    void RandomizeEnvWithGrid()
    {
        numObstacles = numTargets * obst_perTarget;
        List<Vector2> possibleLocations = GeneratePossibleLocations();
        possibleLocations = RemoveForbiddenLocations(possibleLocations);
        SetTargets(possibleLocations);
        SetObstacles(numObstacles, possibleLocations);
    }

    public List<Vector2> GeneratePossibleLocations()
    {

        List<Vector2> possibleLocations = new List<Vector2>();

        float stepX = (location_grid_limitX.y - location_grid_limitX.x) / (numCellsinGrid - 1);
        float stepZ = (location_grid_limitZ.y - location_grid_limitZ.x) / (numCellsinGrid - 1);

        for (float x = location_grid_limitX.x; x <= location_grid_limitX.y; x += stepX)
        {
            for (float y = location_grid_limitZ.x; y <= location_grid_limitZ.y; y += stepZ)
            {
                possibleLocations.Add(new Vector2(x, y));
            }
        }

        return possibleLocations;
    }

    public List<Vector2> RemoveForbiddenLocations(List<Vector2> possibleLocations)
    {
        possibleLocations.RemoveAll(pos =>
            pos.x >= forbiddenZoneX.x && pos.x <= forbiddenZoneX.y &&
            pos.y >= forbiddenZoneZ.x && pos.y <= forbiddenZoneZ.y);

        return possibleLocations;
    }

    void SetObstacles(int numObstacles, List<Vector2> possibleLocations)
    {
        for (int i = 0; i < numObstacles; i++)
        {
            int idx = UnityEngine.Random.Range(0, possibleLocations.Count);
            Vector2 newObstaclePos = possibleLocations[idx];
            possibleLocations.RemoveAt(idx);

            environment.GetComponent<Rocks>().MakeRock(newObstaclePos.x, newObstaclePos.y);
        }
    }

    #endregion

    #region Targets

    void SetTargets(List<Vector2> possibleLocations)
    {

        // Instantiate targets (switched off)
        targets = new GameObject[numTargets];

        for (int i = 0; i < numTargets; i++)
        {
            int idx = UnityEngine.Random.Range(0, possibleLocations.Count);
            Vector2 newTargetPos = possibleLocations[idx];
            possibleLocations.RemoveAt(idx);

            // Add y dimension
            Vector3 targetPos3D = new Vector3(newTargetPos.x, 0.5f, newTargetPos.y);


            // Instantiate
            targets[i] = Instantiate(TargetPrefab, targetPos3D, Quaternion.Euler(0f, 0f, 0f), environment.transform);
            targets[i].name = $"{TargetPrefab.name}_" + i.ToString();

            // Set as inactive (invisible)
            targets[i].SetActive(false);

        }
    }

    private void showTargets(GameObject[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            // Set as active (visible)
            targets[i].SetActive(true);

            // Save target as soon as becomes visible
            GetComponent<Saver>().addObject(targets[i].name,
                "Target",
                targets[i].transform.position.x,
                targets[i].transform.position.y,
                targets[i].transform.position.z,
                TargetPrefab.transform.rotation[0],
                TargetPrefab.transform.rotation[1],
                TargetPrefab.transform.rotation[2],
                targets[i].transform.localScale[0],
                targets[i].transform.localScale[1],
                targets[i].transform.localScale[2]
                );
        }
    }

    private void hideTargets(GameObject[] targets)
    {

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i].activeSelf == true)
            {
                // Set as inactive (invisible)
                targets[i].SetActive(false);

                // Save target end when it stops being visible
                GetComponent<Saver>().addObjectEnd(targets[i].name);
            }
        }
    }

    private void changeTargetMaterial(GameObject target, Material mat)
    {
        target.GetComponent<MeshRenderer>().material = mat;
    }

    #endregion

    #region Black marker

    private void CreateMarkerBlack(GameObject markerObj, Camera Camera)
    {
        // Set the position and scale of the Quad
        markerObj.transform.position = Camera.transform.position + Camera.transform.forward * 1f;
        markerObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Set the Quad to face the camera
        markerObj.transform.LookAt(Camera.transform);
        markerObj.transform.Rotate(0, 180, 0);

        // Create a new Material with a pure black color
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.color = Color.black;

        // Set the Material of the Quad
        Renderer renderer = markerObj.GetComponent<Renderer>();
        renderer.material = material;

        // Disable shadows
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        // Initially set the marker to be invisible
        markerObj.SetActive(false);
    }

    private void showMarkerBlack(GameObject markerObj)
    {
        // Set active
        markerObj.SetActive(true);

        // // Save marker as soon as becomes visible
        string identifier = markerObj.GetInstanceID().ToString();
        experiment.GetComponent<Saver>().addObject(identifier, "Black_pixels",
                        markerObj.transform.position.x, markerObj.transform.position.y, markerObj.transform.position.z,
                        markerObj.transform.eulerAngles.x, markerObj.transform.eulerAngles.y, markerObj.transform.eulerAngles.z,
                        markerObj.transform.localScale.x, markerObj.transform.localScale.y, markerObj.transform.localScale.z);
    }

    private void hideMarkerBlack(GameObject markerObj)
    {
        // Set inactive
        markerObj.SetActive(false);

        // Save marker end when it stops being visible
        GetComponent<Saver>().addObjectEnd(markerObj.GetInstanceID().ToString());
    }
    #endregion

    private static bool notMovingForTime(float seconds)
    {
        if (!isMoving)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }

            if (stopwatch.Elapsed.TotalSeconds > seconds)
            {
                stopwatch.Reset();
                return true;
            }
        }
        else
        {
            stopwatch.Reset();
        }

        return false;
    }

    #endregion

}