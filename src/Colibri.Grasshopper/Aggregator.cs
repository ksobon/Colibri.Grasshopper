﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;

namespace Aggregator
{
    public class Aggregator : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Aggregator()
          : base("Aggregator", "Aggregator",
              "Aggregates design input data, performance metrics, image & json filemanes into a data.csv file for Design Explorer to open.",
              "Colibri", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get {return GH_Exposure.tertiary;} }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FolderPath", "Folder", "Folder path", GH_ParamAccess.item);
            pManager.AddTextParameter("inputsDataSet", "Inputs", "Inputs data", GH_ParamAccess.list);
            pManager.AddTextParameter("outputsDataSet", "Outputs", "Outputs data", GH_ParamAccess.list);
            pManager.AddTextParameter("imageParams", "ImgParams", "ImageParams like height, width of output images", GH_ParamAccess.list);
            pManager.AddBooleanParameter("writeFile", "WriteFile", "Set to yes to run", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("writeInData", "WriteInData", "Use panel to check current data", GH_ParamAccess.item);
            pManager.AddTextParameter("SpectaclesFileName", "SpectaclesFileName",
                "Feed this into the Spectacles_SceneCompiler component downstream.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input variables
            string folder = "";
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            List<string> imgParams = new List<string>();
            bool writeFile = false;
            
            //get data
            DA.GetData(0, ref folder);
            DA.GetDataList(1, inputs);
            DA.GetDataList(2, outputs);
            DA.GetDataList(3, imgParams);
            DA.GetData(4, ref writeFile);

            //operations
            string csvPath = folder + "/data.csv";
            var rawData = inputs;
            int inDataLength = rawData.Count;
            rawData.AddRange(outputs);
            int allDataLength = rawData.Count;

            string imgName = "";
            string imgPath = "";
            string keyReady = "";
            string valueReady = "";
            

            //format write in data
            for (int i = 0; i < rawData.Count; i++)
            {
                string item = Convert.ToString(rawData[i]).Replace("[", "").Replace("]", "").Replace(" ", "");
                string dataKey = item.Split(',')[0];
                string dataValue = item.Split(',')[1];

                if (i > 0) 
                {
                    if (i < inDataLength)
                    {
                        keyReady += ",in:" + dataKey;
                    }
                    else
                    {
                        keyReady += ",out:" + dataKey;
                    }

                    valueReady = valueReady + "," + dataValue;
                    imgName = imgName + "_" + dataValue;
                }
                else
                {
                    //the first set
                    keyReady = "in:" + dataKey;
                    valueReady += dataValue;
                    imgName = dataValue;
                } 
            }


            bool run = writeFile;
            string fileName = imgName;
            imgPath = folder+"/"+imgName + ".png";
            imgName += ".png";
            string jsonFilePath = folder + "/" + fileName + ".json";
            string jsonFileName = fileName + ".json";
            string writeInData = "";
            List<int> cleanedImgParams = new List<int>();
            foreach (string item in imgParams) 
            {
                string cleanItem = Convert.ToString(item).Replace("[", "").Replace("]", "").Replace(" ", "");
                cleanedImgParams.Add(Convert.ToInt32(cleanItem.Split(',')[1]));
            }

            Size viewSize = new Size(cleanedImgParams[0], cleanedImgParams[1]);
            
            
            if (run)
               {
                //check csv file
                if (!File.Exists(csvPath))
                {
                    keyReady = keyReady + ",img,threeD" + Environment.NewLine;
                    File.WriteAllText(csvPath, keyReady);
                }

                //save imgs
                var pic = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(viewSize);
                pic.Save(imgPath);

                //save csv
                writeInData = string.Format("{0},{1},{2}\n", valueReady, imgName, jsonFileName);
                File.AppendAllText(csvPath, writeInData);

            }
            
            //set output
            DA.SetData(0, writeInData);
            DA.SetData(1, jsonFilePath);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Colibri.Grasshopper.Properties.Resources.Colibri_logobase_4;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e1}"); }
        }
    }
}
