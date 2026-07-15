using CompareCrsdAndVets.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Common.Xml;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// トレース情報格納用クラス
    /// </summary>
    internal class Trace
    {
        /// <summary>メインファイルパス</summary>
        public string FilePath_Main { get; set; } = string.Empty;

        /// <summary>TraceVectors.xmlのファイルパス</summary>
        public string FilePath_TraceVectors { get; set; } = string.Empty;

        /// <summary>MassData.binのファイルパス</summary>
        public string FilePath_MassData { get; set; } = string.Empty;

        /// <summary>名前</summary>
        public string Name { get; set; }

        public double MaxTime
        {
            get
            {
                if (Vectors.Count == 0) return 0;
                TraceVectorData vector = Vectors.Find(a => a.Name == "Time");
                if (vector == null) return 0;
                if(vector.TypedData == null || vector.TypedData.Count == 0) return 0;
                return vector.TypedData.Max();
            }
        }

        /// <summary>定速区間かどうか</summary>
        public bool IsConstantSpeedSegment { get; set; }

        /// <summary>ベクトルデータ</summary>
        public List<TraceVectorData> Vectors { get; private set; } = new List<TraceVectorData>();

        /// <summary>行数</summary>
        public int RowCount { get; set; }

        /// <summary>カラム定義</summary>
        public List<TraceColumnData> Columns { get; private set; } = new List<TraceColumnData>();

        /// <summary>
        /// XMLファイルの読み込み
        /// </summary>
        /// <param name="pFilePath">XMLファイルパス</param>
        public void LoadMainXml()
        {
            XDocument doc = XDocument.Load(FilePath_Main);

            XElement resource = doc.Root;
            if (resource == null)
            {
                throw new InvalidDataException("XMLのルート要素(Resource)が見つかりません。");
            }

            XElement VisionStructureNode = FindElement(resource, "VisionStructure");

            Name = GetElementValue(VisionStructureNode, "Administration", "Name");

            FilePath_TraceVectors = GetElementValue(VisionStructureNode, "Child", "RecordRef");
            FilePath_TraceVectors = Path.Combine(GetParentPath(FilePath_Main), FilePath_TraceVectors);

            XElement privateNode = FindElement(resource, "Private");
            if (privateNode == null)
            {
                throw new InvalidDataException("XMLのルート要素(Resource)が見つかりません。");
            }

            IsConstantSpeedSegment = ToBool(GetElementValue(privateNode, "IsConstantSpeedSegment"));

            Vectors.Clear();

            XElement traceVectorsNode = FindElement(privateNode, "TraceVectors");
            if (traceVectorsNode == null)
            {
                return;
            }

            XElement arrayNode = traceVectorsNode.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "ArrayOfVectorData");
            if (arrayNode == null)
            {
                return;
            }

            foreach (XElement vectorNode in arrayNode.Elements().Where(x => x.Name.LocalName == "VectorData"))
            {
                TraceVectorData vectorData = new TraceVectorData
                {
                    Independant = ToBool(GetElementValue(vectorNode, "Independant")),
                    Name = GetElementValue(vectorNode, "Name"),
                    Unit = GetElementValue(vectorNode, "Unit")
                };

                XElement typedDataNode = FindElement(vectorNode, "TypedData");
                if (typedDataNode != null)
                {
                    foreach (XElement dataNode in typedDataNode.Elements())
                    {
                        if (string.IsNullOrWhiteSpace(dataNode.Value))
                        {
                            continue;
                        }

                        vectorData.TypedData.Add(ToDouble(dataNode.Value));
                    }
                }

                Vectors.Add(vectorData);
            }
        }

        /// <summary>
        /// TraceVectors.xmlファイルの読み込み
        /// </summary>
        /// <param name="pFilePath">XMLファイルパス</param>
        public void LoadTraceVectorsXml()
        {
            XDocument doc = XDocument.Load(FilePath_TraceVectors);

            XElement dataMatrix = doc.Root;
            if (dataMatrix == null)
            {
                throw new InvalidDataException("XMLのルート要素(DataMatrix)が見つかりません。");
            }

            //Name = GetElementValue(dataMatrix, "VisionStructure", "Administration", "Name");

            FilePath_MassData = GetElementValue(dataMatrix, "VisionStructure", "AssociatedRecord", "RecordRef");
            FilePath_MassData = Path.Combine(GetParentPath(FilePath_TraceVectors), FilePath_MassData);

            XElement privateNode = FindElement(dataMatrix, "Private");
            if (privateNode == null)
            {
                throw new InvalidDataException("XMLのPrivate要素が見つかりません。");
            }

            RowCount = ToInt(GetElementValue(privateNode, "RowCount"));

            Columns.Clear();
            XElement columnsNode = FindElement(privateNode, "Columns");
            if (columnsNode == null)
            {
                return;
            }

            foreach (XElement columnNode in columnsNode.Elements().Where(x => x.Name.LocalName == "Column"))
            {
                Columns.Add(new TraceColumnData
                {
                    Signal = GetElementValue(columnNode, "Signal"),
                    Mode = GetElementValue(columnNode, "Mode"),
                    Unit = GetElementValue(columnNode, "Unit"),
                    Quantity = GetElementValue(columnNode, "Quantity")
                });
            }
        }

        /// <summary>
        /// MassData.binファイルの読み込み
        /// </summary>
        public void LoadMassDataBin()
        {
            DataPerser perser = new DataPerser();
            List<TraceVectorData> vectorsFromBin = perser.ReadMassDataBin(FilePath_MassData, RowCount, Columns);

            foreach (TraceVectorData vectorFromBin in vectorsFromBin)
            {
                TraceVectorData target = Vectors.FirstOrDefault(x => x.Name == vectorFromBin.Name);
                if (target == null)
                {
                    Vectors.Add(vectorFromBin);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(target.Unit))
                {
                    target.Unit = vectorFromBin.Unit;
                }

                target.TypedData.Clear();
                target.TypedData.AddRange(vectorFromBin.TypedData);
            }
        }
    }

    /// <summary>
    /// トレースベクトルデータ格納用クラス
    /// </summary>
    internal class TraceVectorData
    {
        /// <summary>独立変数かどうか</summary>
        public bool Independant { get; set; }

        /// <summary>信号名</summary>
        public string Name { get; set; }

        /// <summary>単位</summary>
        public string Unit { get; set; }

        /// <summary>データ列</summary>
        public List<double> TypedData { get; private set; } = new List<double>();
    }

    /// <summary>
    /// トレース列情報格納用クラス
    /// </summary>
    internal class TraceColumnData
    {
        /// <summary>信号名</summary>
        public string Signal { get; set; }

        /// <summary>モード</summary>
        public string Mode { get; set; }

        /// <summary>単位</summary>
        public string Unit { get; set; }

        /// <summary>物理量</summary>
        public string Quantity { get; set; }
    }
}
