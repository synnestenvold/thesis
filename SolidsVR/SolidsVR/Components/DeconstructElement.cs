using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace SolidsVR
{
    public class DeconstructElement : GH_Component
    {
    
        public DeconstructElement()
          : base("DeconstructElement", "DeconstructElement",
              "Get information about element",
              "SolidsVR", "Deconstruct")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "Element in brep", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Nodes inside element", GH_ParamAccess.list);
            pManager.AddNumberParameter("ElementNr", "nr", "Element number in global elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("Connectivity", "C", "Connectivity in each element", GH_ParamAccess.list);
            pManager.AddNumberParameter("AvgMises", "AvgM", "Average von Mises in each element", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Removable", "R", "Is element removable", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            Element element = new Element(new List<Node>(), 0, new List<int>());

            // --- input ---

            if (!DA.GetData(0, ref element)) return;

            // --- solve ---

            List<Node> nodes = element.GetVertices();
            int elementNr = element.GetElementNr();
            List<int> connectivity = element.GetConnectivity();
            double avgStress = element.GetAverageStressDir(6);
            Boolean isRemovable = element.IsRemovable();

            // --- output ---

            DA.SetDataList(0, nodes);
            DA.SetData(1, elementNr);
            DA.SetDataList(2, connectivity);
            DA.SetData(3, avgStress);
            DA.SetData(4, isRemovable);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.decElement;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("91b1ec96-164b-4629-a288-ec61a1634b4c"); }
        }
    }
}