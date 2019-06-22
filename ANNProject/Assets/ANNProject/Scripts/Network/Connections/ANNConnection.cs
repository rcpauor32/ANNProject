using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionType
{
    FeedForward = 0,
    Recursive,
}

public class ANNConnection {

    private string uid;
    public string UID
    {
        get
        {
            return uid;
        }
    }

    public ANNConnection(float weight, ANNNode inNode = null, ANNNode outNode = null, ConnectionType type = ConnectionType.FeedForward)
    {
        this.weight  = weight;
        this.outNode = outNode;
        this.inNode  = inNode;
        this.type    = type;
    }
    public ANNConnection()
    { }

    public ANNNode inNode = null;
    public ANNNode outNode = null;

    public float weight = 0;

    public ConnectionType type = ConnectionType.FeedForward;

    public float input = 0;

    public void OnEnable() // Scriptable Object Init 
    {
        GenerateUID();
    }

    private void GenerateUID()
    {
        uid = ANNMathHelpers.GenerateUID("connection");
    }

}
