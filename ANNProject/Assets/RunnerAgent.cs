using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerAgent : ANNAgent {

    public GameObject p1 = null;
    public GameObject p2 = null;
    public GameObject p3 = null;

    public float fitness = 0;

    public float rayOffset = 1f;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        // Get Reference to the 3 posible positions

        p1 = GameObject.Find("Pos1");
        p2 = GameObject.Find("Pos2");
        p3 = GameObject.Find("Pos3");
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        fitness += Time.deltaTime;

        // RayCast to detect obstacle distance ---
        RaycastHit rayhit;
        Physics.Raycast(p1.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);
        this.SetInput("ray_0", rayhit.distance);
        Physics.Raycast(p2.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);
        this.SetInput("ray_1", rayhit.distance);

        Physics.Raycast(p3.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);
        this.SetInput("ray_2", rayhit.distance);


        float output = this.GetOutput("pos");

        // If OUTPUT > 0.33 -> position 1 (left)
        //   else if > 0.66 -> position 2 (middle)
        //   else           -> position 3 (right)

        GameObject auxP = null;
        if (output < 0.33f)
        {
            auxP = p1;
        }
        else if (output < 0.66f)
        {
            auxP = p2;
        }
        else
        {
            auxP = p3;
        }

        transform.position = auxP.transform.position;

    }

    void OnTriggerEnter(Collider col) // On Collision
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            this.EndNetCycle(fitness);
        }
    }
    
    public override void OnAgentEndMulti() // this sets what happens when ONE agent 'ends' on the multi-agent mode
    {
        base.OnAgentEndMulti();
        this.gameObject.SetActive(false); // this just deactivates de agent
    }
    
    
    public override void OnAgentEndSingle() // this sets what happens when ONE agent 'ends' on the single-agent mode
    {
        base.OnAgentEndSingle();
        Debug.Log("Ended Agent -> Fit: " + fitness);
        fitness = 0;
    }
    
    
    public override void OnGenerationEndMulti() // when a whole generation ends 
    {
        base.OnGenerationEndMulti();
        TempleObstacle[] obs = GameObject.FindObjectsOfType<TempleObstacle>();
        foreach (TempleObstacle o in obs)
        {
            Destroy(o.gameObject); // this resets the scene
        }
    }
    
    
    public override void OnGenerationEndSingle()
    {
        base.OnGenerationEndSingle();
        TempleObstacle[] obs = GameObject.FindObjectsOfType<TempleObstacle>();
        foreach (TempleObstacle o in obs)
        {
            Destroy(o.gameObject);
        }
    }
}
