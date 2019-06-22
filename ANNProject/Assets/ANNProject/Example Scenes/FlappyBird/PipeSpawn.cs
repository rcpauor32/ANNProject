using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSpawn : MonoBehaviour {

    public float timeBetweenPipes = 1f;
    public GameObject pipeGO = null;

    public float timer = 0;

	// Use this for initialization
	void Start () {
        timer = timeBetweenPipes;
	}
	
	// Update is called once per frame
	void Update () {
        if (pipeGO == null) return;

        timer += Time.deltaTime;
        if(timer >= timeBetweenPipes)
        {
            GameObject pipe = GameObject.Instantiate(pipeGO);
            float y = 1.4f;
            pipe.transform.position = new Vector3(this.transform.position.x, y, 0);
            timer = 0;
        }
	}
}
