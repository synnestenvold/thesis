﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class DeconstructNode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructNode class.
        /// </summary>
        public DeconstructNode()
          : base("Deconstruct Node", "Deconstruct Node",
              "Description",
              "Category3", "Node")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node for Brep", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Coordinates of the node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Elements", "Elements", "Which elements node include", GH_ParamAccess.list);
            pManager.AddTextParameter("Position", "StringPos", "Position of node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Surfaces", "Surfaces", "Which surface node include", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Node node = new Node(new Point3d(0,0,0),0);

            if (!DA.GetData(0, ref node)) return;

            String text = "";

            Boolean isCorner = node.GetIsCorner();
            Boolean isEdge = node.GetIsEdge();
            Boolean isMiddle = node.GetIsMiddle();

            if (isCorner) text = "Corner";
            if (isEdge) text = "Edge";
            if (isMiddle) text = "Middle";

            List<int> i = node.GetSurfaceNum();

            List<int> elementNr = node.GetElementNr();

            Point3d point = node.GetCoord();

            DA.SetData(0, point);
            DA.SetDataList(1, elementNr);
            DA.SetData(2, text);
            DA.SetDataList(3, i);
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b0ff2068-2969-435a-b890-c7af52d6305a"); }
        }
    }
}