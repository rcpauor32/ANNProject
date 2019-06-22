using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class ANNAgent : MonoBehaviour {

    [HideInInspector]
    public ANNNetwork network = null;

    [HideInInspector]
    public bool ended = false;

    // SINGLE AGENT VARS ---
    private int singleCycles = 0;

    // Use this for initialization
    public virtual void Start () {
        singleCycles = 0;
	}
	
    public virtual void StartAgent()
    {
        if(network != null)
        {
            network.agent = this;
            network.StartNetwork();
        }
    }

	// Update is called once per frame
	public virtual void Update () {
		
	}

    #region // ANN Specifics ---

    // Set Inputs ---
    public void SetInput(ANNInputNode node, float value)
    {
        if (node == null)
        {
            Debug.LogWarning(this.name + " => Trying to SetInput(node, value) of a 'null' node");
            return;
        }
        if(network == null)
        {
            Debug.LogWarning(this.name + " => Trying to SetInput(node, value) with a 'null' network reference");
            return;
        }
        network.SetInput(node, value);
    }
    public void SetInput(string nodeName, float value)
    {
        if (network == null)
        {
            Debug.LogWarning(this.name + " => Trying to SetInput(name, value) with a 'null' network reference");
            return;
        }
        network.SetInput(nodeName, value);
    }

    // Get Outputs ---
    public float GetOutput(ANNOutputNode node)
    {
        if (network == null)
        {
            Debug.LogWarning(this.name + " => Trying to GetOutput(node) with a 'null' network reference");
            return ANNErrorCodes.OUTPUT_ERROR_VALUE;
        }
        return network.GetOutput(node);
    }
    public float GetOutput(string nodeName)
    {
        if (network == null)
        {
            Debug.LogWarning(this.name + " => Trying to GetOutput(name) with a 'null' network reference");
            return ANNErrorCodes.OUTPUT_ERROR_VALUE;
        }
        return network.GetOutput(nodeName);
    }

    // End Cycle ---
    public void EndNetCycle(float fitnessAssigned)
    {
        if(network == null)
        {
            Debug.LogWarning(this.name + " => Trying to EndCycle(float) of a 'null' network");
            return;
        }
        network.EndCycle(fitnessAssigned);
        switch(network.TrainingType)
        {
            case ANNNetwork.ANNNetworkTrainingType.MultiAgent:
                OnAgentEndMulti();
                break;
            case ANNNetwork.ANNNetworkTrainingType.SingleAgent:
                OnAgentEndSingle();
                singleCycles++;
                if(singleCycles > network.AgentsPerGeneration)
                {
                    this.OnGenerationEndSingle();
                    this.network.StepToNextGeneration();
                    this.StartAgent();
                    this.Start();
                    singleCycles = 0;
                }
                break;
        }
    }

    // On Ended ---
    public virtual void OnGenerationEndMulti()          // MULTI
    {
        ANNSerialization.TP.DeleteGeneration(this.network.Parent().name, this.network.Parent().generation + 1);
        gameObject.SetActive(true);
    }

    public virtual void OnGenerationEndSingle()         // SINGLE
    {
        ANNSerialization.TP.DeleteGeneration(this.network.Parent().name, this.network.Parent().generation + 1);
    }

    public virtual void OnAgentEndMulti()               // MULTI
    {
        ended = true;
    }

    public virtual void OnAgentEndSingle()              // SINGLE
    {

    }

    #endregion
}

[CustomEditor(typeof(ANNAgent), true)]
public class ANNBrainComponentEditor : Editor
{
    public bool showNetInspector = true;

    public override void OnInspectorGUI()
    {
        if (target.GetType() != typeof(ANNAgent))
        {
            GUILayout.Label("       ------ Script ------", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }

        GUILayout.Label("       ------ Agent ------", EditorStyles.boldLabel);

        ANNAgent comp = (ANNAgent)target;
        ANNNetwork auxNet = null;
        auxNet = (ANNNetwork)EditorGUILayout.ObjectField("Network:", comp.network, typeof(ANNNetwork), true);
        if (auxNet != comp.network)
        {
            comp.network = auxNet;
            if (auxNet != null)
            {
                comp.network.agent = comp;
            }
        }

        GUILayout.Label("       ------ Network ------", EditorStyles.boldLabel);
        showNetInspector = EditorGUILayout.Toggle("Show Network Inpector:", showNetInspector);
        if (showNetInspector)
        {
            if (comp.network != null)
            {
                ANNNetworkInspector.DrawInspector(comp.network);
            }
        }

    }
}
