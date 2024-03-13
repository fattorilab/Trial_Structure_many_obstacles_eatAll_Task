using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;
using PupilLabs;


public class MainTask : MonoBehaviour
{
    #region Variables Declaration

    [SerializeField]

    [HideInInspector]
    [Header("GameObjects and components")]

    // Cams
    Camera camM;
    Camera camL;
    Camera camR;

    // Pupil
    public PupilDataStream PupilDataStreamScript;
    private RequestController RequestControllerScript;
    private bool PupilDataConnessionStatus;

    // Game
    [HideInInspector] GameObject environment;
    [HideInInspector] GameObject experiment;
    [HideInInspector] GameObject player;

    [Header("Saving info")]
    public string MEF;
    public string path_to_data = "C:/Users/admin/Desktop/Registrazioni_VR/";
    public bool RECORD_EYES;
    [HideInInspector] public int lastIDFromDB;
    [HideInInspector] public int seed = 12345;
    [HideInInspector] public long starttime = 0;
    [HideInInspector] public int frame_number = 0;

    [Header("Reward")]
    public int RewardLength = 50;
    float RewardLength_in_sec; // Formatting
    public int reward_counter = 0; //just for having this information readibily accessible
    public int minimumRewardTime; //delay before juicy ??unnecessary??
    [HideInInspector] public float minimumDistance; //to get juicy

    [Header("Trials Info")]
    public string file_name_positions;
    // States
    public int current_state;
    private int last_state;
    [HideInInspector] public string error_state;
    // Trials
    public int current_trial;
    public int trials_win;
    public int trials_lose;
    public int[] trials_for_target;
    public int trials_for_cond;
    // Conditions
    private int randomIndex;
    public List<int> condition_list;
    [HideInInspector] public int current_condition;
    //
    private float lastevent;
    private string identifier;
    private bool isMoving = false;
    private bool first_frame;

    [Header("Target Info")]
    // List, because it is changing size during the runtime
    public List<Vector3> target_positions = new List<Vector3>();
    GameObject[] targets;
    public GameObject TargetPrefab; 
    public Vector3 TargetSize;
    public Vector3 CorrectTargetCurrentPosition;

    #region Materials
    [Header("Target Materials")]
    public Material neutral_mat;
    public Material red_mat;
    public Material chosen_mat;
    public Material prejuicy_mat;
    public Material juicy_mat;
    public Material eaten_mat;
    #endregion

    [Header("Epoches Info")]
    // Array, because is not changing size during the runtime
    public float[] FREE_timing = { 0.3f, 0.6f, 0.9f };
    public float[] DELAY_timing = { 0.3f, 0.6f, 0.9f };
    public float[] RT_timing = { 0.3f, 0.6f, 0.9f };

    private List<int> FREE_timing_list;
    private List<int> DELAY_timing_list;
    private List<int> RT_timing_list;

    private float FREE_duration;
    private float DELAY_duration;
    private float RT_duration;
    

    [Header("Arduino Info")]
    public Ardu ardu;
    public float arduX;
    public float arduY;
    //public int dead_zone;
      
    [Header("PupilLab Info")]
    public Vector2 centerRightPupilPx = new Vector2(float.NaN, float.NaN);
    public Vector2 centerLeftPupilPx = new Vector2(float.NaN, float.NaN);
    public float diameterRight = float.NaN;
    public float diameterLeft = float.NaN;
    public bool pupilconnection;

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        // Setup
        UnityEngine.Random.InitState(seed);
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        RewardLength_in_sec = RewardLength / 1000f;
        first_frame = true;

        // States
        current_state = 0;
        last_state = -1;
        error_state = "";

        // Trials
        current_trial = 0;
        trials_win = 0;
        trials_lose = 0;
   
        // GameObjects
        ardu = GetComponent<Ardu>(); 
        player = GameObject.Find("Player");
        experiment = GameObject.Find("Experiment");
        environment = GameObject.Find("Environment");

        // Init cameras
        camM = GameObject.Find("Main Camera").GetComponent<Camera>();
        camL = GameObject.Find("Left Camera").GetComponent<Camera>();
        camR = GameObject.Find("Right Camera").GetComponent<Camera>();

        #region PUPIL MANAGEMENT
        /*
        PupilDataStreamScript = GameObject.Find("PupilDataManagment").GetComponent<PupilDataStream>();
        RequestControllerScript = GameObject.Find("PupilDataManagment").GetComponent<RequestController>();
        RequestControllerScript.connectOnEnable = pupilconnection;
        */
        #endregion

