using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hidden Node", menuName = "ANNProject - Node/Hidden")]
public class ANNHiddenNode : ANNNode {

    public int layer = (int)ANNLayerOrders.HiddenBegin;
    public override int GetLayerOrder()
    {
        return layer;
    }

    /// Methods ///
    public override void SetupOutputConnections(bool randomWeights = true)
    {
        base.SetupOutputConnections(randomWeights);

        if(layer < (int)ANNLayerOrders.HiddenBegin)
        {
            Debug.LogWarning(this.name + " => Trying to SetupOutputConnections(bool) but layerOrder is unknown");
            return;
        }

        int nextLayerOrder = layer + 1;
        if (network.NHiddenLayers + (int)ANNLayerOrders.HiddenBegin == nextLayerOrder)
        {
            nextLayerOrder = (int)ANNLayerOrders.Output;
            List<ANNOutputNode> nextLayer = network.OutputNodes;
            foreach (ANNOutputNode nextNode in nextLayer)
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
            List<ANNHiddenNode> nextLayer = network.GetHiddenNodesFromLayerOrder(nextLayerOrder);
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

    /// Utils ///
    public override void Copy(ANNNode target)
    {
        if(target.GetType() != typeof(ANNHiddenNode))
        {
            Debug.LogWarning(this.name + " => Trying to Copy(node) a node that is not a HiddenNode");
            return;
        }
        base.Copy(target);
    }
}
