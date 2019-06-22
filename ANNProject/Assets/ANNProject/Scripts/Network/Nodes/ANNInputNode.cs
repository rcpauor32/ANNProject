using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Node", menuName = "ANNProject - Node/Input")]
public class ANNInputNode : ANNNode
{
    public override int GetLayerOrder()
    {
        return (int)ANNLayerOrders.Input;
    }

    private float inputValue = 0;
    public float GetInputValue()
    { return inputValue; }

    /// Methods ///
    // Setup ---
    public void SetInput(float value)
    {
        inputValue = value;
    }

    public override void SetupOutputConnections(bool randomWeights = true)
    {
        base.SetupOutputConnections(randomWeights);

        List<ANNHiddenNode> nextLayer = network.GetHiddenNodesFromLayerOrder((int)ANNLayerOrders.HiddenBegin);
        if (nextLayer.Count == 0)
        {
            foreach (ANNOutputNode nextNode in network.OutputNodes)
            {
                ANNConnection con = new ANNConnection(
                    randomWeights ? Random.Range(ANNProperties.RandomWeightsRange.x, ANNProperties.RandomWeightsRange.y) : ANNProperties.NonRandomWeightDefaultValue
                    , this
                    , nextNode
                    , ConnectionType.FeedForward);
                con.OnEnable();
                this.outputConnections.Add(con);
                nextNode.inputConnections.Add(con);
            }
        }
        else
        {
            foreach (ANNHiddenNode nextNode in nextLayer)
            {
                ANNConnection con = new ANNConnection(
                    randomWeights ? Random.Range(ANNProperties.RandomWeightsRange.x, ANNProperties.RandomWeightsRange.y) : ANNProperties.NonRandomWeightDefaultValue
                    , this
                    , nextNode
                    , ConnectionType.FeedForward);
                con.OnEnable();
                this.outputConnections.Add(con);
                nextNode.inputConnections.Add(con);
            }
        }
    }

    // Activation ---
    public override float ComputeActivationValue()
    {
        return inputValue; // Input Nodes do not require to use the activation function 
    }

    /// Utils ///
    public override void Copy(ANNNode target)
    {
        if (target.GetType() != typeof(ANNInputNode))
        {
            Debug.LogWarning(this.name + " => Trying to Copy(node) a node that is not an InputNode");
            return;
        }
        base.Copy(target);
    }

}
