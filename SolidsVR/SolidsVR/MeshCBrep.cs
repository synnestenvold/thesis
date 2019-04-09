using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
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

        public Curve[] sortEdges(Curve[] edges)
        {
            Curve[] sortedEdges = new Curve[12];

            sortedEdges[0] = edges[5];
            sortedEdges[1] = edges[7];
            sortedEdges[2] = edges[3];
            sortedEdges[3] = edges[1];
            sortedEdges[4] = edges[4];
            sortedEdges[5] = edges[6];
            sortedEdges[6] = edges[2];
            sortedEdges[7] = edges[0];
            sortedEdges[8] = edges[11];
            sortedEdges[9] = edges[10];
            sortedEdges[10] = edges[9];
            sortedEdges[11] = edges[8];

            return sortedEdges;
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0f4702ea-a195-4b83-b3ae-5e067f56a73f"); }
        }
    }
}