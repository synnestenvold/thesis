using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR.Components
{
    public class MeshCBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshCBrep class.
        /// </summary>
        public MeshCBrep()
          : base("MeshCBrep", "MeshCBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Input geometry as a curved brep", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item, 1);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ---variables-- -

            Brep brp = new Brep();
            int u = 1;
            int v = 1;
            int w = 1;

            // --- input ---

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            // --- solve ---

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }



            var tuple = CreateNewBreps(nodes, u, v, w); // Getting corner nodes and connectivity matrix

            List<List<Point3d>> elementPoints = tuple.Item1;
            List<List<int>> connectivity = tuple.Item2;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            Mesh_class mesh = new Mesh_class(u, v, w);
            mesh.SetConnectivity(connectivity);
            mesh.SetElementPoints(elementPoints);
            mesh.SetSizeOfMatrix(sizeOfMatrix);
            mesh.SetGlobalPoints(globalPoints);

            //---output---

            DA.SetData(0, mesh);
        }

        private Tuple<List<List<Point3d>>, List<List<int>>> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        {
            return null;
        }
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("560285f9-b7cd-469e-ba51-cbf32382362c"); }
        }
    }
}