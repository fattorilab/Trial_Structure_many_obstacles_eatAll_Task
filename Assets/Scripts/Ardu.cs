using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Ardu : MonoBehaviour
{
    arduino ardu;
    bool ardu_working = false;
    public bool testing;
    // ----------------------------- mod marrti 29/11/23
    //PC ALIEN
    //public string COM = "COM3";
    //PC_TELECAMERE
    public string COM = "COM10"; //--------------------
    //public string rewardTime = "500";
    public float ax1 = 0;
    public float ax2 = 0;
    // Start is called before the first frame update

    public int reward_counter;

    void Start()
    {
        try
        {
            ardu = new arduino(COM, 57600, 80);
            ardu_working = true;
        }
        catch // (IOException ioex)
        {
            //Debug.Log($"{Time.frameCount}. exception: {ioex.Message}");
            Debug.Log("Please connect all cables!");
            ardu_working = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ardu_working)
        {
            ax1 = ardu.getX();
            ax2 = -ardu.getY();
            //Debug.Log("X " + ax1 + " Y " + ax2);
        }

        if (Input.GetKey("escape"))
        {
            if (ardu_working)
            {
                ardu.stopserial();
            }
            Application.Quit();
        }

        if (Input.GetKeyDown("space"))
        {
            SendReward(GetComponent<MainTask>().reward_length);
        }
    }

    public void SendReward(int rewardTime)
    {
        if (ardu_working && !testing)
        {
            ardu.sendSerial("R" + rewardTime.ToString());
            Debug.Log("Reward");
            reward_counter += 1;
        }
    }
    // TRIGGER 1 BNC 8(T1)
    public void SendStartRecordingOE()
    {
        if (ardu_working && !testing)
        {
            ardu.sendSerial("TRIG1_ON");
            Debug.Log("REC OE ON");
        }
    }
    // TRIGGER 1 BNC 8(T1)
    public void SendStopRecordingOE()
    {
        if (ardu_working && !testing)
        {
            ardu.sendSerial("TRIG1_OFF");
            Debug.Log("REC OE OFF");
        }
    }
    // TRIGGER 2 BNC 7(T2)
    public void SendStartReferenceMarkerOE()
    {
        if (ardu_working && !testing)
        {
            ardu.sendSerial("TRIG2_ON");
            Debug.Log("TRIG2_ON");
        }
    }
    // TRIGGER 2 BNC 7(T2)
    public void SendStopReferenceMarkerOE()
    {
        if (ardu_working && !testing)
        {
            ardu.sendSerial("TRIG2_OFF");
            Debug.Log("TRIG2_OFF");
        }
    }
}
