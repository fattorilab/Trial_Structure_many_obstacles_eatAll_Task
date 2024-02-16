using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit_classic : MonoBehaviour
{
    public float distance;
    public bool juicy;
    public bool angleJuice = false;
    public bool eaten = false;
    public bool reset_flag = false;
    //public bool Reward_for_rotation;
    GameObject player;
    GameObject experiment;

    public Material neutral_mat;
    public Material juicy_mat;
    public Material eaten_mat;

    float min_distance = 0;
    float min_time2get_reward = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        experiment = GameObject.Find("Experiment");

        try
        {
            min_distance = experiment.GetComponent<Main>().minimumDistance;
            min_time2get_reward = experiment.GetComponent<Main>().minimumRewardTime;
        }
        catch
        {
            min_distance = experiment.GetComponent<Maze>().minimumDistance;
            min_time2get_reward = experiment.GetComponent<Maze>().minimumRewardTime;
        }

        beginTimeJuicy = Time.time;

        GetComponent<Renderer>().material = neutral_mat;
    }

    Vector3 forwardPosition;
    Vector2 pos, tar, fwd;
    public float playerAngle = 90;

    float beginTimeJuicy;
    public float timeJuicy = 0;
    float releasetime = -1;
    float nulltime = 0;
    // Update is called once per frame
    void Update()
    {
        pos = new Vector2(player.transform.position.x, player.transform.position.z);
        tar = new Vector2(transform.position.x, transform.position.z);
        forwardPosition = player.transform.position + player.transform.rotation * Vector3.forward;
        fwd = new Vector2(forwardPosition.x, forwardPosition.z);
        playerAngle = Vector2.Angle(tar - pos, fwd - pos);

        distance = Vector3.Distance(player.transform.position, transform.position);

        if (reset_flag)
        {
            eaten = false;
            juicy = false;
            GetComponent<Renderer>().material = neutral_mat;
            reset_flag = false;
        }

        if (!eaten) //can get juicy or unjuicy
        {
            if (distance < min_distance)
            {
                if (player.GetComponent<Movement_classic>().presstime > min_time2get_reward && !juicy) //get juicy
                {
                    set_juicy();
                }
            }
            else if (!angleJuice)
            {
                GetComponent<Renderer>().material = neutral_mat;
                juicy = false;
                timeJuicy = 0;
            }
        }

        if (juicy)
        {
            timeJuicy = (Time.time - beginTimeJuicy) * 1000;

            if (!player.GetComponent<Movement_classic>().keypressed) //gotit
            {
                //reset released_timer
                if (releasetime == -1)
                {
                    releasetime = Time.time;
                }


                //initiate reset or give reward
                try
                { //Locomotion
                    nulltime = (float)experiment.GetComponent<Main>().nullpositionTime / 1000;
                    if ((Time.time - releasetime) > nulltime)
                    {
                        GetComponent<Renderer>().material = eaten_mat;
                        experiment.GetComponent<Main>().reset();
                        releasetime = -1;
                    }
                }
                catch
                { //Maze
                    nulltime = (float)experiment.GetComponent<Maze>().nullpositionTime / 1000;
                    if ((Time.time - releasetime) > nulltime && !eaten)
                    {
                        GetComponent<Renderer>().material = eaten_mat;
                        eaten = true;
                        juicy = false;
                        experiment.GetComponent<Ardu>().SendReward(experiment.GetComponent<Maze>().reward_length);
                        experiment.GetComponent<Maze>().eaten_targets++;
                        releasetime = -1;
                    }
                }


            }
            else
            {
                releasetime = -1;
            }
        }
        /*
        // modifica di stef e fra 21/10/2022 /////////////////
        if (!eaten && playerAngle > -5 && playerAngle < +5 && experiment.GetComponent<Main>().Reward_for_rotation)
        {
            GetComponent<Renderer>().material = juicy_mat;       
        }

        
        if (!eaten && playerAngle > -1 && playerAngle < +1 && experiment.GetComponent<Main>().Reward_for_rotation)
        {
            GetComponent<Renderer>().material = eaten_mat;
            experiment.GetComponent<Main>().reset();
            releasetime = -1;
        }
        //////////////////////////////////////////////////////
        */
    }

    public void JuicyByAngle(float angle)
    {
        if (!eaten && playerAngle > -angle && playerAngle < angle)
        {
            if (!juicy)
            {
                set_juicy();
            }
            angleJuice = true;
        }
        else
        {
            angleJuice = false;
        }
    }

    void set_juicy()
    {
        GetComponent<Renderer>().material = juicy_mat;
        try
        {
            if (experiment.GetComponent<Main>().immediateReward) { experiment.GetComponent<Ardu>().SendReward(experiment.GetComponent<Main>().reward_length); }
        }
        catch
        {
            if (experiment.GetComponent<Maze>().immediateReward) { experiment.GetComponent<Ardu>().SendReward(experiment.GetComponent<Maze>().reward_length); }
        }

        beginTimeJuicy = Time.time;
        juicy = true;
    }
}
