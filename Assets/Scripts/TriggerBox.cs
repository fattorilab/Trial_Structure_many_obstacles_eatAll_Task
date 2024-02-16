using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class TriggerBox : MonoBehaviour
{
    SerialPort sp;
    Thread thread;
    //int COMspeed = 57600; //int[] baudRate = { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
    public string COM;

    byte[] msg_to_send;
    int triggered = 0;


    // Start is called before the first frame update
    void Start()
    {
		msg_to_send = new byte[1];

		try {
            //sp = new SerialPort("\\\\.\\" + COM, COMspeed);
            sp = new SerialPort(COM);
            if (!sp.IsOpen)
            {
                Debug.Log("Opening " + COM);
                sp.Open();
                sp.ReadTimeout = 100;
                sp.Handshake = Handshake.None;
                //if (sp.IsOpen) { Debug.Log("Open"); }
            }
        } catch
        {
            Debug.Log("TriggerBox was not found at " + COM);
        }

    }

    public void SendToTriggerBox(int msg)
    {
        msg_to_send[0] = (byte)msg;
        //sp.Write(msg_to_send, 0, 1);

        
        if (msg != 0)
        {
            GetComponent<Saver>().addObject("Trigger", msg, msg, 
                System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - GetComponent<Saver>().starttime, "Trigger");
            //is 10-15ms slower
            triggered = 1;
        }
    }

    void OnApplicationQuit()
    {
        sp.Close();
    }

    void Update()
    {
        
        if (triggered != 0)
        {
            triggered += 1;
            if (triggered >= 11)
            {
                SendToTriggerBox(0);
                triggered = 0;
            }
            
        }

        /*
        if (Input.GetKeyDown("space"))
        {
            SendToTriggerBox(1);
            Debug.Log("Started Experiment");
            //msg_to_send[0] = (byte)1;
            //sp.Write(msg_to_send, 0, 1);
        }*/
    }
}
