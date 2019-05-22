using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Geometry;

namespace SolidsVR
{
    public class DeconstructElement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructElement class.
        /// </summary>
        public DeconstructElement()
          : base("Deconstruct Element", "Deconstruct Element",
              "Description",
              "Category3", "Element")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "Element", "Element in brep", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "Nodes", "Nodes inside element", GH_ParamAccess.list);
            pManager.AddNumberParameter("ElementNr", "ElementNr", "Number of elment in global elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("Connectivity", "Connectivity", "Connectivity in each element", GH_ParamAccess.list);
            pManager.AddNumberParameter("AvgMises", "AvgM", "Average von Mises in each element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Element element = new Element(new List<Node>(), 0, new List<int>());

            if (!DA.GetData(0, ref element)) return;

            List<Node> nodes = element.GetVertices();
            int elementNr = element.GetElementNr();
            List<int> connectivity = element.GetConnectivity();
            double avgStress = element.GetAverageStressDir(6);


            DA.SetDataList(0, nodes);
            DA.SetData(1, elementNr);
            DA.SetDataList(2, connectivity);
            DA.SetData(3, avgStress);
        }

        /// <summary>s
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
            get { return new Guid("91b1ec96-164b-4629-a288-ec61a1634b4c"); }
        }
    }
}