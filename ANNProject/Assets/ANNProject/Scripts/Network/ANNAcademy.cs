using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ANNAcademy : MonoBehaviour {

    public GameObject AgentPrefab = null;
    //public ANNNetwork.ANNNetworkTrainingType TrainingType = ANNNetwork.ANNNetworkTrainingType.MultiAgent;

    private ANNNetwork base_net = null;
    private ANNAgent base_agent = null;

    private List<GameObject> childrenGOs = new List<GameObject>();

	// Use this for initialization
	void Start () {
        if (Application.isPlaying)
        {
            base_agent = AgentPrefab.GetComponent<ANNAgent>();
            if (base_agent == null)
            {
                Debug.LogError("ANNAcademy => Trying to use an AgentPrefab without an ANNAgent Component.");
                return;
            }
            base_net = base_agent.network;
            base_net.topBest = 0;

            switch (base_net.TrainingType)
            {
                case ANNNetwork.ANNNetworkTrainingType.MultiAgent:          // MULTI
                    ReplicateAgent(AgentPrefab);
                    break;
                case ANNNetwork.ANNNetworkTrainingType.SingleAgent:         // SINGLE
                    AgentPrefab.SetActive(true);
                    StartAgent(base_agent);
                    break;
            }
        }
        else if(Application.isEditor)
        {
            if(AgentPrefab != null)
            {
                base_agent = AgentPrefab.GetComponent<ANNAgent>();
                if(base_agent != null)
                {
                    base_net = base_agent.network;
                    base_net.LoadNetwork(true, true);
                }
            }
        }

    }

    // Update is called once per frame
    void Update () {
        if (Application.isPlaying)
        {
            switch (base_net.TrainingType)
            {
                case ANNNetwork.ANNNetworkTrainingType.MultiAgent:          // MULTI
                    if (HaveAllChildrenEnded() == true)
                    {
                        base_agent.Start();
                        base_agent.OnGenerationEndMulti();
                        base_net.StepToNextGeneration();
                        ReplicateAgent(AgentPrefab);
                    }
                    break;
                case ANNNetwork.ANNNetworkTrainingType.SingleAgent:         // SINGLE

                    break;
            }
        }
	}

    private void StartAgent(ANNAgent agent)
    {
        agent.StartAgent();
        agent.Start();
    }

    private void ReplicateAgent(GameObject original)
    {
        if (original == null) return;

        DestroyAllChildren();
        original.SetActive(true);
        for(int i = 0; i < base_net.AgentsPerGeneration; ++i)
        {
            GameObject agentObject = GameObject.Instantiate<GameObject>(original);
            childrenGOs.Add(agentObject);
        }
        for(int i = 0; i < childrenGOs.Count; ++i)
        {
            ANNAgent agent = childrenGOs[i].GetComponent<ANNAgent>();
            agent.network = base_net.Instanciate();
            if(i == 0 || i == 1)
            {
                agent.network.loadFromBest = true;
            }
            StartAgent(agent);
        }
        original.SetActive(false);
    }

    // END GEN CONDITIONS //

    private bool HaveAllChildrenEnded()
    {
        for(int i = 0; i < childrenGOs.Count; ++i)
        {
            if(childrenGOs[i].GetComponent<ANNAgent>().ended == false)
            {
                return false;
            }
        }
        return true;
    }

    // CLEAN UP

    private void DestroyAllChildren()
    {
        for(int i = 0; i < childrenGOs.Count; ++i)
        {
            Destroy(childrenGOs[i]);
        }
        childrenGOs.Clear();
    }


}

/*[CustomEditor(typeof(ANNAcademy))]
public class ANNAcademyInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        ANNAcademy academy = (ANNAcademy)target;

        GameObject auxAgent =  (GameObject)EditorGUILayout.ObjectField("Agent: ", academy.agentGO, typeof(GameObject), true);
        if(auxAgent != academy.agentGO)
        {
            if(auxAgent.GetComponent<ANNAgent>() == null)
            {
                Debug.LogError("Trying to Set the Agent of an ANNAcademy but it has no 'ANNAgent' component");
                return;
            }
            academy.agentGO = auxAgent;
        }
        academy.TrainingType = (ANNNetwork.ANNNetworkTrainingType)EditorGUILayout.EnumPopup("Training Type:", academy.TrainingType);
    }
}*/
