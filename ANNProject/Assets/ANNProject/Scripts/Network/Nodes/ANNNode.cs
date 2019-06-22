using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ANNNode : ScriptableObject {

    public enum Type
    {
        Input = 0,
        Hidden,
        Output
    }

    private string uid;
    public string UID
    {
        get
        {
            return uid;
        }
    }

    /// Constructor / Destructor ///
    public ANNNode()
    { 
       
    }
    public ANNNode(ANNNetwork net)
    {
        network = net;
    }
    ~ANNNode()
    { }

    public void OnEnable() // Scriptable Object Init 
    {
        
    }


    /// Variables ///
    // References ---
    public ANNNetwork network = null; // Parent Network
    public string netuid;

    // Values ---
    public float Bias = 0;

    public virtual int GetLayerOrder()
    {
        return (int)ANNLayerOrders.Unknown;
    }

    // Activation Delegate ---
    public delegate float ActivationFunction(float value); // Activation Computation Delegate
    public ActivationFunction ActivationDelegate = ANNActivationMethods.SigmoidActivation;
    public ANNActivationMethodsList ActivationMethodType = ANNActivationMethodsList.Sigmoid;

    // Connections ---
    public List<ANNConnection> inputConnections  = new List<ANNConnection>();
    public List<ANNConnection> outputConnections = new List<ANNConnection>();

    /// INSTANCES COUNT ///
    public List<ANNNode> instances = new List<ANNNode>();
    public ANNNode instanceParent = null;
    public string parentuid;

    /// Methods ///
    // Instance Parent ---
    public ANNNode Parent()
    {
        return this.instanceParent == null ? this : this.instanceParent;
    }
    // Tick ---
    public void Tick()
    {
        float activation = ComputeActivationValue();
        for(int i = 0; i < outputConnections.Count; ++i)
        {
            outputConnections[i].input = activation;
        }
    }
    // Setup ---
    public virtual void SetupOutputConnections(bool randomWeights = true)
    {
        if(network == null)
        {
            Debug.LogWarning(this.name + " => Trying to SetupOutputConnection(bool) with a 'null' network");
            return;
        }
        outputConnections.Clear();
    }

    // Computations ---
    // Activation --
    public virtual float ComputeActivationValue()
    {
        float result = 0;
        // Weighted Sum --- WS = E(Wn * In) + bias
        for (int i = 0; i < inputConnections.Count; ++i)
        {
            if (inputConnections[i] == null || inputConnections[i].inNode == null) continue;
            result += inputConnections[i].weight * inputConnections[i].input;
        }
        result += Bias;
        // Activation Function ---
        return ActivationDelegate(result);
    }

    // CleanUp ---
    public void CleanUpConnections(bool clearInCons = true, bool clearOutCons = true)
    {
        if(clearInCons)
        {
            inputConnections.Clear();
        }
        if(clearOutCons)
        {
            outputConnections.Clear();
        }
    }

    /// SERIALIZATION ///
    // UID ---
    private void GenerateUID()
    {
        uid = ANNMathHelpers.GenerateUID("node");
    }

    /// UTILS ///
    public virtual ANNNode Instanciate()
    {
        ANNNode n = ScriptableObject.CreateInstance<ANNNode>();
        n.Copy(this);
        n.OnEnable();
        this.instances.Add(n);
        n.instanceParent = this.instanceParent == null ? this : this.instanceParent;
        return n;
    }
    #region // Instanciate() - specifics
    public ANNInputNode InstanciateAsInput()
    {
        ANNInputNode n = ScriptableObject.CreateInstance<ANNInputNode>();
        n.Copy(this);
        n.OnEnable();
        this.instances.Add(n);
        n.instanceParent = Parent();
        return n;
    }
    public ANNOutputNode InstanciateAsOutput()
    {
        ANNOutputNode n = ScriptableObject.CreateInstance<ANNOutputNode>();
        n.Copy(this);
        n.OnEnable();
        this.instances.Add(n);
        n.instanceParent = Parent();
        return n;
    }
    public ANNHiddenNode InstanciateAsHidden()
    {
        ANNHiddenNode n = ScriptableObject.CreateInstance<ANNHiddenNode>();
        n.Copy(this);
        n.OnEnable();
        this.instances.Add(n);
        n.instanceParent = Parent();
        return n;
    }
    #endregion

    public virtual void Copy(ANNNode target)
    {
        this.name = target.name + GetNextNumAppendixFromName(target.name);
        network = target.network;
        Bias = target.Bias;
        ActivationMethodType = target.ActivationMethodType;
        ActivationDelegate = target.ActivationDelegate;
    }
    string GetNextNumAppendixFromName(string name)
    {
        return "";
    }
}

[CustomEditor(typeof(ANNNode), true)]
public class ANNNodeInspector : Editor
{

    public override void OnInspectorGUI()
    {
        ANNNode node = (ANNNode)target;

        EditorGUILayout.ObjectField("Network:", node.network, typeof(ANNNetwork), false);
        node.Bias = EditorGUILayout.FloatField("Bias Value:", node.Bias);

        ANNActivationMethodsList auxActivationMethod = node.ActivationMethodType;

        node.ActivationMethodType = (ANNActivationMethodsList)EditorGUILayout.EnumPopup("Activation Method:", node.ActivationMethodType);

        if(auxActivationMethod != node.ActivationMethodType)
        {
            auxActivationMethod = node.ActivationMethodType;
            node.network.Serialize();
        }

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Apply to Layer"))
        {
            ApplyActivationToLayer();
            node.network.Serialize();
        }
        if(GUILayout.Button("Apply to All Nodes"))
        {
            ApplyActivationToAllNodes();
            node.network.Serialize();
        }
        EditorGUILayout.EndHorizontal();
    }

    void ApplyActivationToLayer()
    {
        ANNNode node = (ANNNode)target;
        if (node == null || node.network == null)
        {
            return;
        }

        if (node.GetType() == typeof(ANNInputNode))
        {
            List<ANNInputNode> list = node.network.InputNodes;
            for(int i = 0; i < list.Count; ++i)
            {
                list[i].ActivationMethodType = node.ActivationMethodType;
            }
        }
        else if(node.GetType() == typeof(ANNOutputNode))
        {
            List<ANNOutputNode> list = node.network.OutputNodes;
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].ActivationMethodType = node.ActivationMethodType;
            }
        }
        else if(node.GetType() == typeof(ANNHiddenNode))
        {
            List<ANNHiddenNode> list = node.network.GetHiddenNodesFromLayerOrder(node.GetLayerOrder());
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].ActivationMethodType = node.ActivationMethodType;
            }
        }
    }
    void ApplyActivationToAllNodes()
    {
        ANNNode node = (ANNNode)target;
        if(node == null || node.network == null)
        {
            return;
        }
        for(int i = 0; i < node.network.nodeList.Count; ++i)
        {
            node.network.nodeList[i].ActivationMethodType = node.ActivationMethodType;
        }
    }

}
