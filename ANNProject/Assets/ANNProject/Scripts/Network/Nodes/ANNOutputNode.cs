using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Output Node", menuName = "ANNProject - Node/Output")]
public class ANNOutputNode : ANNNode {

    public override int GetLayerOrder()
    {
        return (int)ANNLayerOrders.Output;
    }

    /// Variables ///
    // Output ---
    private float Output = 0;
    public float output
    {
        get
        {
            return Output;
        }
    }

    /// Methods ///
    // Output ---
    public float GetOutput()
    {
        if(network == null)
        {
            Debug.LogWarning(this.name + " => Trying to GetOutput() with 'null' network");
            return ANNErrorCodes.OUTPUT_ERROR_VALUE;
        }
        return Output;
    }

    // Activation ---
    public override float ComputeActivationValue()
    {
        Output = base.ComputeActivationValue();
        return Output;
    }

    /// Utils ///
    public override void Copy(ANNNode target)
    {
        if (target.GetType() != typeof(ANNOutputNode))
        {
            Debug.LogWarning(this.name + " => Trying to Copy(node) a node that is not an OutputNode");
            return;
        }
        base.Copy(target);
    }

}
