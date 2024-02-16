using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    GameObject experiment;

    void Start()
    {
        experiment = GameObject.Find("Experiment");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            experiment.GetComponent<TriggerBox>().SendToTriggerBox(12);
            experiment.GetComponent<Saver>().addObject(transform.tag,
                                                    transform.position.x,
                                                    transform.position.z,
                                                    transform.eulerAngles.y, "TouchedARock");
            experiment.GetComponent<Forest>().phase = 103;
        }
    }
}
