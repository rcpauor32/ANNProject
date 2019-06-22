using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleGameManager : MonoBehaviour {

    public GameObject uni = null;
    public GameObject bi = null;

    public GameObject coin = null;

    public GameObject spawn1 = null;
    public GameObject spawn2 = null;
    public GameObject spawn3 = null;

    public float timeBtwObs = 2.0f;
    float timer = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

        if(timer > timeBtwObs)
        {
            SpawnObstacle();
            timer = 0;
        }
	}

    void SpawnObstacle()
    {
        GameObject spawn = null;
        float rand = Random.Range(0f, 1f);
        if(rand < 0.33)
        {
            spawn = spawn1;
        }
        else if(rand < 0.66)
        {
            spawn = spawn2;
        }
        else
        {
            spawn = spawn3;
        }
        GameObject obs = GameObject.Instantiate<GameObject>(uni);
        obs.transform.position = spawn.transform.position;

        List<GameObject> open = new List<GameObject>();
        if (spawn1 != spawn)
        {
            open.Add(spawn1);
        }
        if (spawn2 != spawn)
        {
            open.Add(spawn2);
        }
        if (spawn3 != spawn)
        {
            open.Add(spawn3);
        }

        // coin

        if (Random.Range(0f, 1f) < 0.10f)
        {

            GameObject c = GameObject.Instantiate<GameObject>(coin);
            c.transform.position = open[Random.Range(0, open.Count)].transform.position;
        }
        else
        {
            GameObject obs2 = GameObject.Instantiate<GameObject>(uni);
            obs2.transform.position = open[Random.Range(0, open.Count)].transform.position;
        }
        
    }
}
