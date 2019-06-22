using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NeuralNetworkComponent))]
public class NeuralNetworkInspector : Editor {

    // Window Reference ---
    NeuralNetworkWindow window = null;

    // References ---
    NeuralNetworkComponent component = new NeuralNetworkComponent();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        component = (NeuralNetworkComponent)target;
        if (component.network == null)
            EditorGUILayout.TextField("Network:", "(none) null");
        else
            EditorGUILayout.TextField("Network:", component.network.Name);
        if(component.network != null)
        {
            if(GUILayout.Button("Network Tab"))
            {
                window = NeuralNetworkWindow.GetWindow<NeuralNetworkWindow>(component.network.Name);
                window.network = component.network;
            }
        }
        if(GUILayout.Button("Choose Network"))
        {
            ChooseNetworkWindow chooseWin = ChooseNetworkWindow.GetWindow<ChooseNetworkWindow>("Choose Network");
            chooseWin.inspector = this;
        }
    }

    public void SetNetwork(NeuralNetwork n)
    {
        if(n != null)
            component.network = n;
    }
}

// ChooseNetworkWindow ---
#region // ChooseNetwork Window ---
class ChooseNetworkWindow : EditorWindow
{
    static EditorWindow window = null;

    private NeuralNetworkInspector Inspector = null;
    public NeuralNetworkInspector inspector
    {
        get
        {
            return Inspector;
        }
        set
        {
            Inspector = value;
        }
    } // Getter & Setter

    bool showCreateNetWin = false;

    // Create Window Vars ---
    int nInputs = 1;
    int nOutputs = 1;
    int nHLayers = 1;
    int nHl = 0;
    bool manualHl = false;
    string netName = "Network";

    // ScrollView Variables ---
    Vector2 scrollPos = Vector2.zero;

    // Selected Net Reference ---
    NeuralNetwork selectedNet = null;

    public NeuralNetwork GetSelectedNet()
    { return selectedNet; }

    [MenuItem("Assets/ANNProject Network")]
    public static void ShowWindow()
    {
        window = GetWindow<ANNNetworkCreator>("Networks");
    }
    void OnGUI()
    {
        if(inspector == null) { Close(); return; }
        if (ANNProperties.networks.Count > 0 &&
            GUILayout.Button("Choose This Net\n[" + (selectedNet == null ? "(none) null" : selectedNet.Name) + "]",
            GUILayout.Height(35)))
        {
            if (selectedNet != null)
            {
                inspector.SetNetwork(selectedNet);
                Close();
                return;
            }
        }
        GUILayout.Label("Networks: ", EditorStyles.boldLabel);
        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos, false, true);
        if (ANNProperties.networks.Count <= 0)
        {
            GUILayout.Label("No existing Networks.", EditorStyles.miniBoldLabel);
        }
        else
        {
            //EditorGUILayout.TextField("Selected:", (selectedNet == null ? "none (null)" : selectedNet.name), EditorStyles.
            foreach (NeuralNetwork net in ANNProperties.networks)
            {
                if (net == null) continue;
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.black;
                if (selectedNet == net) labelStyle.normal.textColor = Color.blue;
                GUILayout.Label("\t" + net.Name, labelStyle);
                if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    selectedNet = net;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Network"))
        {
            showCreateNetWin = true;
        }
        GUILayout.EndHorizontal();
        if (showCreateNetWin)
        {
            BeginWindows();
            GUI.Window(0, new Rect(100, 50, 300, 250), CreateNetWindow, "Create Network");
            EndWindows();
        }
    }
    NeuralNetwork GetNetwork(string netName)
    {
        foreach (NeuralNetwork net in ANNProperties.networks)
        {
            if (netName == net.Name) return net;
        }
        return null;
    }
    void CreateNetWindow(int windowID)
    {
        netName =
            EditorGUILayout.TextField("Network Name:", netName);
        int repeatedName = netNameExists(netName);
        if (repeatedName > 0)
        {
            netName = netName + "(" + repeatedName + ")";
        }
        nInputs =
            EditorGUILayout.IntField("Input Nodes:", nInputs);
        nOutputs =
            EditorGUILayout.IntField("Output Nodes:", nOutputs);
        nHLayers =
            EditorGUILayout.IntField("Hidden Layers: ", nHLayers);
        if (manualHl)
        {
            nHl =
                EditorGUILayout.IntField("Hidden Nodes:", nHl);
        }
        else
        {
            nHl = (int)Mathf.Pow(nInputs + nOutputs, 0.5f) + 1;
            EditorGUILayout.IntField("Hidden Nodes", nHl, EditorStyles.boldLabel);
        }
        manualHl = EditorGUILayout.Toggle("Manual Number of Hidden Nodes", manualHl);
        if (GUILayout.Button("Create"))
        {
            NeuralNetwork newnet = new NeuralNetwork();
            newnet.CreateNetwork(nInputs, nOutputs, nHl, netName);
            ANNProperties.networks.Add(newnet);
            ResetNetPrefs();
            showCreateNetWin = false;
        }
    }
    void ResetNetPrefs()
    {
        nInputs = 1;
        nOutputs = 1;
        nHl = 0;
        manualHl = false;
        netName = "Network";
    }
    int netNameExists(string n)
    {
        int count = 0;
        foreach (NeuralNetwork net in ANNProperties.networks)
        {
            if (net.Name == n) count++;
        }
        return count;
    }
}
#endregion

public class NeuralNetworkWindow : EditorWindow
{
    // References ---
    // Network
    public NeuralNetwork network = null;
    // Nodes
    List<GraphicNode> gNodes = new List<GraphicNode>();
    List<GraphicConnection> gCons = new List<GraphicConnection>();

