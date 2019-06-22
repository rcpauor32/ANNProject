using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleObstacle : MonoBehaviour {

    public Vector3 speed = new Vector3(0, 0, -10);
    public GameObject despawn = null;

	// Use this for initialization
	void Start () {
        despawn = GameObject.FindGameObjectWithTag("Finish");
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += speed * Time.deltaTime;
        if (transform.position.z < despawn.transform.position.z)
            Destroy(gameObject);
	}
}
