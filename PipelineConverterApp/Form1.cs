using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using PilotGaea.Serialize;
using PilotGaea.TMPEngine;
using PilotGaea.Geometry;

namespace PipelineConverterApp
{
    public partial class Form1 : Form
    {
        CPipelineMaker m_Maker = null;
        Stopwatch m_Stopwatch = new Stopwatch();

        public enum SourceType
        {
            SHP, GML
        }

        public Form1()
        {
            InitializeComponent();

            //加入來源列表
            comboBox_Source.Items.AddRange(Enum.GetNames(typeof(SourceType)));
            comboBox_Source.SelectedIndex = 0;

            //加入功能列表
            List<string> featureNames = new List<string>();
            featureNames.Add("基本管線");
            featureNames.Add("輸出OGC I3S");
            featureNames.Add("輸出OGC 3DTiles");
            comboBox_Features.Items.AddRange(featureNames.ToArray());
            comboBox_Features.SelectedIndex = 0;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            EnableUI(false);

            //抓出SourceType
            SourceType sourceType = (SourceType)Enum.Parse(typeof(SourceType), comboBox_Source.SelectedItem.ToString());

            //將來源資料輸出成pipeline圖層
            System.Environment.CurrentDirectory = @"C:\Program Files\PilotGaea\TileMap";//為了順利存取安裝目錄下的相關DLL
            m_Maker = new CPipelineMaker();
            //設定必要參數
            //     圖層名稱
            string LayerName = "test";
            //     資料庫路徑
            string LayerDBFile = string.Format(@"{0}\..\output\pipeline_maker_{1}.DB", Application.StartupPath, sourceType.ToString());
            //     參照的地形圖層名稱
            string TerrainName = "terrain";
            //     地形圖層位於的資料庫路徑
            string TerrainDBFile = string.Format(@"{0}\..\data\terrain_maker\terrain.DB", Application.StartupPath);
            //     PIPELINE_SHP_SRC來源列表，可多個來源合併成一個圖層
            List<PIPELINE_SHP_SRC> Sources = new List<PIPELINE_SHP_SRC>();
            switch (sourceType)
            {
                default : break;
                case SourceType.SHP:
                    Sources.Add(new PIPELINE_SHP_SRC(string.Format(@"{0}\..\data\pipeline_maker\中華電信_線.shp", Application.StartupPath), 3826));
                    break;
                case SourceType.GML:
                    Sources.Add(new PIPELINE_SHP_SRC(string.Format(@"{0}\..\data\pipeline_maker\範例_自來水管線water_uty.gml", Application.StartupPath), 3826)
                    {
                        GMLTagName = "UTL_管線_自來水"
                    });
                    break;
            }

            //監聽轉檔事件
            m_Maker.CreateLayerCompleted += M_Maker_CreateLayerCompleted;
            m_Maker.ProgressMessageChanged += M_Maker_ProgressMessageChanged;
            m_Maker.ProgressPercentChanged += M_Maker_ProgressPercentChanged;

            //設定進階參數
            switch (comboBox_Features.SelectedIndex)
            {
                case 0://"基本"
                    break;
                case 1://"輸出OGC I3S"
                    break;
                case 2://"輸出OGC 3DTiles
                    break;
            }

            m_Stopwatch.Restart();
            //開始非同步轉檔
            bool ret = false;
            switch (comboBox_Features.SelectedIndex)
            {
                case 0://"基本"
                    ret = m_Maker.CreatePipeline(EXPORT_TYPE.LET_DB, LayerName, LayerDBFile, TerrainName, TerrainDBFile, Sources);
                    break;
                case 1://"輸出OGC I3S"
                    LayerName = "pipeline_maker_ogci3s";
                    //會在destPath目錄下產生layerName.slpk
                    ret = m_Maker.CreatePipeline(EXPORT_TYPE.LET_OGCI3S, LayerName, LayerDBFile, TerrainName, TerrainDBFile, Sources);
                    break;
                case 2://"輸出OGC 3DTiles
                    LayerName = "pipeline_maker_ogc3dtiles";
                    //會在destPath目錄下產生layerName資料夾
                    ret = m_Maker.CreatePipeline(EXPORT_TYPE.LET_OGC3DTILES, LayerName, LayerDBFile, TerrainName, TerrainDBFile, Sources);
                    break;
            }

            string message = string.Format("Create{0}", (ret ? "通過" : "失敗"));
            listBox_Main.Items.Add(message);
        }

        private void M_Maker_ProgressPercentChanged(double Percent)
        {
            progressBar_Main.Value = Convert.ToInt32(Percent);
        }

        private void M_Maker_ProgressMessageChanged(string Message)
        {
            listBox_Main.Items.Add(Message);
        }

        private void M_Maker_CreateLayerCompleted(string LayerName, bool Success, string ErrorMessage)
        {
            m_Stopwatch.Stop();
            string message = string.Format("轉檔{0}", (Success ? "成功" : "失敗"));
            listBox_Main.Items.Add(message);
            message = string.Format("耗時{0}分。", m_Stopwatch.Elapsed.TotalMinutes.ToString("0.00"));
            listBox_Main.Items.Add(message);
        }

        private void EnableUI(bool enable)
        {
            button_Start.Enabled = enable;
            comboBox_Features.Enabled = enable;
        }
    }
}