    // Properties ---
    static private Vector2 MinSize = new Vector2(375, 250);
    static private Vector2 MaxSize = new Vector2(750, 1213.5f);

    // Graphic Variables ---
    // Position & Scale
    private Rect    layerTransforms = new Rect(0, 0, 200f, 100f);
    private Vector2 graphicOrigin = new Vector2(250, 250);
    private Vector2 graphicOffset = new Vector2(0, 0);
    // Connection Graphics
    private float tangentPoint = 0f;
    // Node Variables
    private Vector2 nodeScale = new Vector2(100, 100);

    // Styles ---
    GUIStyle inStyle = null;
    GUIStyle outStyle = null;
    GUIStyle hlStyle = null;

    // Network Info Window Variables ---
    private Rect netInfoRect = new Rect(20, 20, 250, 250);

    // Init ---
    #region // Init ---
    void Init()
    {
        // Scales -
        minSize = MinSize;
        maxSize = MaxSize;
        // Styles -
        InitStyles();
    }
    void InitStyles()
    {
        // inStyle
        inStyle = new GUIStyle();
        inStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        inStyle.border = new RectOffset(12, 12, 12, 12);
        // outStyle
        outStyle = new GUIStyle();
        outStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        outStyle.border = new RectOffset(12, 12, 12, 12);
        // hlStyle
        hlStyle = new GUIStyle();
        hlStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        hlStyle.border = new RectOffset(12, 12, 12, 12);
    }
    #endregion

    // Enable ---
    #region // Enable ---
    void OnEnable()
    {
        Init();
        Focus();
        UpdateGraphics();
        //OnGUI();
    }
    #endregion

