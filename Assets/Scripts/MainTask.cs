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


public class MainTask : MonoBehaviour
{
    #region Variables Declaration

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

    [Header("Saving info")]
    public string MEF;
    public string path_to_data = "C:/Users/admin/Desktop/Registrazioni_VR/";
    public bool RECORD_EYES;
    [System.NonSerialized] public int lastIDFromDB;
    [HideInInspector] public int seed;
    [HideInInspector] public long starttime = 0;
    [HideInInspector] public int frame_number = 0;

    [Header("Reward")]
    public int RewardLength = 50;
    private float RewardLength_in_sec; // Formatting
    public int reward_counter = 0; //just for having this information readibily accessible

    [Header("Trials Info")]
    // States
    [System.NonSerialized] public int current_state;
    private int last_state;
    [System.NonSerialized] public string error_state;
    // Trials
    [System.NonSerialized] public int current_trial;
    public int trials_win;
    public int trials_lose;
    public int[] trials_for_target;
    public int trials_for_cond = 1;
    // Conditions
    [System.NonSerialized] public int current_condition;
    //
    private float lastevent;
    private string identifier;
    private bool first_frame;
    // Moving timer
    private static bool isMoving = false;
    private static Diagnostics.Stopwatch stopwatch = new Diagnostics.Stopwatch();
    // Obstacles 
    public float[] obstacles_x_pos = { -15, -10, -5, 0, 5, 10, 15 };
    public float[] obstacles_z_pos = { 5, 10, 15, 20, 25, 30, 35 };
    public bool randomizeRocksPosition = true;

    [Header("Target Info")]
    public string file_name_positions;
    // List, because it is changing size during the runtime
    public List<Vector3> target_positions = new List<Vector3>();
    public bool randomizeTargetsPosition = true;
    GameObject[] targets;
    [System.NonSerialized] public GameObject TargetPrefab;
    [System.NonSerialized] public Vector3 CorrectTargetCurrentPosition;

    #region Materials
    [Header("Target Materials")]
    [System.NonSerialized] public Material initial_grey;
    [System.NonSerialized] public Material red;
    [System.NonSerialized] public Material green_dot;
    [System.NonSerialized] public Material red_dot;
    [System.NonSerialized] public Material final_grey;
    [System.NonSerialized] public Material white;
    #endregion

    [Header("Epoches Info")]
    // Array, because is not changing size during the runtime
    public float BASELINE_duration = 2f;
    public float INTERTRIAL_duration = 2f;
    public float MOVEMENT_maxduration = 6f;
    public float RT_maxduration = 2f;

    [Header("Arduino Info")]
    [System.NonSerialized] public Ardu ardu;
    [System.NonSerialized] public float arduX;
    [System.NonSerialized] public float arduY;

    [Header("PupilLab Info")]
    [System.NonSerialized] public Vector2 centerRightPupilPx = new Vector2(float.NaN, float.NaN);
    [System.NonSerialized] public Vector2 centerLeftPupilPx = new Vector2(float.NaN, float.NaN);
    [System.NonSerialized] public float diameterRight = float.NaN;
    [System.NonSerialized] public float diameterLeft = float.NaN;
    [System.NonSerialized] public bool pupilconnection;

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
        RewardLength_in_sec = RewardLength / 1000f;
        first_frame = true;

        // States
        current_state = -2;
        last_state = -2;
        error_state = "";

        // Trials
        current_trial = 0;
        trials_win = 0;
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

        // Import targets coordinates from csv file into target_positions list
        // and initiate the targets in a disable state (i.e invisible)
        InstantiateTargets(target_positions);

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

        // Start on first operating frame
        if (first_frame) 
        {
            Debug.Log("START TASK");
            // Start time main task unity
            starttime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            // Send START trigger
            ardu.SendStartRecordingOE();           
            first_frame = false;
        }

        // Check if the player is moving the joystick
        isMoving = player.GetComponent<Movement>().keypressed;

        #region StateMachine

