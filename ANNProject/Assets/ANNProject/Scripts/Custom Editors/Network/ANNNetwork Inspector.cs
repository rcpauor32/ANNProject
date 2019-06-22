using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ANNNetwork))]
public class ANNNetworkInspector : Editor {

    public static bool hideInNodeNames = false;
    public static bool hideOutNodeNames = false;

    public static AnimationCurve curve = new AnimationCurve();
    public static List<float> curveKeys = new List<float>();

    // Add Nodes Vars ---
    public enum NodeType
    {
        Input = 0,
        Output
    }
    static public NodeType nodetype = NodeType.Input;
    static public string nodename = "new node";
    static public int inputnumber = 1;
    static public int outputnumber = 1;

    static public ANNInputNode InNodeAux = null;
    static public ANNOutputNode OutNodeAux = null;

    static public int generationToChange = 0;

    /// OnGUI ///
    public override void OnInspectorGUI()
    {
        if (curve.length <= 0)
            curve.AddKey(-1, 0);
        curve.keys[0].value = 0;

        ANNNetwork net = (ANNNetwork)target;
        DrawInspector(net);
    }

    public static void DrawInspector(ANNNetwork net)
    {
        if (curve.length <= 0)
            curve.AddKey(-1, 0);
        curve.keys[0].value = 0;

        // Cycles per Generation
        net.AgentsPerGeneration = EditorGUILayout.IntField("Cycles per Generation:", net.AgentsPerGeneration);
        // Net State
        net.TrainingType = (ANNNetwork.ANNNetworkTrainingType)EditorGUILayout.EnumPopup("Network Training Type:", net.TrainingType);
        // Hidden Layers
        EditorGUILayout.Space();
        net.NHiddenLayers = EditorGUILayout.IntField("Number of Hidden Layers", net.NHiddenLayers);
        // Hidden Nodes
        net.ManualNHiddenNodes = EditorGUILayout.Toggle("Manual Number of Hidden Nodes:", net.ManualNHiddenNodes);
        if (net.ManualNHiddenNodes)
        {
            net.NHiddenNodesPerLayer = EditorGUILayout.IntField("Number of Hidden Nodes:", net.NHiddenNodesPerLayer);
        }
        // Add Nodes
        GUILayout.Label("-- Add Nodes --", EditorStyles.boldLabel);
        InNodeAux = (ANNInputNode)EditorGUILayout.ObjectField("Add Input Node:", InNodeAux, typeof(ANNInputNode), true);
        //nodetype = (NodeType)EditorGUILayout.EnumPopup("New Node Type: ", nodetype);
        //nodename = EditorGUILayout.TextField("New Node Name: ", nodename);
        GUILayout.BeginHorizontal();
        inputnumber = EditorGUILayout.IntField("Amount to Add: ", inputnumber);
        if (inputnumber < 0) inputnumber = 0;
        if(GUILayout.Button("Add Input Node") && InNodeAux != null)
        {
            if(inputnumber <= 0)
            {
                
            }
            else if(inputnumber == 1)
            {
                net.AddNode(InNodeAux, true, false, false, false);
            }
            else
            {
                for(int i = 0; i < inputnumber; ++i)
                {
                    ANNNode n = net.AddNode(InNodeAux, true, false, false, false);
                    n.name = InNodeAux.Parent().name + "_" + i;
                }
                net.ResetNet();
                net.ResetGeneration();
            }
            InNodeAux = null;
            net.Serialize();
        }
        GUILayout.EndHorizontal();
        if(inputnumber > 99)
        {
            //EditorStyles.colorField = Color.yellow;
            GUILayout.Label("The number of nodes you want to add is too high, could lead to performance issues.", EditorStyles.miniLabel);
        }
        // Add Output Nodes
        OutNodeAux = (ANNOutputNode)EditorGUILayout.ObjectField("Add Output Node:", OutNodeAux, typeof(ANNOutputNode), true);
        GUILayout.BeginHorizontal();
        outputnumber = EditorGUILayout.IntField("Amount to Add: ", outputnumber);
        if (outputnumber < 0) outputnumber = 0;
        if (GUILayout.Button("Add Output Node") && OutNodeAux != null)
        {
            if (outputnumber <= 0)
            {

            }
            else if (outputnumber == 1)
            {
                net.AddNode(OutNodeAux, true, false, false, false);
            }
            else
            {
                for (int i = 0; i < outputnumber; ++i)
                {
                    ANNNode n = net.AddNode(OutNodeAux, true, false, false, false);
                    n.name = OutNodeAux.Parent().name + "_" + i;
                }
                net.ResetNet();
                net.ResetGeneration();
            }
            OutNodeAux = null;
            net.Serialize();
        }
        GUILayout.EndHorizontal();
        if (outputnumber > 99)
        {
            //EditorStyles.colorField = Color.yellow;
            GUILayout.Label("The number of nodes you want to add is too high, could lead to performance issues.", EditorStyles.miniLabel);
        }
        GUILayout.BeginHorizontal();
        EditorGUILayout.IntField("Number of Input Nodes", net.InputNodes.Count, EditorStyles.boldLabel);
        hideInNodeNames = EditorGUILayout.Toggle("Hide Names:", hideInNodeNames);
        GUILayout.EndHorizontal();
        if (!hideInNodeNames)
        {
            for (int i = 0; i < net.InputNodes.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                if (net.InputNodes[i] != null)
                {
                    GUILayout.Label("   - " + net.InputNodes[i].name);
                }
                else
                {
                    GUILayout.Label("   - ¡¡ error reading name !!");
                }
                GUILayout.Space(50f);
                if (GUILayout.Button("Delete", GUILayout.Height(20f), GUILayout.Width(50f)))
                {
                    if (net.InputNodes[i] == null)
                    {
                        net.InputNodes.RemoveAt(i--);
                        continue;
                    }
                    net.RemoveNode(net.InputNodes[i]);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.BeginHorizontal();
        EditorGUILayout.IntField("Number of Output Nodes", net.OutputNodes.Count, EditorStyles.boldLabel);
        hideOutNodeNames = EditorGUILayout.Toggle("Hide Names:", hideOutNodeNames);
        GUILayout.EndHorizontal();
        if (!hideOutNodeNames)
        {
            for (int i = 0; i < net.OutputNodes.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                if (net.OutputNodes[i] != null)
                {
                    GUILayout.Label("   - " + net.OutputNodes[i].name);
                }
                else
                {
                    GUILayout.Label("   - ¡¡ error reading name !!");
                }
                GUILayout.Space(50f);
                if(GUILayout.Button("Delete", GUILayout.Height(20f), GUILayout.Width(50f)))
                {
                    if(net.OutputNodes[i] == null)
                    {
                        net.OutputNodes.RemoveAt(i--);
                        continue;
                    }
                    net.RemoveNode(net.OutputNodes[i]);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        net.tickType = (ANNNetwork.TickType)EditorGUILayout.EnumPopup("Tick Type", net.tickType);
        EditorGUILayout.Separator();
        if (GUILayout.Button("Network Tab", GUILayout.Height(25f)))
        {
            ANNNetworkTab window = ANNNetworkTab.GetWindow<ANNNetworkTab>("ANNetwork Tab");
            window.network = net;
            window.UpdateGraphics();
        }
        if (GUILayout.Button("Save Network", GUILayout.Height(25f)))
        {
            net.Serialize();
        }

        EditorGUILayout.IntField("Generation: ", net.Parent().generation, EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        generationToChange = EditorGUILayout.IntField("Change Generation: ", generationToChange);
        if(GUILayout.Button("Change Generation"))
        {
            net.SetGeneration(generationToChange);
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.FloatField("Best Fitness: ", net.topBest, EditorStyles.label);
        EditorGUILayout.FloatField("Last Fitness: ", curveKeys.Count > 0 ? curveKeys[curveKeys.Count - 1] : 0, EditorStyles.label);

        for (int i = 0; i < curve.length; ++i)
        {
            curve.RemoveKey(i);
        }
        for (int i = 0; i < curveKeys.Count; ++i)
        {
            curve.AddKey(i, curveKeys[i]);
        }
        EditorGUILayout.CurveField(curve, GUILayout.Height(350f));
    }
}