    // Updates ---
    public void UpdateGraphics()
    {
        if (network == null) return;
        gNodes.Clear();
        gCons.Clear();
        List<NeuralNode> closedNodes = new List<NeuralNode>();
        List<NeuralConnection> closedCons = new List<NeuralConnection>();
        foreach(NeuralNode n in network.neuralNodes)
        {
            GraphicNode gnodeA = null;
            if (IsInClosedNodes(n, closedNodes) == false)
            {
                closedNodes.Add(n);
                gnodeA = CreateGNode(n);
            }
            foreach(NeuralConnection c in n.outCons)
            {
                GraphicNode gnodeB = null;
                if (c.outNode == null) continue;
                if (IsInClosedNodes(c.outNode, closedNodes) == false)
                {
                    closedNodes.Add(c.outNode);
                    gnodeB = CreateGNode(c.outNode);
                }
                if(IsInClosedCons(c, closedCons) == false)
                {
                    closedCons.Add(c);
                    if (gnodeA == null) gnodeA = GetGNodeByNeuralNode(n);
                    if (gnodeB == null) gnodeB = GetGNodeByNeuralNode(c.outNode);
                    CreateGCon(c, gnodeB, gnodeA);
                }
            }
        }
            
    }
    #region // Update Utility --
    private bool IsInClosedNodes(NeuralNode n, List<NeuralNode> list)
    {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] == n) return true;
        return false;
    }
    private bool IsInClosedCons(NeuralConnection c, List<NeuralConnection> list)
    {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] == c) return true;
        return false;
    }
    private GraphicNode GetGNodeByNeuralNode(NeuralNode n)
    {
        foreach (GraphicNode gn in gNodes)
        {
            if (gn.node == n)
            {
                return gn;
            }
        }
        return null;
    }
    #endregion

    // Node Creation ---
    private GraphicNode CreateGNode(NeuralNode n)
    {
        if (n == null) return null;
        GUIStyle style = new GUIStyle();
        if (n.nodeOrder == NeuralNetwork.inputNodeOrder)
            style = inStyle;
        else if (n.nodeOrder == NeuralNetwork.outputNodeOrder)
            style = outStyle;
        else
            style = hlStyle;
        GraphicNode gnode = new GraphicNode(n, new Vector2(0, 0), nodeScale.x, nodeScale.y, style);
        gNodes.Add(gnode);
        return gnode;
    }
    // Connection Creation ---
    private GraphicConnection CreateGCon(NeuralConnection c, GraphicNode ingn, GraphicNode outgn)
    {
        if (c == null || ingn == null || outgn == null) return null;
        GraphicConnection gc = new GraphicConnection(c, ingn, outgn);
        gCons.Add(gc);
        return gc;
    }

    // Draw ---
    public void DrawGraphics()
    {
        Dictionary<int, int> nodesByOrder = new Dictionary<int, int>(); // <order, nnodes>
        foreach(GraphicNode gn in gNodes)
        {
            if (nodesByOrder.ContainsKey(gn.node.nodeOrder) == false)
                nodesByOrder.Add(gn.node.nodeOrder, 0);
            nodesByOrder[gn.node.nodeOrder]++;
            gn.rect.x = gn.selfOffset.x + graphicOrigin.x + graphicOffset.x + layerTransforms.width * gn.node.nodeOrder;
            gn.rect.y = gn.selfOffset.y + graphicOrigin.y + graphicOffset.y + layerTransforms.height * (nodesByOrder[gn.node.nodeOrder] - 1);
            gn.Draw();
        }
        foreach(GraphicConnection gc in gCons)
        {
            gc.Draw();
        }
    }

    // OnGUI ---
    void OnGUI()
    {
        #region // Security ---
        if (network == null)
        {
            GUILayout.Label("Reference to Network cannot be found.", EditorStyles.miniBoldLabel);
            return;
        }
        #endregion

        // Info ---
        PrintNetworkInfo();
        // Adding ---
        if (GUILayout.Button("Add Node", GUILayout.Height(50f), GUILayout.Width(75f)))
        { OnClickAddNode(); }

        if(GUILayout.Button("StoreThoughts"))
        {
            network.StoreThoughts();
        }

        // Drawing ---
        DrawGraphics();

        // Events ---
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();

    }

    // Print Network Info ---
    void PrintNetworkInfo()
    {
        EditorGUILayout.IntField("Nº of Nodes:", network.neuralNodes.Count, EditorStyles.boldLabel);
        EditorGUILayout.IntField("Input Nodes:", network.inputNodes.Count, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Output Nodes:", network.outputNodes.Count, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Hidden Nodes:", network.neuralNodes.Count - network.inputNodes.Count - network.outputNodes.Count, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Connections", gCons.Count, EditorStyles.boldLabel);
    }

    // Event Handling ---
    #region // Event Handling ---
    private void ProcessNodeEvents(Event e)
    {
        if (gNodes == null) return;
        foreach(GraphicNode gn in gNodes)
        {
            bool guiChanged = gn.ProcessEvents(e);
            if (guiChanged) GUI.changed = true;
        }
    }
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                    foreach(GraphicNode g in gNodes)
                    {
                        Debug.Log(g.node.id);
                    }
                }
                break;
            case EventType.MouseUp:
                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                    e.Use();
                    return;
                }
                break;
        }
    }
    private void OnDrag(Vector2 delta)
    {
        graphicOffset += delta;
        GUI.changed = true;
    }
    private void ProcessContextMenu(Vector2 mousePos)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Node"), false, () => OnClickAddNode());
        genericMenu.AddItem(new GUIContent("Delete Node"), false, () => OnClickDeleteNode()); // Need to know which node was clicked by mousepos
        genericMenu.ShowAsContext();
    }
    private void OnClickDeleteNode()
    {

    }
    private void OnClickAddNode()
    {
        if (network == null) return;
        AddNodeWindow nodeWindow = AddNodeWindow.GetWindow<AddNodeWindow>("Create Node");
        nodeWindow.network = network;
        nodeWindow.netWin = this;
    }
    #endregion
}

// GraphicNode ---
#region // Graphic Node ---
public class GraphicNode
{
    public Rect rect = new Rect();
    public string title = "";

    public GUIStyle style = new GUIStyle();

    public NeuralNode node = null;

    public bool isSelected = false;
    public Vector2 selfOffset = Vector2.zero;

    // Constructor ---
    public GraphicNode() { }
    public GraphicNode(NeuralNode n, Vector2 p, float w, float h, GUIStyle s)
    { rect = new Rect(p.x, p.y, w, h); style = s; node = n; }

