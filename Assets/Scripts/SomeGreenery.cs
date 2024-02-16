using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeGreenery : MonoBehaviour
{
    public GameObject bush1;
    public GameObject bush2;
    public GameObject bush3;
    public GameObject bush4;
    public GameObject bush5;

    public GameObject mushroom1;
    public GameObject mushroom2;
    public GameObject mushroom3;

    // Start is called before the first frame update
    void Start()
    {
        createGreenery();
    }

    public void createGreenery()
    {
        for (int i = 0; i < 100; i++)
        {
            createOneObject(bush1);
            createOneObject(bush2);
            createOneObject(bush3);
            createOneObject(bush4);
            createOneObject(bush5);

            createOneObject(mushroom1);
            createOneObject(mushroom2);
            createOneObject(mushroom3);
        }
    }

    void createOneObject(GameObject Prefab)
    {
        Vector3 pos = new Vector3(Random.Range(-25f, 25f), 0, Random.Range(-25f, 25f));
        /*if (!(pos.x <  2f && pos.x > - 2f && pos.z < 12 && pos.z > -1)) //Im Weg
        {
            Instantiate(Prefab, pos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), transform);
        }  */
        Instantiate(Prefab, pos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), transform);
    }

    public void deleteGreenery()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}