        // Import targets coordinates from csv file into target_positions list
        // and initiate the targets in a disable state (i.e invisible)
        InstantiateTargets(target_positions);

        // Define number of trials per each target
        trials_for_target = new int[target_positions.Count];

        // Save target settings (????)
        GetComponent<Saver>().addObject("Target_Setting: Custom", "Setting", 0, 0, 0, 0, 0, 0, 0, 0, 0);

        // Generate condition and timing vectors
        condition_list = CreateRandomSequence(target_positions.Count, trials_for_cond * target_positions.Count);
        FREE_timing_list = CreateRandomSequence(FREE_timing.Length, trials_for_cond * target_positions.Count);
        DELAY_timing_list = CreateRandomSequence(DELAY_timing.Length, trials_for_cond * target_positions.Count);
        RT_timing_list = CreateRandomSequence(RT_timing.Length, trials_for_cond * target_positions.Count);

    }

    void Update()
    {
        frame_number++;

        #region PUPIL MANAGEMENT
        /* serve ancora ?
        PupilDataConnessionStatus = PupilDataStreamScript.subsCtrl.IsConnected;

        if (PupilDataConnessionStatus)
        {
            //Debug.Log((centerRightPupilPx[0]).ToString());
            centerRightPupilPx = PupilDataStreamScript.CenterRightPupilPx;
            centerLeftPupilPx = PupilDataStreamScript.CenterLeftPupilPx;
            diameterRight = PupilDataStreamScript.DiameterRight;
            diameterLeft = PupilDataStreamScript.DiameterLeft;
            ardu.SendPupilLabData(centerRightPupilPx[0], centerRightPupilPx[1], centerLeftPupilPx[0], centerLeftPupilPx[1]);
        }
        */
        #endregion

        #region Ardu sanity check
        arduX = ardu.ax1;   //note: if arduino is not connected (or not working) the arduX,Y = NaN;
        arduY = ardu.ax2;

        if ((!float.IsNaN(arduX) && arduX != 0) || (!float.IsNaN(arduY) && arduY != 0) || Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) //if arduX is nan, I cannot compare it with 0
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        #endregion


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


        #region StateMachine
 
        switch (current_state)
        {
            case 0: //INTERTRIAL

                #region State Beginning (executed once upon entering)

                if (last_state != current_state)
                {
                    current_condition = -1;

                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";
                }
                #endregion

                #region State Body (executed every frame while in state)

                current_condition = -1;

                #endregion

                #region State End (executed once upon exiting)
                if (!isMoving)
                {   
                    // Prepare everything for next trial

                    // Choose the correct target
                    current_condition = condition_list[0];
                    CorrectTargetCurrentPosition = target_positions[current_condition];

                    // Picking first time from the timing list to select epoch durations in this trial
                    FREE_duration = FREE_timing[FREE_timing_list[0]];
                    DELAY_duration = DELAY_timing[DELAY_timing_list[0]];
                    RT_duration = RT_timing[RT_timing_list[0]];

                    // Move to state 1
                    current_state = 1;

                    // Trial starts
                    current_trial++;

                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            case 1: //FREE

                #region State Beginning
                if (last_state != current_state) 
                {
                    // Enable targets

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";
                }
                #endregion

                #region State Body
                if (isMoving)
                {
                    error_state = "ERR: Moving in FREE";
                    current_state = -99;
                }
                #endregion

                #region State End
                if ((Time.time - lastevent) >= FREE_duration && !isMoving) //StateEnd
                {
                    current_state = 2;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 2: //DELAY

                #region State Beginning
                if (last_state != current_state)
                {
                    // Set targets color
                    for (int i = 0; i < targets.Length; i++)
                    {
                        targets[i].GetComponent<Renderer>().material = juicy_mat; // CHANGE
                    }

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";
                }
                #endregion

                #region State Body
                if (isMoving)
                {
                    error_state = "ERR: Moving in DELAY";
                    current_state = -99;
                }
                #endregion

                #region State End
                if ((Time.time - lastevent) >= DELAY_duration && !isMoving)
                {
                    current_state = 3;
                }
                #endregion

                break;


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 3: //RT

                #region State Beginning
                if (last_state != current_state)
                {
                    // Switch target color
                    for (int i = 0; i < targets.Length; i++)
                    {
                        targets[i].GetComponent<Renderer>().material = prejuicy_mat;
                    }

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;
                    error_state = "";
                }
                #endregion

                #region State Body
                if (isMoving) 
                {
                    current_state = 99;
                }
                #endregion

                #region State End
                if ((Time.time - lastevent) >= RT_duration && !isMoving)
                {
                    error_state = "ERR: Not Moving in RT";
                    current_state = -99;
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case -99: //ERROR

                #region State Beginning
                if (last_state != current_state) //StateBeginning
                {
                    Debug.Log(error_state);
                    identifier = "Dummy for debugging";
                    GetComponent<Saver>().addObjectEnd(identifier);
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
                    current_state = 0;
                    error_state = "";
                }
                #endregion

                break;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            case 99: //WIN

                #region State Beginning
                if (last_state != current_state)
                {
                    Debug.Log("TRIAL DONE");
                    identifier = "Dummy for debugging";
                    GetComponent<Saver>().addObjectEnd(identifier);
                    reset_win();

                    //Beginning routine
                    lastevent = Time.time;
                    last_state = current_state;

                }
                #endregion

                #region State Body

                if (condition_list.Count == 0) { QuitGame();  }

                #endregion

                #region State End
                if (true)
                {
                    current_state = 0;
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
        // Disable targets
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Renderer>().enabled = false;
        }

        // Send reward
        ardu.SendReward(RewardLength);

        // Reset states
        current_state = 0;
        lastevent = Time.time;

        // Count trial
        trials_win++;
        trials_for_target[current_condition]++;

        // Remove condition
        condition_list.RemoveAt(0);
        FREE_timing_list.RemoveAt(0);
        DELAY_timing_list.RemoveAt(0);
        RT_timing_list.RemoveAt(0);
    }

    void reset_lose()
    {
        // Disable targets
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Renderer>().enabled = false;
        }

        condition_list = SwapVector(condition_list);
        FREE_timing_list = SwapVector(FREE_timing_list); //not strictly necessary, but better for coherence...
        DELAY_timing_list = SwapVector(DELAY_timing_list);
        RT_timing_list = SwapVector(RT_timing_list);

        current_state = 0;
        lastevent = Time.time;
        trials_lose++;
    }

    #endregion

    #region Manage conditions

    public List<int> CreateRandomSequence(int n, int k) //n, number of elements; k, length of the required vector
    {
        var vector = new List<int>();
        for (int i = 0; i < Math.Floor((double)k / n) + 1; i++)
        {
            var tmp = Enumerable.Range(0, n).OrderBy(x => UnityEngine.Random.Range(0, n)).ToList();
            vector.AddRange(tmp);
        }

        // If k is not a multiple of n, we need to remove the extra elements
        if (vector.Count > k)
        {
            vector = vector.Take(k).ToList();
        }

        return vector;
    }

    public List<int> SwapVector(List<int> vector)
    {
        // Moves the first half to fifth of the vector to the end of the vector
        int i = vector.Count / UnityEngine.Random.Range(2, 5);  
        if (i > 0)
        {
            vector = vector.Skip(i).Concat(vector.Take(i)).ToList();
        }
        return vector;
    }

    void set_epochs_duration()
    {
        int randomIndex_FREE = UnityEngine.Random.Range(0, FREE_timing.Length);
        int randomIndex_DELAY = UnityEngine.Random.Range(0, DELAY_timing.Length);
        int randomIndex_RT = UnityEngine.Random.Range(0, RT_timing.Length);

        FREE_duration = FREE_timing[randomIndex_FREE];
        DELAY_duration = DELAY_timing[randomIndex_DELAY];
        RT_duration = RT_timing[randomIndex_RT];

    }

    #endregion

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
        Debug.Log($"Target positioned as: {target_positions}");

        // Instantiate targets (switched off)
        targets = new GameObject[target_positions.Count];
        for (int i = 0; i < targets.Length; i++)
        {
            // Instantiate
            targets[i] = Instantiate(TargetPrefab, target_positions[i], TargetPrefab.transform.rotation, environment.transform); // Quaternion.Euler(0, 0, 0), environment.transform);
            targets[i].name = $"{TargetPrefab.name}_" + i.ToString();
            targets[i].transform.localScale = TargetSize;

            // Disable (make invisible)
            targets[i].GetComponent<Renderer>().enabled = false;

            // Add target to data to be saved
            identifier = "Target" + current_condition.ToString();

            GetComponent<Saver>().addObject(identifier,
                "Target", // target_positions[i].x, target_positions[i].z, 0, "Position");
                targets[i].transform.position.x,
                targets[i].transform.position.y,
                targets[i].transform.position.z,
                TargetPrefab.transform.rotation[0],
                TargetPrefab.transform.rotation[1],
                TargetPrefab.transform.rotation[2],
                TargetSize[0],
                TargetSize[1],
                TargetSize[2]
                );
        }
    }
}