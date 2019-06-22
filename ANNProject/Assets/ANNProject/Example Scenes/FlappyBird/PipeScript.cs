using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeScript : MonoBehaviour {

    public float speed = 1f;
    private GameObject despawn;

    public BirdScript bird = null;
    public float yDelta = 5f;

	// Use this for initialization
	void Start () {
        despawn = GameObject.Find("PipeDespawn");
        //bird = FindObjectOfType<BirdScript>();
        transform.position = transform.position + new Vector3(0, Random.Range(-yDelta, yDelta), 0);
	}
	
	// Update is called once per frame
	void Update () {

        transform.position += Vector3.left.normalized * speed * Time.deltaTime;
        if (transform.position.x < despawn.transform.position.x)
        {
            //bird.npipes++;
            Destroy(this.gameObject);
        }
	}
}
