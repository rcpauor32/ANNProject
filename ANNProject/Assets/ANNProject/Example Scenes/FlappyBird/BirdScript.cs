using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdScript : ANNAgent {

    public float click;

    public float jumpforce = 10;
    public float gravity = 9.8f;

    public float movementSpeed = 5.0f;

    private float timer = 0;

    Vector3 startpos;
    Rigidbody body;

    Vector3 auxpos = Vector3.zero;

    GameObject passpoint = null;

    public float timeBetweenJumps = 0.1f;
    private float jumpTimer = 0;

	// Use this for initialization
	public override void Start () {
        base.Start();
        startpos = transform.position;
        auxpos = transform.position;
        body = GetComponent<Rigidbody>();
	}

    // Update is called once per frames
    public override void Update() {
        base.Update();

        timer += Time.deltaTime;
        GetComponent<Rigidbody>().AddForce(Vector3.down * gravity);

        float speed = Vector3.Magnitude(transform.position - auxpos) / Time.deltaTime;

        auxpos = transform.position;

        SetInput("speed", speed);

        // ANN //
        passpoint = null;
        GameObject[] points = GameObject.FindGameObjectsWithTag("passpoint");
        if (points.Length > 0)
        {
            passpoint = points[0];
            foreach (GameObject p in points)
            {
                float xdist = p.transform.position.x - transform.position.x;
                float pxdist = passpoint.transform.position.x - transform.position.x;
                if (xdist > 0 && (xdist < pxdist || pxdist < 0))
                {
                    passpoint = p;
                }
            }

            float d = passpoint.transform.position.x - transform.position.x;
            float h = passpoint.transform.position.y - transform.position.y;
            SetInput("ddist", d);
            SetInput("hdist", h);
        }
        else
        {
            SetInput("ddist", -1);
            SetInput("hdist", -1);
        }

        SetInput("ypos", this.transform.position.y);

        click = GetOutput("click");

        jumpTimer += Time.deltaTime;
        if(click > 0.5)
        {
            Jump();
            //transform.position += Vector3.up * movementSpeed * Time.deltaTime;
            GetComponent<MeshRenderer>().material.color = Color.green;
        }
        else
        {
            //transform.position += Vector3.down * movementSpeed * Time.deltaTime;
            GetComponent<MeshRenderer>().material.color = Color.red;
        }

    }

    public void Jump()
    {
        if (jumpTimer > timeBetweenJumps)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpforce);
            jumpTimer = 0;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<BirdScript>() == false)
        {
            float fitness = timer;
            EndNetCycle(fitness);
        }
    }

    public override void OnAgentEndMulti()
    {
        base.OnAgentEndMulti();
        this.gameObject.SetActive(false);
    }

    public override void OnAgentEndSingle()
    {
        base.OnAgentEndSingle();
        transform.position = startpos;
        timer = 0;
        PipeScript[] pipes = FindObjectsOfType<PipeScript>();
        foreach (PipeScript p in pipes)
        {
            Destroy(p.gameObject);
        }
        PipeSpawn spawn = FindObjectOfType<PipeSpawn>();
        spawn.timer = spawn.timeBetweenPipes;
        body.Sleep();
    }

    public override void OnGenerationEndMulti()
    {
        base.OnGenerationEndMulti();
        transform.position = startpos;
        timer = 0;
        PipeScript[] pipes = FindObjectsOfType<PipeScript>();
        foreach (PipeScript p in pipes)
        {
            Destroy(p.gameObject);
        }
        PipeSpawn spawn = FindObjectOfType<PipeSpawn>();
        spawn.timer = spawn.timeBetweenPipes;
        body.Sleep();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (passpoint != null)
        {
            Gizmos.DrawWireSphere(passpoint.transform.position, 0.1f);
            Gizmos.DrawLine(transform.position, passpoint.transform.position);
        }
    }
}
