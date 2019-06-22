using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ANNSerialization {

    /// GLOBALS ///
    #region // GLOBALS //
    public class GLOBALS
    {
        // GLOBALS //
        private static string _Path = Application.dataPath + "/ANNProject/Serialization";

        // Getters ---
        #region // Getters ---
        public static string Path
        {
            get
            {
                return _Path;
            }
        }
        #endregion
        
        // NET //
        public class NETS
        {
            private static string _FolderName = "Networks";
            private static string _NodeSeparator = "|";

            private static string _NetValuesFileName = "NetValues";

            // Getters ---
            public static string FolderName
            {
                get
                {
                    return _FolderName;
                }
            } 
            public static string Path
            {
                get
                {
                    return ANNSerialization.GLOBALS.Path + "/" + _FolderName;
                }
            }
            public static string NodeSeparator
            {
                get
                {
                    return _NodeSeparator;
                }
            }
            public static string NetValuesFileName
            {
                get
                {
                    return _NetValuesFileName;
                }
            }
            #region // Getters ---
            #endregion
        }

        // TP //
        #region // TP //

        public class TP
        {
            private static string _FolderName = "Generations";
            private static string _GenFolderName = "Gen_";
            private static string _FitnessMarker = "f";
            private static string _ThoughtMarker = "&";
            private static string _FitnessSeparator = "|";
            private static string _ThoughtSeparator = "|";
            // Getters ---
            #region // Getters ---
            public static string FolderName
            {
                get
                {
                    return _FolderName;
                }
            }
            public static string Path()
            {
                return ANNSerialization.GLOBALS.Path + "/" + FolderName;
            }
            public static string Path(string netname)
            {
                return ANNSerialization.GLOBALS.Path + "/" + FolderName + "/" + netname;
            }
            public static string GenFolderName
            {
                get
                {
                    return _GenFolderName;
                }
            }
            public static string TPMarker
            {
                get
                {
                    return _FitnessMarker;
                }
            }
            public static string ThoughtMarker
            {
                get
                {
                    return _ThoughtMarker;
                }
            }
            public static string TPSeparator
            {
                get
                {
                    return _FitnessSeparator;
                }
            }
            public static string ThoughtSeparator
            {
                get
                {
                    return _ThoughtSeparator;
                }
            }

            public static string FileName(string path)
            {
                int index = 0;
                for (index = 0; File.Exists(path + "/" + index); ++index) ;

                return index.ToString();
            }
            #endregion

        }

        /// Thought Process Serialization ///
        /*public struct TP
        {
            // FILE ---
            // file format
            private static string _fileFormat = ""; // no format
                                                    // file path
            private static string _filePath = "Assets/ANNProject/ThoughtProcesses";
            // file gen
            private static string _fileGen = "Gen-";

            // THOUGHT ---
            // fitness
            private static string _fitnessMarker = "f";
            // bias
            private static string _biasMarker = "b";
            // weights
            private static string _weightMarker = "w";
            private static string _weightSeparator = ",";

            // Getters ---
            #region // Getters ---
            // FILE
            #region // FILE
            public static string fileFormat
            {
                get
                {
                    return _fileFormat;
                }
            }
            public static string filePath
            {
                get
                {
                    return _filePath;
                }
            }
            public static string fileGen
            {
                get
                {
                    return _fileGen;
                }
            }
            #endregion
            // THOUGHT
            #region // TOUGHT
            public static string fitnessMarker
            {
                get
                {
                    return _fitnessMarker;
                }
            }
            public static string biasMarker
            {
                get
                {
                    return _biasMarker;
                }
            }
            public static string weightMarker
            {
                get
                {
                    return _weightMarker;
                }
            }
            public static string weightSeparator
            {
                get
                {
                    return _weightSeparator;
                }
            }
            #endregion
            #endregion
        }*/
        #endregion
    }
    #endregion

    /// SERIALIZATION ///

    // NETWORKS //
    public class NETS
    {
        // Serialization ---
        public static string ToStream(ANNNetwork net)
        {
            if (net == null) return "";
            DATA_STRUCTS.NetworkDataStruct data = new DATA_STRUCTS.NetworkDataStruct();
            data.Read(net);
            return JsonUtility.ToJson(data);
        }
        public static void OverrideNetValues(string stream)
        {
            // [TODO]

            
        }
        public static void SerializeValues(ANNNetwork net)
        {
            string path = GLOBALS.NETS.Path;
            string file = "";

            // Network
            file += NETS.ToStream(net);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += "/" + net.name;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(path + "/" + GLOBALS.NETS.NetValuesFileName, file);
        }
        public static void SerializeNodes(ANNNetwork net)
        {
            string path = GLOBALS.NETS.Path;
            string file = "";
            // nodes
            foreach (ANNNode n in net.InputNodes)
            {
                file += NODES.ToStream(n) + GLOBALS.NETS.NodeSeparator;
            }
            foreach (ANNNode n in net.OutputNodes)
            {
                file += NODES.ToStream(n) + GLOBALS.NETS.NodeSeparator;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += "/" + net.name;
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(path + "/" + net.name, file);
        }

        // Load ---
        public static DATA_STRUCTS.NetworkDataStruct LoadValues(string stream)
        {
            DATA_STRUCTS.NetworkDataStruct data = new DATA_STRUCTS.NetworkDataStruct();
            data = JsonUtility.FromJson<DATA_STRUCTS.NetworkDataStruct>(stream);
            return data;
        }

    }

    // NODES //
    public class NODES
    {
        // Serialization ---
        public static string ToStream(ANNNode node)
        {
            if (node == null) return "";
            DATA_STRUCTS.NodeDataStruct data = new DATA_STRUCTS.NodeDataStruct();
            data.Read(node);
            return JsonUtility.ToJson(data);
        }
        // Load ---
        public static ANNNode Load(string stream)
        {
            DATA_STRUCTS.NodeDataStruct data = new DATA_STRUCTS.NodeDataStruct();
            data = JsonUtility.FromJson<DATA_STRUCTS.NodeDataStruct>(stream);
            ANNNode node = null;

            switch((DATA_STRUCTS.NodeDataStruct.NodeType)data.type)
            {
                case DATA_STRUCTS.NodeDataStruct.NodeType.Input:
                    node = LoadInputNode(data);
                    break;
                case DATA_STRUCTS.NodeDataStruct.NodeType.Output:
                    node = LoadOutputNode(data);
                    break;
                case DATA_STRUCTS.NodeDataStruct.NodeType.Hidden:
                    break;
                default:
                    Debug.LogWarning("Trying to Load(stream) a node with unknown type.");
                    return null;
            }

            if (node == null)
                return null;

            data.Write(ref node);

            return node;
        }
        #region specifics
        private static ANNInputNode LoadInputNode(DATA_STRUCTS.NodeDataStruct data)
        {
            ANNInputNode parent = null;
            foreach (string asset in AssetDatabase.FindAssets("t:ANNInputNode"))
            {
                if (data.referenceuid == asset)
                {
                    parent = AssetDatabase.LoadAssetAtPath<ANNInputNode>(AssetDatabase.GUIDToAssetPath(asset));
                    break;
                }
            }
            if (parent == null) return null;
            return parent.InstanciateAsInput();
        }
        private static ANNOutputNode LoadOutputNode(DATA_STRUCTS.NodeDataStruct data)
        {
            ANNOutputNode parent = null;
            foreach (string asset in AssetDatabase.FindAssets("t:ANNOutputNode"))
            {
                if (data.referenceuid == asset)
                {
                    parent = AssetDatabase.LoadAssetAtPath<ANNOutputNode>(AssetDatabase.GUIDToAssetPath(asset));
                    break;
                }
            }
            return parent.InstanciateAsOutput();
        }
        #endregion
    }

    public class TP
    {
        public static void DeleteGeneration(string net_name, int gen)
        {
            string path = GLOBALS.TP.Path(net_name) + "/" + GLOBALS.TP.GenFolderName + gen;
            if(Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public static void Serialize(ANNThoughtProcess tp)
        {
            string path = GLOBALS.TP.Path(tp.net.name) + "/" + GLOBALS.TP.GenFolderName + tp.net.generation;
            string file = "";
            file += ANNSerialization.GLOBALS.TP.TPMarker + ANNSerialization.TP.ToStream(tp) + GLOBALS.TP.TPSeparator;
            // thoughts
            file += ANNSerialization.GLOBALS.TP.ThoughtMarker;
            foreach(ANNNodeThought nt in tp.thoughts)
            {
                file += THOUGHTS.ToStream(nt) + GLOBALS.TP.ThoughtSeparator;
            }


            if(!Directory.Exists(GLOBALS.TP.Path()))
            {
                Directory.CreateDirectory(GLOBALS.TP.Path());
            }
            if(!Directory.Exists(GLOBALS.TP.Path(tp.net.name)))
            {
                Directory.CreateDirectory(GLOBALS.TP.Path(tp.net.name));
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filename = GLOBALS.TP.FileName(path);
            //Debug.Log(path + " // " + filename);

            File.WriteAllText(path + "/" + filename, file);

        }

        public static string ToStream(ANNThoughtProcess tp)
        {
            DATA_STRUCTS.TPDataStruct data = new DATA_STRUCTS.TPDataStruct();
            data.Read(tp);
            return JsonUtility.ToJson(data);
        }

        public static DATA_STRUCTS.TPDataStruct LoadData(string stream)
        {
            DATA_STRUCTS.TPDataStruct data = new DATA_STRUCTS.TPDataStruct();
            data = JsonUtility.FromJson<DATA_STRUCTS.TPDataStruct>(stream);
            return data;
        }
        public static ANNThoughtProcess Load(string stream)
        {
            DATA_STRUCTS.TPDataStruct data = new DATA_STRUCTS.TPDataStruct();
            data = JsonUtility.FromJson<DATA_STRUCTS.TPDataStruct>(stream);
            ANNThoughtProcess tp = new ANNThoughtProcess();
            data.Write(ref tp);
            return tp;
        }
    }

    public class THOUGHTS
    {
        // Serialization ---
        public static string ToStream(ANNNodeThought thought)
        {
            DATA_STRUCTS.ThoughtDataStruct data = new DATA_STRUCTS.ThoughtDataStruct();
            data.Read(thought);
            return JsonUtility.ToJson(data);
        }
        // Load ---
        public static ANNNodeThought Load(string stream)
        {
            DATA_STRUCTS.ThoughtDataStruct data = new DATA_STRUCTS.ThoughtDataStruct();
            data = JsonUtility.FromJson<DATA_STRUCTS.ThoughtDataStruct>(stream);
            ANNNodeThought nt = new ANNNodeThought();
            data.Write(ref nt);
            return nt;
        }
    }

    /// STRUCTURES ///
    public class DATA_STRUCTS
    {
        // NETWORKS //
        public struct NetworkDataStruct
        {
            public string name;
            public int generation;
            // references
            public void Read(ANNNetwork net)
            {
                if (net == null)
                {
                    Debug.LogWarning("Trying to Read a 'null' net.");
                    return;
                }
                name = net.name;
                generation = net.generation;
            }
            public void Write(ref ANNNetwork net)
            {
                if(net == null)
                {
                    Debug.LogWarning("Trying to Write a 'null' net.");
                    return;
                }
                net.name = name;
                net.SetGeneration(generation);
            }
        }
        // NODES //
        public struct NodeDataStruct
        {
            public string name;
            public int type;
            public int activationtype;
            // references
            public string referenceuid;
            public long localid;

            public enum NodeType
            {
                Input,
                Output,
                Hidden
            }

            // Read & Write
            public void Read(ANNNode node)
            {
                if(node.GetType() == typeof(ANNInputNode))
                {
                    type = (int)NodeType.Input;
                }
                else if(node.GetType() == typeof(ANNOutputNode))
                {
                    type = (int)NodeType.Output;
                }
                else if(node.GetType() == typeof(ANNHiddenNode))
                {
                    type = (int)NodeType.Hidden;
                }
                else
                {
                    Debug.LogWarning("Trying to serialize a node that is not an Input, Output or Hidden Node => Node: '" + node.name + "'.");
                    return;
                }
                name = node.name;
                activationtype = (int)node.ActivationMethodType;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(node.Parent(), out referenceuid, out localid);
            }
            public void Write(ref ANNNode node)
            {
                if(node == null)
                {
                    Debug.LogWarning("Trying to Write(node) a 'null' node.");
                    return;
                }
                node.name = name;
                node.ActivationMethodType = (ANNActivationMethodsList)activationtype;
            }
        }

        // THOUGHT PROCESS //
        public struct TPDataStruct
        {
            public float fitness;
            public void Read(ANNThoughtProcess tp)
            {
                fitness = tp.fitness;
            }
            public void Write(ref ANNThoughtProcess tp)
            {
                tp.fitness = fitness;
            }
        }

        // THOUGHTS //
        public struct ThoughtDataStruct
        {
            public float bias;
            public List<float> weights;
            // Read & Write
            public void Read(ANNNodeThought nt)
            {
                bias = nt.bias;
                if(weights == null)
                {
                    weights = new List<float>();
                }
                for (int i = 0; i < nt.weights.Count; ++i)
                {
                    weights.Add(nt.weights[i]);
                }
            }

            public void Write(ref ANNNodeThought nt)
            {
                if (nt == null)
                {
                    Debug.LogWarning("Trying to Write(nodethought) a 'null' thought.");
                    return;
                }
                nt.bias = bias;
                for(int i = 0; i < this.weights.Count; ++i)
                {
                    nt.weights.Add(this.weights[i]);
                }
            }
        }
    }
}