    // Draw ---
    public void Draw()
    {
        GUI.Box(rect, title, style);
    }

    // Events ---
    public bool ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if(rect.Contains(e.mousePosition))
                    {
                        isSelected = true;
                    }
                    GUI.changed = true;
                }
                break;
            case EventType.MouseUp:
                isSelected = false;
                break;
            case EventType.MouseDrag:
                if(e.button == 0 && isSelected)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }
    public void Drag(Vector2 delta)
    {
        selfOffset.x += delta.x;
        selfOffset.y += delta.y;
    }

}
#endregion

// GraphicConnection ---
#region // GraphicConnection ---
public class GraphicConnection
{
    // Variables ---
    public NeuralConnection connection = null;
    public GraphicNode  inNode = null;
    public GraphicNode  outNode = null;

    // Graphics ---
    public Color        color = ANNProperties.ConnectionsColor;

    // Constructor ---
    public GraphicConnection() { }
    public GraphicConnection(NeuralConnection c, GraphicNode inp, GraphicNode outp)
    { connection = c; inNode = inp;  outNode = outp; }

    // Draw ---
    public void Draw()
    {
        if (inNode == null || outNode == null) return;
        Vector3 oCenter = new Vector3(outNode.rect.x + outNode.rect.width, outNode.rect.y + outNode.rect.height / 2, 0);
        Vector3 iCenter = new Vector3(inNode.rect.x, inNode.rect.y + inNode.rect.height / 2, 0);
        Handles.DrawBezier(
            oCenter,
            iCenter,
            iCenter,
            oCenter,
            color,
            null,
            (ANNMathHelpers.Sigmoid(connection.weight) - 0.5f) * ANNProperties.GraphicWeightMultiplier
            );
    }
}
#endregion

// Add Node Window ---
#region // Add Node Window ---
class AddNodeWindow : EditorWindow
{
    // References ---
    public NeuralNetwork network = null;
    public NeuralNetworkWindow netWin = null;

    // Window Properties ---
    static private Vector2 MinSize = new Vector2(100, 100);
    static private Vector2 MaxSize = new Vector2(350, 450);

    // Node Variables ---
    private enum NodeType
    {
        Input = 0,
        Hidden,
        Output
    }

    private int order = NeuralNetwork.stdHiddenNodeOrder;
    private float BiasValue = 0f;
    private NodeType type = NodeType.Input;

    void OnInit()
    {
        minSize = MinSize;
        maxSize = MaxSize;
        Focus();
    }

    void OnEnable()
    {
        order = NeuralNetwork.stdHiddenNodeOrder;
    }

    void OnGUI()
    {
        type = (NodeType)EditorGUILayout.EnumPopup("Node Type:", type);
        switch (type)
        {
            case NodeType.Input:
                order = NeuralNetwork.inputNodeOrder;
                break;
            case NodeType.Hidden:
                int currorder = order;
                GUILayout.BeginHorizontal();
                order = EditorGUILayout.IntField("Layer Order", order);
                if (NeuralNetwork.isInNodeOrderRange(order, false) == false)
                {
                    order = NeuralNetwork.stdHiddenNodeOrder;
                }
                GUILayout.Label("(min.:" + (NeuralNetwork.inputNodeOrder + 1) + "max.:" + (NeuralNetwork.outputNodeOrder - 1) + ")", EditorStyles.miniLabel);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                BiasValue = EditorGUILayout.FloatField("Bias Value:", BiasValue);
                if (GUILayout.Button("Rand"))
                {
                    BiasValue = Random.Range(ANNProperties.BiasRandomRange.x, ANNProperties.BiasRandomRange.y);
                }
                GUILayout.EndHorizontal();
                break;
            case NodeType.Output:
                order = NeuralNetwork.outputNodeOrder;
                GUILayout.BeginHorizontal();
                BiasValue = EditorGUILayout.FloatField("Bias Value:", BiasValue);
                if (GUILayout.Button("Rand"))
                {
                    BiasValue = Random.Range(ANNProperties.BiasRandomRange.x, ANNProperties.BiasRandomRange.y);
                }
                GUILayout.EndHorizontal();
                break;
        }

        // Accept / Cancel ---
        #region // Accept / Cancel ---
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            netWin.Focus();
            this.Close();
        }
        if (GUILayout.Button("Create"))
        {
            GenerateNode();
            netWin.Focus();
            this.Close();
        }
        GUILayout.EndHorizontal();
        #endregion
    }

    private void GenerateNode()
    {
        if (network == null) return;
        NeuralNode node = new NeuralNode(network, order, BiasValue);
        network.AddNode(node);
        netWin.UpdateGraphics();
    }

}
#endregion
