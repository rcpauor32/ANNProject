using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ANNThoughtProcess {

    public ANNNetwork net = null;
    public float probability;
    public float fitness;
    public int generation;

    // Thoughts //
    public List<ANNNodeThought> thoughts = new List<ANNNodeThought>();

    /// Methods ///
    // Read ---
    public void Read(ANNNetwork net, float fitness)
    {
        // Net ---
        this.net = net.Parent();
        this.fitness = fitness;
        generation = net.generation;
        // Nodes ---
        // input nodes --> not needed for the Thought Processes
        // hidden nodes
        for(int i = 0; i < net.HiddenNodes.Count; ++i)
        {
            thoughts.Add(new ANNNodeThought(net.HiddenNodes[i]));
        }
        // output nodes
        for(int i = 0; i < net.OutputNodes.Count; ++i)
        {
            thoughts.Add(new ANNNodeThought(net.OutputNodes[i]));
        }
    }

    // Serialize ---
    
    public void Serialize()
    {
        ANNSerialization.TP.Serialize(this);
    }
}

public class ANNNodeThought
{
    public ANNNodeThought(ANNNode node)
    {
        this.Read(node);
    }
    public ANNNodeThought()
    {} 

    public float bias;
    public List<float> weights = new List<float>();

    public void Read(ANNNode n)
    {
        if(n == null)
        {
            Debug.LogWarning("ANNodeThought => Trying to Read(node) a 'null' node");
            return;
        }
        bias = n.Bias;
        for(int i = 0; i < n.inputConnections.Count; ++i)
        {
            weights.Add(n.inputConnections[i].weight);
        }
    }
    /*public string asString()
    {
        // node bias 
        string str = ANNSerialization.GLOBALS.TP.biasMarker + bias;
        // node weights
        str += ANNSerialization.GLOBALS.TP.weightMarker;
        for(int i = 0; i < weights.Count; ++i)
        {
            str += weights[i] + ANNSerialization.GLOBALS.TP.weightSeparator;
        }
        return str;
    }*/
}