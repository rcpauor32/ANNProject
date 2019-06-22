using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class ANNNetworkTab : EditorWindow {

    // Called on Opening Asset ---
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj.GetType() == typeof(ANNNetwork))
        {
            ANNNetworkTab window = ANNNetworkTab.GetWindow<ANNNetworkTab>("ANNetwork Tab");
            window.network = (ANNNetwork)obj;
            window.UpdateGraphics();
        }
        return false;
    }

    /// References ///
    public ANNNetwork network = null;
    public static bool networkChanged = false;

    /// Window Properties ///
    static public Vector2 MinSize = new Vector2(375, 250);
    static public Vector2 MaxSize = new Vector2(1080, 1213.5f);

    /// Graphic Variables ///
    // Graphic Nodes ---
    List<ANNGraphicNode> gNodes = new List<ANNGraphicNode>();
    List<ANNGraphicConnection> gCons = new List<ANNGraphicConnection>();
    // Position & Scale ---
    public Rect    layerTransforms  = new Rect(0f, 0f, 200f, 100f);
    public Vector2 graphicOrigin = new Vector2(250f, 250f);
    public Vector2 graphicOffset = new Vector2(0f, 0f);
    // Graphic Node Variables ---
    public Vector2 nodeScale = new Vector2(100, 100);
    // Connection Graphics ---
    public float tangentPoint = 0f;

    // Styles ---
    GUIStyle inStyle  = null;
    GUIStyle outStyle = null;
    GUIStyle hdnStyle = null;

    // Info Rect Variables ---
    public Rect infoRect = new Rect(20, 20, 250, 250);

    /// Init ///
    void Init()
    {
        // Scales
        minSize = MinSize;
        maxSize = MaxSize;
        // Styles
        InitStyles();
    }
    // Init Styles ---
    void InitStyles()
    {
        //nStyle
        inStyle = new GUIStyle();
        inStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        inStyle.border = new RectOffset(12, 12, 12, 12);
        // outStyle
        outStyle = new GUIStyle();
        outStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
        outStyle.border = new RectOffset(12, 12, 12, 12);
        // hlStyle
        hdnStyle = new GUIStyle();
        hdnStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
        hdnStyle.border = new RectOffset(12, 12, 12, 12);
    }

    /// On Enable ///
    void OnEnable()
    {
        Init();
        Focus();
        UpdateGraphics();
    }

    /// Updates ///
    public void UpdateGraphics()
    {
        if(network == null)
        {
            Debug.LogWarning(this.name + " => Trying to UpdateGraphics() with 'null' network reference");
            return;
        }
        gNodes.Clear();
        gCons.Clear();
        List<ANNNode> closedNodes = new List<ANNNode>();
        List<ANNConnection> closedCons = new List<ANNConnection>();
        foreach(ANNNode n in network.nodeList)
        {
            ANNGraphicNode gnodeA = null;
            if(IsInClosedNodes(n, closedNodes) == false)
            {
                closedNodes.Add(n);
                gnodeA = CreateGNode(n);
            }
            foreach(ANNConnection c in n.outputConnections)
            {
                ANNGraphicNode gnodeB = null;
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

    // Update Graphics Utils ---
    private bool IsInClosedNodes(ANNNode n, List<ANNNode> list)
    {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] == n) return true;
        return false;
    }
    private bool IsInClosedCons(ANNConnection c, List<ANNConnection> list)
    {
        for (int i = 0; i < list.Count; ++i)
            if (list[i] == c) return true;
        return false;
    }
    private ANNGraphicNode GetGNodeByNeuralNode(ANNNode n)
    {
        foreach (ANNGraphicNode gn in gNodes)
        {
            if (gn.node == n)
            {
                return gn;
            }
        }
        return null;
    }

    /// Creation ///
    // Nodes ---
    ANNGraphicNode CreateGNode(ANNNode n)
    {
        if (n == null) return null;
        GUIStyle style = new GUIStyle();
        if (n.GetLayerOrder() == (int)ANNLayerOrders.Input)
            style = inStyle;
        else if (n.GetLayerOrder() == (int)ANNLayerOrders.Output)
            style = outStyle;
        else
            style = hdnStyle;
        ANNGraphicNode gnode = new ANNGraphicNode(n, new Vector2(0, 0), nodeScale.x, nodeScale.y, style);
        gNodes.Add(gnode);
        return gnode;
    }
    // Connections ---
    private ANNGraphicConnection CreateGCon(ANNConnection c, ANNGraphicNode ingn, ANNGraphicNode outgn)
    {
        if (c == null || ingn == null || outgn == null) return null;
        ANNGraphicConnection gc = new ANNGraphicConnection(c, ingn, outgn);
        gCons.Add(gc);
        return gc;
    }
/// OnGUI ///
    void OnGUI()
    {
        if (network == null)
        {
            Debug.LogWarning(this.name + " => Trying to OnGUI() with 'null' network reference");
            return;
        }

        // Draw
        DrawGraphics();

        // Events
        ProcessNodesEvents(Event.current);
        ProcessEvents(Event.current);

        // Gui Changed ??
        if (GUI.changed) Repaint();
        if(networkChanged == true) { UpdateGraphics(); networkChanged = false; }

    }

    // Info Rect ---
    void DrawInfoRect()
    {
        Rect rectInfoRect = new Rect(
            0f,
            0f,
            250f,
            210f);
        GUI.Box(rectInfoRect, "");

        GUILayout.Label(network.name, EditorStyles.largeLabel);
        EditorGUILayout.IntField("Generation:", network.generation, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        int nInNodes = network.InputNodes.Count;
        int nOutNodes = network.OutputNodes.Count;
        int nHdnNodes = network.HiddenNodes.Count;
        EditorGUILayout.IntField("Nº of Nodes:", nInNodes + nOutNodes + nHdnNodes, EditorStyles.boldLabel);
        EditorGUILayout.IntField("Input Nodes:", nInNodes, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Output Nodes:", nOutNodes, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Hidden Nodes:", nHdnNodes, GUILayout.ExpandWidth(false));
        EditorGUILayout.IntField("Connections", gCons.Count, EditorStyles.boldLabel);
    }
    // Draw ---
    void DrawGraphics()
    {
        DrawNetwork();
        DrawInfoRect();
        DrawButtons();
    }

    void DrawButtons()
    {
        if(GUILayout.Button("Tick Net", GUILayout.Height(25f), GUILayout.Width(75f)))
        { OnClickTickNet(); }
        if (GUILayout.Button("Reset Net", GUILayout.Height(25f), GUILayout.Width(75f)))
        { OnClickResetNet(); }
        if (GUILayout.Button("Reload Net", GUILayout.Height(25f), GUILayout.Width(75f)))
        { OnClickReloadNet(); }
    }

    void DrawNetwork()
    {
        int inputLayer = -1;
        int outputLayer = inputLayer + 1;
        for (int i = 0; i < gNodes.Count; ++i)
        {
            if (gNodes[i].node.GetLayerOrder() > outputLayer)
                outputLayer = gNodes[i].node.GetLayerOrder();
        }
        outputLayer++;
        // Connections ---
        foreach (ANNGraphicConnection gc in gCons)
        {
            gc.Draw();
        }
        // Nodes ---
        Dictionary<int, int> nodesByOrder = new Dictionary<int, int>(); // <order, nnodes>
        foreach (ANNGraphicNode gn in gNodes)
        {
            if (nodesByOrder.ContainsKey(gn.node.GetLayerOrder()) == false)
                nodesByOrder.Add(gn.node.GetLayerOrder(), 0);
            nodesByOrder[gn.node.GetLayerOrder()]++;
            int layerorder = gn.node.GetLayerOrder();
            if(layerorder == (int)ANNLayerOrders.Input)
            {
                layerorder = inputLayer;
            }
            else if(layerorder == (int)ANNLayerOrders.Output)
            {
                layerorder = outputLayer;
            }
            gn.rect.x = gn.selfOffset.x + graphicOrigin.x + graphicOffset.x + layerTransforms.width * layerorder;
            gn.rect.y = gn.selfOffset.y + graphicOrigin.y + graphicOffset.y + layerTransforms.height * (nodesByOrder[gn.node.GetLayerOrder()] - 1);
            gn.Draw();
        }
        // Layer Grid ---
        GUILayout.BeginHorizontal();
        for(int i = -1; i <= network.NHiddenLayers; ++i)
        {
            float labelYOffset = 30f;
            Rect labelRect = new Rect(
                graphicOrigin.x + 25f + graphicOffset.x + layerTransforms.width * i , // X
                labelYOffset,                                          // Y
                50f,                                                            // W
                50f);
            //BoxStyle.border = new RectOffset(1, 1, 1, 1);
            Vector2 BoxExtraSize = new Vector2(12f, 15f);
            Rect labelBoxRect = new Rect(
            labelRect.x - BoxExtraSize.x / 2,
            labelRect.y + labelYOffset - BoxExtraSize.y / 2 - 5f,
            labelRect.width + BoxExtraSize.x,
            BoxExtraSize.y);
            GUI.Box(labelBoxRect, "", EditorStyles.miniButton);
            string labelTitle = "Layer-"+i;
            if (i == -1) labelTitle = "Input";
            else if (i == network.NHiddenLayers) labelTitle = "Output";
            EditorGUI.LabelField(labelRect, labelTitle, EditorStyles.centeredGreyMiniLabel);  
        }
        GUILayout.EndHorizontal();
    }

    /// Buttons ///
    void OnClickTickNet()
    {
        if(network == null)
        {
            return;
        }
        Selection.activeObject = null;
        network.Tick();
    }
    void OnClickResetNet()
    {
        if(network == null)
        {
            return;
        }
        Selection.activeObject = null;
        network.ResetNet();
        //network.ResetGeneration();
    }
    void OnClickReloadNet()
    {
        if(network == null)
        {
            return;
        }
        Selection.activeObject = null;
        network.LoadNetwork(true, true);

    }

    /// Events ///
    bool ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if(e.button == 0)
                {
                    Selection.activeObject = network;
                    //AssetDatabase.OpenAsset(network);
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }
    bool ProcessNodesEvents(Event e)
    {
        foreach(ANNGraphicNode gn in gNodes)
        {
            gn.ProcessEvents(e);
        }
        return false;
    }

    void OnDrag(Vector2 delta)
    {
        graphicOffset += delta;
        GUI.changed = true;
    }

}

/// GRAPHIC NODES ///
public class ANNGraphicNode
{
    public Rect rect = new Rect();
    public string title = "";

    public GUIStyle style = new GUIStyle();

    public ANNNode node = null;

    public Vector2 selfOffset = Vector2.zero;

    /// Constructor ///
    public ANNGraphicNode(ANNNode n, Vector2 pos, float w, float h, GUIStyle s)
    { rect = new Rect(pos.x, pos.y, w, h); style = s; node = n;
        if(node.GetType() == typeof(ANNInputNode))
        {
            title = node.name + " : " + ((ANNInputNode)node).GetInputValue();
        }
        else if(node.GetType() == typeof(ANNHiddenNode))
        {
            title = node.name;
        }
        else if(node.GetType() == typeof(ANNOutputNode))
        {
            title = node.name + " : " + ((ANNOutputNode)node).output;
        }
    }
    public ANNGraphicNode()
    {}

    /// Draw ///
    public void Draw()
    {
        if(node.GetType() == typeof(ANNOutputNode))
        {
            title = node.name + " : " + ((ANNOutputNode)node).output;
        }
        GUI.Box(rect, title, style);
    }

    /// Process Events ///
    public bool ProcessEvents(Event e)
    {
        switch(e.type)
        {
            case EventType.MouseDown:
                if (rect.Contains(e.mousePosition))
                {
                    if (e.button == 0) // Mouse::LeftButton
                    {
                        Selection.activeObject = node;
                        //AssetDatabase.OpenAsset(node);
                        e.Use();
                    }
                }
                break;
        }
        return false;
    }

}

/// GRAPHIC CONNECTION ///
public class ANNGraphicConnection
{
    // Variables --- 
    public ANNConnection con = null;
    public ANNGraphicNode inGNode = null;
    public ANNGraphicNode outGNode = null;

    // Graphics ---
    public Color color = ANNProperties.ConnectionsColor;

    // Constructor ---
    public ANNGraphicConnection()
    {}
    public ANNGraphicConnection(ANNConnection c, ANNGraphicNode inp, ANNGraphicNode outp)
    { con = c;  inGNode = inp; outGNode = outp; }

    // Draw ---
    public void Draw()
    {
        if(inGNode == null || outGNode == null)
        {
            return;
        }
        Vector3 oCenter = new Vector3(outGNode.rect.x + outGNode.rect.width, outGNode.rect.y + outGNode.rect.height / 2, 0);
        Vector3 iCenter = new Vector3(inGNode.rect.x, inGNode.rect.y + inGNode.rect.height / 2, 0);
        Handles.DrawBezier(
            oCenter,
            iCenter,
            iCenter,
            oCenter,
            color,
            null,
            (ANNMathHelpers.Sigmoid(con.weight) - 0.5f) * ANNProperties.GraphicWeightMultiplier
            );
    }
}