        switch (current_state)
        {
            case -2: // TASK BEGIN

                if (PupilDataStreamScript.subsCtrl.IsConnected || RequestControllerScript.ans)
                {
                    current_state = -1;
                }

                if (current_state == -1)
                {
                    // Disable movement
                    player.GetComponent<Movement>().restrict_backwards = 0;
                    player.GetComponent<Movement>().restrict_forwards = 0;
                    player.GetComponent<Movement>().restrict_horizontal = 0;
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
                    // Enable movement
                    player.GetComponent<Movement>().restrict_backwards = 1;
                    player.GetComponent<Movement>().restrict_forwards = 1;
                    player.GetComponent<Movement>().restrict_horizontal = 1;

                    // Switch OFF black pixels objects
                    hideMarkerBlack(markerObject_M);
                    hideMarkerBlack(markerObject_R);
                    hideMarkerBlack(markerObject_L);

                    // Instantiate rocks
                    // if desired, randomize disposition of rocks and/or targets
                    randomizeScene(randomizeTargetsPosition, randomizeRocksPosition, targets, obstacles_x_pos, obstacles_z_pos);

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

                }
                #endregion

                #region State Body (executed every frame while in state)
                // Prevent entering MOVEMENT state from a position different from initial
                if (isMoving)
                {
                    reset_position();
                }
                #endregion

                #region State End (executed once upon exiting)
                if (notMovingForTime(BASELINE_duration))
                {
                    // Prepare everything for next trial

                    // Make targets visible
                    showTargets(targets);

                    // Change target material 
                    for (int i = 0; i < targets.Length; i++)
                    {
                        changeTargetMaterial(targets[i], red);      
                    }

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
                    bool isRock = Regex.IsMatch(player.GetComponent<Movement>().CollidedObject.tag, @"Rock[1-5]$"); ;
                    bool isTree = Regex.IsMatch(player.GetComponent<Movement>().CollidedObject.tag, @"\btree\b"); ;
                    if (isRock || isTree)
                    {
                        error_state = $"ERR: touched rock obstacle at {player.GetComponent<Movement>().CollidedObject.transform.position}";
                        current_state = -99;
                    }
                    else // Target
                    {
                        // Change target material
                        for (int i = 0; i < targets.Length; i++)
                        {
                            if (targets[i].name == player.GetComponent<Movement>().CollidedObject.name)
                            {
                                changeTargetMaterial(targets[i], final_grey);
                            }
                        }

                        // Go to RT state
                        current_state = 2;
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
                    current_state = 99;
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
                    error_state = "ERR: Not Moving in RT";
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

                    reset_lose();

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
                    current_state = -1;
                    error_state = "";

                    // Do not randomize rocks' or targets' positions
                    randomizeRocksPosition = false;
                    randomizeTargetsPosition = false;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 99: //WIN

                #region State Beginning
                if (last_state != current_state)
                {
                    // Change target material
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i].name == player.GetComponent<Movement>().CollidedObject.name)
                        {
                            changeTargetMaterial(targets[i], white);
                        }
                    }

                    Debug.Log("TRIAL DONE");
                    GetComponent<Saver>().addObjectEnd(player.GetComponent<Movement>().CollidedObject.name);

                    reset_win();

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;

                }
                #endregion

                #region State Body

                #endregion

                #region State End
                if ((Time.time - lastevent) >= RewardLength_in_sec)
                {
                    // Disable targets
                    hideTargets(targets);

                    // Reset position
                    reset_position();

                    current_state = -1;

                    // Randomize rocks' or targets' positions
                    randomizeRocksPosition = true;
                    randomizeTargetsPosition = true;

                    // Delete current rocks
                    environment.GetComponent<Rocks>().deleteRocks();
                }
                #endregion

