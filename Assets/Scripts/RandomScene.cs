using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RandomScene : MonoBehaviour
{
    public GameObject bush1;
    public GameObject bush2;
    public GameObject bush3;
    public GameObject bush4;
    public GameObject bush5;

    public GameObject mushroom1;
    public GameObject mushroom2;
    public GameObject mushroom3;

    public GameObject tree1;
    public GameObject tree2;
    public GameObject lowlevel;
    public GameObject midlevel;
    public GameObject highlevel;

    // Start is called before the first frame update
    void Start()
    {
        createGreenery();
    }
    public void createGreenery()
    {
        for (int i = 0; i < 50; i++) //150
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

        smallFoliages();
        createTreesAndObjects();
    }

    void createTreesAndObjects()
    {
        for (int i = 0; i < 3; i++)
        {
            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor
            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor

            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor
            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor

            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor
            createOneObject(tree1); //Distraktor
            createOneObject(tree2); //Distraktor

            createOneObject(lowlevel);
            createOneObject(midlevel);
            createOneObject(highlevel);
        }

    }

    void smallFoliages()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Material/Stylized foliages pack" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            for (int i = 0; i < 20; i++) //50
            {
                createSmallObject(go);
            }
            //createOneObject(go);
        }
    }

    void createOneObject(GameObject Prefab)
    {
        Vector3 pos = new Vector3(Random.Range(-25f, 25f), 0, Random.Range(-25f, 25f));
        Instantiate(Prefab, transform.position + pos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), transform);
    }

    void createSmallObject(GameObject Prefab)
    {
        Vector3 pos = new Vector3(Random.Range(-25f, 25f), 0, Random.Range(-25f, 25f));
        GameObject newObject = Instantiate(Prefab, transform.position + pos, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), transform);
        newObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // change its local scale in x y z format
    }

    public void deleteGreenery()
    {
        foreach (Transform child in transform)
        {
            if (child.name != "Ground")
            {
                Destroy(child.gameObject);
            }     
        }
    }

}
