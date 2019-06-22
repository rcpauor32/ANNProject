using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New Neural Network", menuName = "ANNProject - Neural Network")]
public class ANNNetwork : ScriptableObject {

    /// USER VARIABLES ------------------------------------------------------------------------------------
    public bool hasEnded
    {
        get
        {
            for(int i = 0; i < agents.Count; ++i)
            {
                if(agents[i].ended == false)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// !_USER VARIABLES ----------------------------------------------------------------------------------

    // Net Basics
    public ANNAgent agent = null;

    private bool LoadFromBest = false;
    public bool loadFromBest
    {
        get
        {
            return LoadFromBest;
        }
        set
        {
            LoadFromBest = value;
        }
    }

    public float initMutationRate = 0.2f;
    public float mutationRate = 0.2f;
    public float mutationRateStep = 0.1f;

    public float percentageOfTopFitness = 0.30f;

    public float topBest = 0;

    public int NHiddenLayers = 1;
    public int NHiddenNodesPerLayer = 0;
    public bool ManualNHiddenNodes = false;

    private int Generation = 0;
    public int generation
    {
        get
        {
            return Parent().Generation;
        }
    }
    // Nodes Basics --- 
    public enum ANNNetworkTrainingType
    {
        MultiAgent,
        SingleAgent
    }
    public ANNNetworkTrainingType TrainingType = ANNNetworkTrainingType.MultiAgent;

    [SerializeField]
    private List<ANNInputNode>  inputNodes  = new List<ANNInputNode>();
    [SerializeField]
    private List<ANNOutputNode> outputNodes = new List<ANNOutputNode>();
    [SerializeField]
    private List<ANNHiddenNode> hiddenNodes = new List<ANNHiddenNode>();

    public List<ANNNode> nodeList
    {
        get
        {
            List<ANNNode> aux = new List<ANNNode>();
            foreach(ANNInputNode n in inputNodes)
            {
                aux.Add(n);
            }
            foreach (ANNOutputNode n in outputNodes)
            {
                aux.Add(n);
            }
            foreach (ANNHiddenNode n in hiddenNodes)
            {
                aux.Add(n);
            }
            return aux;
        }
    }

    // Generation Variables ---
    public int AgentsPerGeneration = 50;

    public ANNGeneticAlgorithms.GeneticAlgorithm GeneticAlgorithmDelegate = ANNGeneticAlgorithms.RandomCrossover;

    // Thought Processes ---
    public List<ANNThoughtProcess> lastGenTPs = new List<ANNThoughtProcess>();
    public List<ANNThoughtProcess> thisGenTPs = new List<ANNThoughtProcess>();

    // Tick Variables ---
    public enum TickType
    {
        TickOnOutput = 0,   // Net->Tick() when asked an Output
        TickOnInput,        // Net->Tick() when given an Input
        ManualTick          // Net does not Tick(), user must use Net->Tick() in his script
    }
    public TickType tickType = TickType.TickOnOutput;

    /// MULTI AGENTS ///
    public List<ANNAgent> agents = new List<ANNAgent>();
    public List<ANNNetwork> children = new List<ANNNetwork>();

    /// INSTANCE COUNT ///
    public List<ANNNetwork> instances = new List<ANNNetwork>();
    public ANNNetwork instanceParent = null;
    bool IsInstance = false;
    public bool isInstance
    {
        get
        {
            return IsInstance;
        }
    }

    #region // Getters -
    public List<ANNInputNode> InputNodes
    {
        get
        {
            return inputNodes;
        }
    }
    public List<ANNOutputNode> OutputNodes
    {
        get
        {
            return outputNodes;
        }
    }
    public List<ANNHiddenNode> HiddenNodes
    {
        get
        {
            return hiddenNodes;
        }
    }
    #endregion
    /// USER METHODS /// --------------------------------------------------------------------------------------------------
    
    // Input ---
    public void SetInput(ANNInputNode node, float value)
    {
        if(node == null)
    
        {
            Debug.LogWarning(this.name + " => Trying to SetInput(node, value) of a 'null' node");
            return;
        }
        if (tickType == TickType.TickOnInput) this.Tick();
        node.SetInput(value);
    }
    public void SetInput(string nodeName, float value)
    {
        ANNInputNode n = this.GetInputNodeFromName(nodeName);
        if(n == null)
        {
            //Debug.LogWarning(this.name + " => Trying to SetInput(name, value) of a non-existing node");
            return;
        }
        if (tickType == TickType.TickOnInput) this.Tick();
        n.SetInput(value);
    }

    // Output ---
    public float GetOutput(ANNOutputNode node)
    {
        if(node == null)
        {
            Debug.LogWarning(this.name + " => Trying to GetOutput(node) of a 'null' node");
            return ANNErrorCodes.OUTPUT_ERROR_VALUE;
        }
        if (tickType == TickType.TickOnOutput) this.Tick(); 
        return node.GetOutput();
        
    }
    public float GetOutput(string nodeName)
    {
        ANNOutputNode n = this.GetOutputNodeFromName(nodeName);
        if(n == null)
        {
            //Debug.LogWarning(this.name + " => Trying to GetInput(name) of a non-existing node");
            return ANNErrorCodes.OUTPUT_ERROR_VALUE;
        }
        if (tickType == TickType.TickOnOutput) this.Tick();
        return n.GetOutput();
    }

    // End Cycle ---
    public void EndCycle(float fitnessAssigned)
    {
        //Debug.Log(this.Agent.GetInstanceID() + " - " + this.GetInstanceID() + " - END");
        switch(Parent().TrainingType)
        {
            case ANNNetworkTrainingType.MultiAgent:
                this.StoreThoughts(fitnessAssigned);
                break;
            case ANNNetworkTrainingType.SingleAgent:
                this.StoreThoughts(fitnessAssigned);
                UpdateNetwork();
                break;
        }
    }

    /// !_USER METHODS /// ------------------------------------------------------------------------------------------------

    public ANNNetwork CreateChild(ANNAgent agent)
    {
        ANNNetwork child = this.Instanciate();
        child.LoadNetwork(false, true);
        child.agent = agent;
        children.Add(child);
        return child;
    }

    /// LIFE CYCLE ///
    public void StartNetwork()
    {
        LoadNetwork(false, true);
    }

    public void UpdateNetwork()
    {
        if (Parent().lastGenTPs.Count == 0)  // SPECIAL CASE: Gen 0 -> it has no 'lastGenTPs'
        {
            if(this.generation == 0)
            {
                ResetNet(); // a.k.a. new random net
                //ResetGeneration();
            }
            else if(this.generation < 0)
            {
                ResetNet();
                SetGeneration(0);
                Debug.LogWarning(this.name + " => Has a generation value of < 0. Gen setted to 0 and reseted the net.");
            }
            else
            {
                //ResetGeneration();
                Parent().lastGenTPs.Clear();
                //foreach (ANNThoughtProcess tp in LoadGeneration(Parent().generation - 1))
                //{
                //    lastGenTPs.Add(tp);
                //}
                Parent().lastGenTPs = LoadGeneration(Parent().generation - 1);
                if (Parent().lastGenTPs.Count > 0)
                {
                    GenerateNetFromGen(Parent().lastGenTPs);
                }
                else
                {
                    ResetNet();
                }
            }
        }
        else
        {
            GenerateNetFromGen(Parent().lastGenTPs);
        }
        ANNSerialization.NETS.SerializeValues(Parent());
    }

    /// ADD NODES ///
    // Create ---
    public ANNNode CreateNode(ANNNode.Type type, string name, bool savenet = true)
    {
        string path = ANNSerialization.GLOBALS.NETS.Path + "/" + Parent().name;
        switch(type)
        {
            case ANNNode.Type.Input:
                {
                    ANNInputNode node = ScriptableObject.CreateInstance<ANNInputNode>();
                    node.name = name;
                    if(!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    AssetDatabase.CreateAsset(node,"Assets/ANNProject/Serialization/Networks/"+ Parent().name + "/" + name + ".asset");
                    AssetDatabase.SaveAssets();
                    AddNode(node, savenet, true, true, true);
                    return node;
                }
            case ANNNode.Type.Hidden:
                {
                    ANNHiddenNode node = ScriptableObject.CreateInstance<ANNHiddenNode>();
                    node.name = name;
                    AssetDatabase.CreateAsset(node, "Assets/ANNProject/Serialization/Networks/" + Parent().name + "/" + name + ".asset");
                    AddNode(node, savenet, true, true, true);
                    return node;
                }
            case ANNNode.Type.Output:
                {
                    ANNOutputNode node = ScriptableObject.CreateInstance<ANNOutputNode>();
                    node.name = name;
                    AssetDatabase.CreateAsset(node, "Assets/ANNProject/Serialization/Networks/" + Parent().name + "/" + name + ".asset");
                    AddNode(node, savenet, true, true, true);
                    return node;
                }
        }
        Debug.LogWarning(this.name + " -> Error Creating Node - Type: '" + type + "' Name: '" + name + "'.");
        return null;
    }
    // Add ---
    public ANNNode AddNode(ANNNode n, bool instanciate = true, bool savenet = true, bool reset = true, bool resetGen = false)
    {
        if (n == null)
        {
            Debug.LogWarning(this.name + " => Trying to AddNode(node, bool) a 'null' node");
            return null;
        }

        ANNNode ret = null;

        if (n.GetType() == typeof(ANNInputNode))
        { ret =  AddInputNode((ANNInputNode)n, instanciate, reset, resetGen); }
        else if (n.GetType() == typeof(ANNOutputNode))
        { ret = AddOutputNode((ANNOutputNode)n, instanciate, reset, resetGen); }
        else if (n.GetType() == typeof(ANNHiddenNode))
        { ret = AddHiddenNode((ANNHiddenNode)n, instanciate); }

        if(savenet)
            Parent().Serialize();
        return ret;
    }
    #region // Adding Nodes - Input, Hidden, Output
    // Input
    private ANNInputNode AddInputNode(ANNInputNode n, bool instanciate = true, bool reset = true, bool resetGen = false)
    {
        ANNInputNode auxN = n.InstanciateAsInput();
        auxN.network = this;
        inputNodes.Add(auxN);
        if (reset)
        {
            ResetNet();
        }
        if (resetGen)
        {
            //ResetGeneration();
        }
        return auxN;
    }
    // Output
    private ANNOutputNode AddOutputNode(ANNOutputNode n, bool instanciate = true, bool reset = true, bool resetGen = false)
    {
        ANNOutputNode auxN = n.InstanciateAsOutput();
        auxN.network = this;
        this.outputNodes.Add(auxN);
        if (reset)
        {
            ResetNet();
            //ResetGeneration();
        }
        if (resetGen)
        {
            ResetGeneration();
        }
        return auxN;
    }
    // Hidden
    private ANNHiddenNode AddHiddenNode(ANNHiddenNode n, bool instanciate = true, bool reset = true)
    {
        ANNHiddenNode auxN = n.InstanciateAsHidden();
        auxN.network = this;
        this.hiddenNodes.Add(auxN);
        SetupConnections(ANNProperties.SetupWithRandomWeights);
        return auxN;
    }
    #endregion

    // REMOVE NODES
    public void RemoveNode(ANNNode node)
    {
        if(node == null)
        {
            Debug.LogWarning(this.name + " -> Trying to remove a 'null' node.");
            return;
        }

        bool removed = false;

        if(node.GetType() == typeof(ANNInputNode))
        {
            for(int i = 0; i < inputNodes.Count; ++i)
            {
                if(inputNodes[i] == node)
                {
                    inputNodes.RemoveAt(i--);
                    removed = true;
                }
            }
        }
        else if (node.GetType() == typeof(ANNHiddenNode))
        {
            for (int i = 0; i < hiddenNodes.Count; ++i)
            {
                if (hiddenNodes[i] == node)
                {
                    hiddenNodes.RemoveAt(i--);
                    removed = true;
                }
            }
        }
        else if (node.GetType() == typeof(ANNOutputNode))
        {
            for (int i = 0; i < outputNodes.Count; ++i)
            {
                if (outputNodes[i] == node)
                {
                    outputNodes.RemoveAt(i--);
                    removed = true;
                }
            }
        }

        if(removed == false)
            Debug.LogWarning(this.name + " -> Tried to remove a node '" + node.name + "' that is not in this net.");
        else
        {
            string path = "Assets/ANNProject/Serialization/Networks/" + Parent().name;
            if(Directory.Exists(path))
            {
                AssetDatabase.DeleteAsset("Assets/ANNProject/Serialization/Networks/" + Parent().name + "/" + node.name + ".asset");
            }
            if(Application.isEditor)
            {
                DestroyImmediate(node);
            }
            else
            {
                Destroy(node);
            }
        }

        SetupConnections();

        this.Serialize();

    }

    /// LOAD ///
    public void LoadNetwork(bool loadNet = true, bool loadNodes = true)
    {
        if(loadNet)
        {
            string path = ANNSerialization.GLOBALS.NETS.Path + "/" + Parent().name + "/" +ANNSerialization.GLOBALS.NETS.NetValuesFileName;
            if (!File.Exists(path))
            {
                Debug.LogWarning(this.name + " => Trying to LoadNetworkValues but there isn't any file serialized from this network.");
            }
            else
            {
                string file = File.ReadAllText(path);
                ANNSerialization.DATA_STRUCTS.NetworkDataStruct net_data = ANNSerialization.NETS.LoadValues(file);
                Parent().SetGeneration(net_data.generation);
            }
        }
        if(loadNodes)
        {
            LoadNodes(0);
        }
    }
    public void LoadNodes(int first_index)
    {
        inputNodes.Clear();
        outputNodes.Clear();
        hiddenNodes.Clear();

        string path = ANNSerialization.GLOBALS.NETS.Path + "/" + Parent().name + "/" + Parent().name;
        if(!File.Exists(path))
        {
            Debug.LogWarning(this.name + " => Trying to LoadNodes() but there isn't any file serialized from this network.");
            return;
        }
        string file = File.ReadAllText(path);
        string str = "";
        for(int i = first_index; i < file.Length; ++i)
        {
            if (file[i] == ANNSerialization.GLOBALS.NETS.NodeSeparator[0])
            {
                ANNNode node = ANNSerialization.NODES.Load(str);
                AddNode(node, true, false, true, false);
                str = ""; // reset str
            }
            else
            {
                str += file[i];
            }
        }

        UpdateNetwork();

    }

    public List<ANNThoughtProcess> LoadGeneration(int gen)
    {
        List<ANNThoughtProcess> ret = new List<ANNThoughtProcess>();
        if (gen <= 0)
        {
            Debug.LogWarning(this.name + " => Trying to LoadGeneration(int gen) with an invalid gen number (Note: gen <= 0 is invalid). Gen -> " + gen);
            return ret;
        }
        string path = ANNSerialization.GLOBALS.TP.Path(Parent().name) + "/" + ANNSerialization.GLOBALS.TP.GenFolderName + gen;
        if (!Directory.Exists(path))
        {
            Debug.LogWarning(this.name + " => Trying to LoadGeneration(int gen) but there isn't any file serialized from this network.");
            return ret;
        }
        for (int index = 0; File.Exists(path + "/" + index.ToString()); ++index)
        {
            ANNThoughtProcess tp = new ANNThoughtProcess();
            string file = File.ReadAllText(path + "/" + index.ToString());
            string str = "";
            char marker = ' ';
            for (int i = 0; i < file.Length; ++i)
            {
                if (marker == ' ' && file[i] == ANNSerialization.GLOBALS.TP.TPMarker[0])
                {
                    marker = ANNSerialization.GLOBALS.TP.TPMarker[0];
                }
                else if (marker == ANNSerialization.GLOBALS.TP.TPMarker[0] && file[i] == ANNSerialization.GLOBALS.TP.ThoughtMarker[0])
                {
                    marker = ANNSerialization.GLOBALS.TP.ThoughtMarker[0];
                }
                else
                {
                    if (marker == ANNSerialization.GLOBALS.TP.TPMarker[0])
                    {
                        if (file[i] == ANNSerialization.GLOBALS.TP.TPSeparator[0])
                        {
                            ANNSerialization.DATA_STRUCTS.TPDataStruct tp_data = ANNSerialization.TP.LoadData(str);
                            tp_data.Write(ref tp);
                            str = "";
                        }
                        else
                        {
                            str += file[i];
                        }
                    }
                    else if (marker == ANNSerialization.GLOBALS.TP.ThoughtMarker[0])
                    {
                        if (file[i] == ANNSerialization.GLOBALS.TP.ThoughtSeparator[0])
                        {
                            if (tp != null)
                            {
                                ANNNodeThought nt = ANNSerialization.THOUGHTS.Load(str);
                                tp.thoughts.Add(nt);
                                str = "";
                            }
                        }
                        else
                        {
                            str += file[i];
                        }
                    }
                }
            }
            if(tp != null)
            {
                ret.Add(tp);
            }
        }
        return ret;
    }

    public void LoadGenerationIntoNet(int gen)
    {
        if(gen == 0)
        {
            Parent().Generation = 0;
            Parent().UpdateNetwork();
        }

        string path = ANNSerialization.GLOBALS.TP.Path(Parent().name) + "/" + ANNSerialization.GLOBALS.TP.GenFolderName + gen;
        if(!Directory.Exists(path))
        {
            Debug.LogWarning(this.name + " => Trying to LoadGeneration(gen) but there isn't any file serialized from this network.");
            Debug.Log(path);
            return;
        }
        thisGenTPs.Clear();
        lastGenTPs.Clear();
        for(int index = 0; File.Exists(index.ToString()); ++index)
        {
            ANNThoughtProcess tp = null;
            string file = File.ReadAllText(path + "/" + index.ToString());
            string str = "";
            char marker = ' ';
            for(int i = 0; i < file.Length; ++i)
            {
                if (file[i] == ANNSerialization.GLOBALS.TP.TPMarker[0]) marker = file[i];
                else if (file[i] == ANNSerialization.GLOBALS.TP.ThoughtMarker[0]) marker = file[i];
                else
                {
                    if(marker == ANNSerialization.GLOBALS.TP.TPMarker[0])
                    {
                        if(file[i] == ANNSerialization.GLOBALS.TP.TPSeparator[0])
                        {
                            tp = ANNSerialization.TP.Load(str);

                        }
                        else
                        {
                            str += file[i];
                        }
                    }
                    else if(marker == ANNSerialization.GLOBALS.TP.ThoughtMarker[0])
                    {
                        if (file[i] == ANNSerialization.GLOBALS.TP.ThoughtSeparator[0])
                        {
                            ANNNodeThought nt = ANNSerialization.THOUGHTS.Load(str);
                            tp.thoughts.Add(nt);
                            str = "";
                        }
                        else
                        {
                            str += file[i];
                        }
                    }
                }
            } 

            if(tp != null)
            {
                thisGenTPs.Add(tp);
            }
        }
        StepToNextGeneration();
        UpdateNetwork();
        
    }

    /// Setup ///
    public void ResetNet()
    {
        //Generation = 0;
        mutationRate = initMutationRate;
        int nHdn = this.ManualNHiddenNodes == true ? 
            this.NHiddenNodesPerLayer
            : (int)Mathf.Pow(inputNodes.Count + outputNodes.Count, 0.5f) + 1;
        hiddenNodes.Clear();
        for(int li = 0; li < NHiddenLayers; ++li)
        {
            for(int ni = 0; ni < nHdn; ++ni)
            {
                ANNHiddenNode hdn = ScriptableObject.CreateInstance<ANNHiddenNode>();
                hdn.name = "H-" + li + ":" + ni;
                hdn.network = this;
                hdn.Bias = Random.Range(ANNProperties.BiasRandomRange.x, ANNProperties.BiasRandomRange.y);
                hdn.layer = li;
                this.hiddenNodes.Add(hdn);
            }
        }
        for(int i = 0; i < outputNodes.Count; ++i)
        {
            outputNodes[i].Bias = Random.Range(ANNProperties.BiasRandomRange.x, ANNProperties.BiasRandomRange.y);
        }
        SetupConnections();
        //if (outputNodes.Count > 0)
        //    Debug.Log(this.GetInstanceID() + " ==> BIAS ==> " + outputNodes[0].Bias + " in POSTCONNECTIONS");
        ANNNetworkTab.networkChanged = true;
    }

    // Connections Setup ---
    public void SetupConnections(bool randomWeights = true, bool clearConnections = true)
    {
        if(clearConnections)
        {
            CleanUpConnections(true, true);
        }
        foreach(ANNInputNode n in inputNodes)
        {
            n.SetupOutputConnections(randomWeights);
        }
        foreach (ANNHiddenNode n in hiddenNodes)
        {
            n.SetupOutputConnections(randomWeights);
        }
        foreach (ANNOutputNode n in outputNodes)
        {
            n.SetupOutputConnections(randomWeights);
        }
    }

    /// Serialization ///
    
    /// Thought Processes ///
    public void StoreThoughts(float fitness)
    {
        ANNThoughtProcess tp = new ANNThoughtProcess();
        tp.Read(this, fitness);
        tp.Serialize();
        Parent().thisGenTPs.Add(tp);
    }

    public List<ANNThoughtProcess> AdjustThoughtsProbability(List<ANNThoughtProcess> listOfThoughts)
    {
        // Compute the new total fitness
        float newTotal = 0;
        for(int i = 0; i < listOfThoughts.Count; ++i)
        {
            for(int j = 0; j < listOfThoughts.Count; ++j)
            {
                if(listOfThoughts[i].fitness > listOfThoughts[j].fitness)
                {
                    ANNThoughtProcess aux = listOfThoughts[i];
                    listOfThoughts[i] = listOfThoughts[j];
                    listOfThoughts[j] = aux;
                }
            }
        }

        int nThoughts = (int)(listOfThoughts.Count * percentageOfTopFitness);

        List<float> auxfit = new List<float>();

        for (int i = 0; i < nThoughts; ++i)
        {
            auxfit.Add(listOfThoughts[i].fitness * listOfThoughts[i].fitness);
            newTotal += auxfit[i];
        }
        // Readjust thoughts probs
        for(int i = 0; i < listOfThoughts.Count; ++i)
        {
            if (i < nThoughts)
                listOfThoughts[i].probability = (auxfit[i] / newTotal) * 100f; // '* 100' to be between 0 - 100, not 0 - 1
            else
                listOfThoughts[i].probability = 0;
        }
        return listOfThoughts;
    }

    /// Methods ///
    // Tick Network ---
    public void Tick() // [TODO] -> Make a Thread and make a method to know if the Thread finished (w\ bool or event)
    {
        // Inputs ---
        for(int i = 0; i < inputNodes.Count; ++i)
        {
            inputNodes[i].Tick();
        }
        // Hidden ---
        for(int i = 0; i < this.NHiddenLayers; ++i)
        {
            List<ANNHiddenNode> hdnLayer = GetHiddenNodesFromLayerOrder(i + (int)ANNLayerOrders.HiddenBegin);
            for(int it = 0; it < hdnLayer.Count; ++it)
            {
                hdnLayer[i].Tick();
            }
        }
        // Output ---
        for(int i = 0; i < outputNodes.Count; ++i)
        {
            outputNodes[i].Tick();
        }
    }

    // Next Gen ---
    public void GenerateNetFromGen(List<ANNThoughtProcess> lastGen)
    {
        List<ANNThoughtProcess> parentTPs = GetTPPair(lastGen);
        ANNThoughtProcess childTP = GeneticAlgorithmDelegate(parentTPs[0], parentTPs[1], mutationRate);
        this.AdjustValuesFromTP(childTP);
    }
    public void AdjustValuesFromTP(ANNThoughtProcess tp)
    {
        if(tp == null)
        {
            Debug.LogWarning(this.name + " => Trying to AdjustValuesFromTP(thoughtprocess) with a 'null' thoughtprocess");
            return;
        }
        int auxMaxIt = 0;
        int auxMinIt = 0;
        for (int ti = 0; ti < tp.thoughts.Count; ++ti)
        {
            // input nodes -> not needed to adjust
            // hidden nodes
            auxMaxIt = this.hiddenNodes.Count;
            auxMinIt = 0;
            if(ti >= auxMinIt && ti < auxMaxIt)
            {
                int ni = ti - auxMinIt;
                // iterate on HN
                hiddenNodes[ni].Bias = tp.thoughts[ti].bias;
                for(int wi = 0; wi < tp.thoughts[ti].weights.Count; ++wi)
                {
                    hiddenNodes[ni].inputConnections[wi].weight = tp.thoughts[ti].weights[wi];
                }
            }
            // output nodes
            auxMaxIt = tp.thoughts.Count;
            auxMinIt = this.hiddenNodes.Count;
            if(ti >= auxMinIt && ti < auxMaxIt)
            {
                int ni = ti - auxMinIt;
                // iterate on ON
                outputNodes[ni].Bias = tp.thoughts[ti].bias;
                for (int wi = 0; wi < tp.thoughts[ti].weights.Count; ++wi)
                {
                    outputNodes[ni].inputConnections[wi].weight = tp.thoughts[ti].weights[wi];
                }
            }
        }
    }

    public void StepToNextGeneration()
    {
        Parent().Generation += 1;
        lastGenTPs.Clear();
        for(int i = 0; i < thisGenTPs.Count; ++i)
        {
            lastGenTPs.Add(thisGenTPs[i]);
        }
        thisGenTPs.Clear();
        float thisGenBest = 0;
        for(int i = 0; i < lastGenTPs.Count; ++i)
        {
            if(lastGenTPs[i].fitness > thisGenBest)
            {
                thisGenBest = lastGenTPs[i].fitness;
            }
        }
        if (topBest < thisGenBest) topBest = thisGenBest;
        lastGenTPs = AdjustThoughtsProbability(lastGenTPs);
        ANNNetworkInspector.curveKeys.Add(thisGenBest);
        /*
        if (topBest != 0)
        {
            initMutationRate = 0.1f;
            mutationRate = initMutationRate * topBest / best;
            Debug.Log(topBest + " / " + best + " = " + topBest/best + " -> " + initMutationRate + " => " + mutationRate);
        }
        if (best > topBest) topBest = best;
        */
        initMutationRate = 0.2f;
        if (ANNNetworkInspector.curveKeys.Count >= 2)
        {
            float last = ANNNetworkInspector.curveKeys[ANNNetworkInspector.curveKeys.Count - 2];
            if ( last <= thisGenBest)
            {
                mutationRate *= (1 - mutationRateStep * (thisGenBest / last));
            }
            else
            {
                mutationRate *= (1 + mutationRateStep * (last / thisGenBest));
            }
        }
        else
        {
            mutationRate = initMutationRate;
        }
        if (mutationRate > 0.995) mutationRate = 0.995f;
        if (mutationRate < 0.005) mutationRate = 0.005f;
    }

    public List<ANNThoughtProcess> GetTPPair(List<ANNThoughtProcess> TPList)
    {
        if (loadFromBest == false)
        {
            List<ANNThoughtProcess> TPPair = new List<ANNThoughtProcess>();
            TPPair.Add(GetTPFromProbInList(TPList));
            List<ANNThoughtProcess> auxList = new List<ANNThoughtProcess>(TPList);
            auxList.Remove(TPPair[0]);
            auxList = AdjustThoughtsProbability(auxList);
            TPPair.Add(GetTPFromProbInList(auxList));
            return TPPair;
        }
        else
        {
            return TPList.GetRange(0, 2);
        }
    }
    private ANNThoughtProcess GetTPFromProbInList(List<ANNThoughtProcess> list)
    {
        if (list.Count == 0)
        {
            Debug.LogError("GetTPFromProbInList(list) => list is empty");
            return null;
        }
        //list = AdjustThoughtsProbability(list);
        float rand = Random.Range(0, 100);
        float sum = 0;
        float debugsum = 0;
        for(int i = 0; i < list.Count * percentageOfTopFitness; ++i)
        {
            debugsum += list[i].probability;
        }
        for (int i = 0; i < list.Count; ++i)
        {
            if (sum + list[i].probability > rand)
            {
                return list[i];
            }
            sum += list[i].probability;
        }
        return list[Random.Range(0, list.Count)];
    }

    /// CleanUps ///
    public void CleanUpConnections(bool clearInCons = true, bool clearOutCons = true)
    {
        foreach (ANNInputNode n in inputNodes)
        {
            n.CleanUpConnections(clearInCons, clearOutCons);
        }
        foreach (ANNOutputNode n in outputNodes)
        {
            n.CleanUpConnections(clearInCons, clearOutCons);
        }
        foreach (ANNHiddenNode n in hiddenNodes)
        {
            n.CleanUpConnections(clearInCons, clearOutCons);
        }
    }

    /// SERIALIZATION ///
    public void Serialize()
    {
        if (Application.isPlaying) return;
        ANNSerialization.NETS.SerializeNodes(Parent());
        ANNSerialization.NETS.SerializeValues(Parent());
    }

    /// UTILS ///
    #region /// Utils ///
    public ANNNetwork Parent()
    {
        return instanceParent == null ? this : instanceParent;
    }
    public ANNNetwork Instanciate()
    {
        ANNNetwork auxNet = ScriptableObject.CreateInstance<ANNNetwork>();
        auxNet.Copy(this);
        instances.Add(auxNet);
        auxNet.instanceParent = this.instanceParent == null ? this : this.instanceParent;
        auxNet.IsInstance = true;
        return auxNet;
    }
    public void Copy(ANNNetwork net)
    {
        this.name = net.name + GetNextNumAppendixFromName(net.name);
        NHiddenLayers = net.NHiddenLayers;
        Generation = net.Generation;
        AgentsPerGeneration = net.AgentsPerGeneration;
        GeneticAlgorithmDelegate = net.GeneticAlgorithmDelegate;
        tickType = net.tickType;
        ManualNHiddenNodes = net.ManualNHiddenNodes;
        NHiddenNodesPerLayer = net.NHiddenNodesPerLayer;
    }
    string GetNextNumAppendixFromName(string name)
    {
        return " (instance)";
    }

    public List<ANNHiddenNode> GetHiddenNodesFromLayerOrder(int layerOrder)
    {
        List<ANNHiddenNode> nodes = new List<ANNHiddenNode>();
        foreach(ANNHiddenNode n in hiddenNodes)
        {
            if (n.layer == layerOrder)
                nodes.Add(n);
        }
        return nodes;
    }
    public ANNInputNode GetInputNodeFromName(string name)
    {
        for(int i = 0; i < inputNodes.Count; ++i)
        {
            if (inputNodes[i] == null) continue;
            if(inputNodes[i].name == name)
            {
                return inputNodes[i];
            }
        }
        //Debug.LogWarning(this.name + " => Trying to GetInputNodeFromName of a non-existing node -> '" + name + "'");
        return null;
    }
    public ANNOutputNode GetOutputNodeFromName(string name)
    {
        for (int i = 0; i < outputNodes.Count; ++i)
        {
            if (outputNodes[i] == null) continue;
            if (outputNodes[i].name == name)
            {
                return outputNodes[i];
            }
        }
        //Debug.LogWarning(this.name + " => Trying to GetOutputNodeFromName of a non-existing node -> '" + name + "'");
        return null;
    }
    public void ResetGeneration()
    {
        Generation = 0;
        string path = ANNSerialization.GLOBALS.TP.Path(Parent().name);
        if(Directory.Exists(path))
        {
            lastGenTPs.Clear();
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
    public void SetGeneration(int gen)
    {
        Generation = gen;
    }

    #endregion
}