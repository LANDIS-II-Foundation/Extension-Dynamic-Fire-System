using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Core;
using System.IO;
using Landis.Utilities;

namespace Landis.Extension.DynamicFire
{
    public static class MetadataHandler
    {

        public static ExtensionMetadata Extension { get; set; }
        public static void InitializeMetadata(string mapNameTemplate, string eventLogName, string summaryLogName)
        {

            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore)
            //Extension = new ExtensionMetadata()
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = PlugIn.ModelCore.CurrentTime,
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            CreateDirectory(eventLogName);
            CreateDirectory(summaryLogName);
            PlugIn.eventLog = new MetadataTable<EventsLog>(eventLogName);
            PlugIn.summaryLog = new MetadataTable<SummaryLog>(summaryLogName);

            PlugIn.ModelCore.UI.WriteLine("   Generating event table...");
            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "EventLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            PlugIn.ModelCore.UI.WriteLine("   Generating summary table...");
            OutputMetadata tblOut_summary = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "SummaryLog",
                FilePath = PlugIn.summaryLog.FilePath,
                Visualize = true,
            };
            tblOut_summary.RetriveFields(typeof(SummaryLog));
            Extension.OutputMetadatas.Add(tblOut_summary);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            OutputMetadata mapOut_fire_severity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Fire_Severity_Map",
                FilePath = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.ModelCore.CurrentTime),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_fire_severity);

            OutputMetadata mapOut_last_fire = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "Time_Of_Last_Fire",
                FilePath = MapNames.ReplaceTemplateVars("./DFFS-output/TimeOfLastFire-{timestep}.tif", PlugIn.ModelCore.CurrentTime),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_last_fire);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);
        }
        public static void CreateDirectory(string path)
        {
            //Require.ArgumentNotNull(path);
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Landis.Utilities.Directory.EnsureExists(dir);
            }

            //return new StreamWriter(path);
            return;
        }
    }
}