                break;

        }

        #endregion

        // Manual reward
        if (Input.GetKeyDown("space"))        { ardu.SendReward(RewardLength); }
        reward_counter = ardu.reward_counter;

    }

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

    #region Reset methods

    void reset_win()
    {
        // Reset collision
        player.GetComponent<Movement>().resetCollision();

        // Send reward
        ardu.SendReward(RewardLength);

        // Count trial
        trials_win++;
    }

    void reset_lose()
    {
        // Reset collision
        player.GetComponent<Movement>().resetCollision();

        // Disable targets
        hideTargets(targets);

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
        player.GetComponent<Movement>().restrict_backwards = 1;
        player.GetComponent<Movement>().restrict_forwards = 1;
        player.GetComponent<Movement>().restrict_horizontal = 1;
    }

    #endregion

    #region Manage targets

    private void LoadPositionsFromCSV(List<Vector3> target_positions)
    {
        string filePath = Application.dataPath + "/" + file_name_positions + ".csv";
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line = reader.ReadLine(); // Salta la riga degli header se presente

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    if (fields.Length >= 3)
                    {
                        float x, y, z;
                        if (float.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                            float.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                            float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                        {
                            Vector3 position = new Vector3(x, y, z);
                            target_positions.Add(position);
                        }
                        else
                        {
                            Debug.LogWarning("Impossible to convert coordinates in numbers: " + line);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Line has not enough coordinates: " + line);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("File does not exist: " + filePath);
        }
    }

    private void InstantiateTargets(List<Vector3> target_positions)
    {

        // Import targets coordinates from csv file into target_positions list
        LoadPositionsFromCSV(target_positions);

        // Instantiate targets (switched off)
        targets = new GameObject[target_positions.Count];

        for (int i = 0; i < targets.Length; i++)
        {
            // Instantiate
            targets[i] = Instantiate(TargetPrefab, target_positions[i], Quaternion.Euler(0, 0, 0), environment.transform);
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

    #region Manage black marker
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

    #region Manage scene and obstacles

    private void randomizeScene(bool randomizeTargets, bool randomizeRocks, GameObject[] targets, float[] x_pos_array, float[] z_pos_array)
    {
        // Sanity checks
        if (targets.Length > x_pos_array.Length || targets.Length > z_pos_array.Length)
        {
            Debug.LogError("There are more targets than available positions. " +
                "Please, increase number of obstacles positions or decrease number of targets");
            QuitGame();
        }

        // Shuffle position arrays
        Shuffle(x_pos_array);
        Shuffle(z_pos_array);

        // Randomize rocks, if desired
        if (randomizeRocks)
        {
            instantiateRandomRocks(targets, x_pos_array, z_pos_array);
        }

        // Randomize targets, if desired
        if (randomizeTargets)
        {
            for (int i = 0; i < targets.Length; i++)
            {

                if (z_pos_array[i] > 15) //artificially make it less likely to just be there
                {
                    targets[i].transform.position = new Vector3(x_pos_array[i], 0.5f, z_pos_array[i]);
                }
                else
                {
                    targets[i].transform.position = new Vector3(x_pos_array[i], 0.5f, z_pos_array[z_pos_array.Length - 1]);
                }

                GetComponent<Saver>().addObject(targets[i].name, "Target_Position",
                    targets[i].transform.position.x, 0, targets[i].transform.position.z,
                    0, 0, 0,
                    0, 0, 0);

            }
        }

    }

    private void instantiateRandomRocks(GameObject[] targets, float[] x_pos_array, float[] z_pos_array)
    {
        // Instantiate new rocks, randomly disposed
        for (int i = targets.Length; i < x_pos_array.Length; i++)
        {
            environment.GetComponent<Rocks>().MakeRock(x_pos_array[i], z_pos_array[i]);
        }

        for (int i = 0; i < x_pos_array.Length - 1; i++)
        {
            environment.GetComponent<Rocks>().MakeRock(x_pos_array[i], z_pos_array[i + 1]);
            environment.GetComponent<Rocks>().MakeRock(x_pos_array[i + 1], z_pos_array[i]);
        }

        // Save new rocks
        environment.GetComponent<Rocks>().saveObjects();
    }

    private void Shuffle(float[] array)
    {
        int n = array.Length - 1;
        for (; n > 0; n--)
        {
            int r = UnityEngine.Random.Range(0, n);
            float t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
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
}