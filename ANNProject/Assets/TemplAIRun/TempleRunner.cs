using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleRunner : ANNAgent {

    public GameObject p1 = null;
    public GameObject p2 = null;
    public GameObject p3 = null;

    public float timescale = 1f;

    public float fitness = 0;

    public float CoinPoints = 100;

    List<Vector3> positions = new List<Vector3>();

    public float rayOffset = 1f;

    private float lifetimer = 0;

    // Use this for initialization
    public override void Start () {
        base.Start();
        p1 = GameObject.Find("Pos1");
        p2 = GameObject.Find("Pos2");
        p3 = GameObject.Find("Pos3");
    }

    // Update is called once per frame
    public override void Update () {
        base.Update();

        lifetimer += Time.deltaTime;

        if(lifetimer > 60000)
        {
            EndNetCycle(fitness * fitness);
            return;
        }

        Time.timeScale = timescale;

        fitness += Time.deltaTime;

        positions.Clear();

        RaycastHit rayhit;
        Physics.Raycast(p1.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);

        this.SetInput("ray1", rayhit.distance);
        if (rayhit.collider != null)
        {
            if(rayhit.collider.gameObject.CompareTag("Coin"))
            {
                this.SetInput("obj1", 1);
            }
            else if(rayhit.collider.gameObject.CompareTag("Obstacle"))
            {
                this.SetInput("obj1", 0);
            }
            positions.Add(rayhit.point);
        }
        else
        {
            this.SetInput("obj1", -1);
            this.SetInput("ray1", -999);
        }

        Physics.Raycast(p2.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);

        this.SetInput("ray2", rayhit.distance);

        if (rayhit.collider != null)
        {
            if (rayhit.collider.gameObject.CompareTag("Coin"))
            {
                this.SetInput("obj2", 1);
            }
            else if (rayhit.collider.gameObject.CompareTag("Obstacle"))
            {
                this.SetInput("obj2", -1);
            }
            positions.Add(rayhit.point);
        }
        else
        {
            this.SetInput("obj2", 0);
            this.SetInput("ray2", -999);
        }

        Physics.Raycast(p3.transform.position - Vector3.forward * rayOffset, Vector3.forward, out rayhit, 100);

        this.SetInput("ray3", rayhit.distance);

        if (rayhit.collider != null)
        {
            if (rayhit.collider.gameObject.CompareTag("Coin"))
            {
                this.SetInput("obj3", 1);
            }
            else if (rayhit.collider.gameObject.CompareTag("Obstacle"))
            {
                this.SetInput("obj3", -1);
            }
            positions.Add(rayhit.point);
        }
        else
        {
            this.SetInput("obj3", 0);
            this.SetInput("ray3", -999);
        }


        float output = this.GetOutput("position");

        GameObject auxP = null;
        if (output < 0.33f)
        {
            auxP = p1;
        }
        else if(output < 0.66f)
        {
            auxP = p2;
        }
        else
        {
            auxP = p3;
        }

        transform.position = auxP.transform.position;

    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Obstacle"))
        {
            EndNetCycle(fitness * fitness);
        }
        else if(col.gameObject.CompareTag("Coin"))
        {
            fitness += CoinPoints;
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
        Debug.Log("Ended Agent -> Fit: " + fitness);
        fitness = 0;
    }

    public override void OnGenerationEndMulti()
    {
        base.OnGenerationEndMulti();
        TempleObstacle[] obs = GameObject.FindObjectsOfType<TempleObstacle>();
        foreach(TempleObstacle o in obs)
        {
            Destroy(o.gameObject);
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

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(p1.transform.position- Vector3.forward * rayOffset, p1.transform.position + Vector3.forward * 100);
        Gizmos.DrawLine(p2.transform.position- Vector3.forward * rayOffset, p2.transform.position + Vector3.forward * 100);
        Gizmos.DrawLine(p3.transform.position - Vector3.forward * rayOffset, p3.transform.position + Vector3.forward * 100);
        foreach(Vector3 p in positions)
        {
            Gizmos.DrawWireSphere(p, 0.1f);
        }
    }

}
