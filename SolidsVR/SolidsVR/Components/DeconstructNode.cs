using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;


namespace SolidsVR
{
    public class DeconstructNode : GH_Component
    {

        public DeconstructNode()
          : base("Deconstruct Node", "DeconstructNode",
              "Deconstruct Node",
              "SolidsVR", "Deconstruct")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Node instance", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Coordinate", "C", "Coordinates of the node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Elements", "E", "Elements node is part of", GH_ParamAccess.list);
            pManager.AddTextParameter("Position", "P", "Position of node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Surfaces", "S", "Surfaces node is part of", GH_ParamAccess.list);
            pManager.AddNumberParameter("Deformations", "D", "Deformations in node", GH_ParamAccess.list);
            pManager.AddNumberParameter("Global Strain", "strains", "Averaged strains in node", GH_ParamAccess.list);
            pManager.AddNumberParameter("Global Stress", "Stress", "Averaged stress in node", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            Node node = new Node(new Point3d(0,0,0),0);

            // --- input ---

            if (!DA.GetData(0, ref node)) return;

            // --- solve ---

            Point3d coordinate = node.GetCoord();
            List<int> elementNr = node.GetElementNr();

            String pos = "";
            Boolean isCorner = node.GetIsCorner();
            Boolean isEdge = node.GetIsEdge();
            Boolean isMiddle = node.GetIsMiddle();

            if (isCorner) pos = "Corner";
            if (isEdge) pos = "Edge";
            if (isMiddle) pos = "Middle";

            List<int> surfaceNum = node.GetSurfaceNum();
            List<double> def = node.GetDeformation();
            Vector<double> globalStrain = node.GetGlobalStrain();
            Vector<double> globalStress = node.GetGlobalStress();

            // --- output ---

            DA.SetData(0, coordinate);
            DA.SetDataList(1, elementNr);
            DA.SetData(2, pos);
            DA.SetDataList(3, surfaceNum);
            DA.SetDataList(4, def);
            DA.SetDataList(6, globalStrain);
            DA.SetDataList(7, globalStress);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.decNode;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b0ff2068-2969-435a-b890-c7af52d6305a"); }
        }
    }
}