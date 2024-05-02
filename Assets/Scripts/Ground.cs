using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ground : MonoBehaviour
{
    public GameObject player;
    public GameObject[] ground = new GameObject[9];
    GameObject[] applied_ground = new GameObject[9];

    int midX = 0;
    int midZ = 0;
    // Start is called before the first frame update
    void Start()
    {

        //Set Camera Clipping Distance to 50 (default 1000)
        applied_ground[0] = ground[0];
        applied_ground[1] = ground[1];
        applied_ground[2] = ground[2];

        applied_ground[3] = ground[3];
        applied_ground[4] = ground[4];
        applied_ground[5] = ground[5];

        applied_ground[6] = ground[6];
        applied_ground[7] = ground[7];
        applied_ground[8] = ground[8];

        SetGround();

    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.localPosition.x > midX + 25 || player.transform.localPosition.x < midX - 25 ||
            player.transform.localPosition.z > midZ + 25 || player.transform.localPosition.z < midZ - 25)
<<<<<<< HEAD
        { // 25 to 40
=======
        {
            // 25 to 40
>>>>>>> 8d84b20 (Commenting thoroughly)
            if (player.transform.localPosition.x > midX + 40) { midX += 50; SwapRight(); }
            if (player.transform.localPosition.x < midX - 40) { midX -= 50; SwapLeft(); }
            if (player.transform.localPosition.z > midZ + 40) { midZ += 50; SwapForward(); }
            if (player.transform.localPosition.z < midZ - 40) { midZ -= 50; SwapBackward(); }
            SetGround();
        }

    }

    private void SetGround()
    {
        applied_ground[0].transform.localPosition = new Vector3(midX - 50, 0, midZ + 50);
        applied_ground[1].transform.localPosition = new Vector3(midX, 0, midZ + 50);
        applied_ground[2].transform.localPosition = new Vector3(midX + 50, 0, midZ + 50);

        applied_ground[3].transform.localPosition = new Vector3(midX - 50, 0, midZ);
        applied_ground[4].transform.localPosition = new Vector3(midX, 0, midZ); //minus weil entgegen gesetzt
        applied_ground[5].transform.localPosition = new Vector3(midX + 50, 0, midZ);

        applied_ground[6].transform.localPosition = new Vector3(midX - 50, 0, midZ - 50);
        applied_ground[7].transform.localPosition = new Vector3(midX, 0, midZ - 50);
        applied_ground[8].transform.localPosition = new Vector3(midX + 50, 0, midZ - 50);

    }

    private void SwapForward()
    {
        //old_grounds
        ground[0] = applied_ground[0];
        ground[1] = applied_ground[1];
        ground[2] = applied_ground[2];

        ground[3] = applied_ground[3];
        ground[4] = applied_ground[4];
        ground[5] = applied_ground[5];

        ground[6] = applied_ground[6];
        ground[7] = applied_ground[7];
        ground[8] = applied_ground[8];


        //new grounds
        applied_ground[0] = ground[6]; //anew
        applied_ground[1] = ground[7]; //anew
        applied_ground[2] = ground[8]; //anew

        applied_ground[3] = ground[0];
        applied_ground[4] = ground[1];
        applied_ground[5] = ground[2];

        applied_ground[6] = ground[3];
        applied_ground[7] = ground[4];
        applied_ground[8] = ground[5];


        applied_ground[0].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[1].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[2].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[0].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[1].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[2].GetComponent<CreateTreesAndTargets>().createGreenery();

    }

    private void SwapBackward()
    {
        //old_grounds
        ground[0] = applied_ground[0];
        ground[1] = applied_ground[1];
        ground[2] = applied_ground[2];

        ground[3] = applied_ground[3];
        ground[4] = applied_ground[4];
        ground[5] = applied_ground[5];

        ground[6] = applied_ground[6];
        ground[7] = applied_ground[7];
        ground[8] = applied_ground[8];


        //new grounds
        applied_ground[0] = ground[3];
        applied_ground[1] = ground[4];
        applied_ground[2] = ground[5];

        applied_ground[3] = ground[6];
        applied_ground[4] = ground[7];
        applied_ground[5] = ground[8];

        applied_ground[6] = ground[0]; //anew
        applied_ground[7] = ground[1]; //anew
        applied_ground[8] = ground[2]; //anew

        applied_ground[6].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[7].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[8].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[6].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[7].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[8].GetComponent<CreateTreesAndTargets>().createGreenery();

    }

    // 0 1 2
    // 3 4 5
    // 6 7 8

    private void SwapRight()
    {
        //old_grounds
        ground[0] = applied_ground[0];
        ground[1] = applied_ground[1];
        ground[2] = applied_ground[2];

        ground[3] = applied_ground[3];
        ground[4] = applied_ground[4];
        ground[5] = applied_ground[5];

        ground[6] = applied_ground[6];
        ground[7] = applied_ground[7];
        ground[8] = applied_ground[8];


        //new grounds
        applied_ground[0] = ground[1];
        applied_ground[1] = ground[2];
        applied_ground[2] = ground[0]; //anew

        applied_ground[3] = ground[4];
        applied_ground[4] = ground[5];
        applied_ground[5] = ground[3]; //anew

        applied_ground[6] = ground[7];
        applied_ground[7] = ground[8];
        applied_ground[8] = ground[6]; //anew

        applied_ground[2].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[5].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[8].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[2].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[5].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[8].GetComponent<CreateTreesAndTargets>().createGreenery();

    }

    private void SwapLeft()
    {
        //old_grounds
        ground[0] = applied_ground[0];
        ground[1] = applied_ground[1];
        ground[2] = applied_ground[2];

        ground[3] = applied_ground[3];
        ground[4] = applied_ground[4];
        ground[5] = applied_ground[5];

        ground[6] = applied_ground[6];
        ground[7] = applied_ground[7];
        ground[8] = applied_ground[8];


        //new grounds
        applied_ground[0] = ground[2]; //anew
        applied_ground[1] = ground[0];
        applied_ground[2] = ground[1];

        applied_ground[3] = ground[5];//anew
        applied_ground[4] = ground[3];
        applied_ground[5] = ground[4];

        applied_ground[6] = ground[8]; //anew
        applied_ground[7] = ground[6];
        applied_ground[8] = ground[7];

        applied_ground[0].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[3].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[6].GetComponent<CreateTreesAndTargets>().deleteGreenery();
        applied_ground[0].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[3].GetComponent<CreateTreesAndTargets>().createGreenery();
        applied_ground[6].GetComponent<CreateTreesAndTargets>().createGreenery();

    }

    // 0 1 2
    // 3 4 5
    // 6 7 8
}
