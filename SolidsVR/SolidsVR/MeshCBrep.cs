using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

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

            Curve[] edges = brp.DuplicateEdgeCurves();

            Curve[] sortedEdges = SortEdges(edges);
            

            var tuple = CreateNewBreps(sortedEdges, u, v, w); // Getting corner nodes and connectivity matrix

            //List<List<Point3d>> elementPoints = tuple.Item1;
            //List<List<int>> connectivity = tuple.Item2;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            //Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            Mesh_class mesh = new Mesh_class(u, v, w);
            //mesh.SetConnectivity(connectivity);
            //mesh.SetElementPoints(elementPoints);
            //mesh.SetSizeOfMatrix(sizeOfMatrix);
            //mesh.SetGlobalPoints(globalPoints);

            //---output---

            DA.SetData(0, mesh);
        }

        private List<Point3d> CreateNewBreps(Curve[] edges, int u, int v, int w)
        {
            List<Point3d> points = new List<Point3d>();
            List<List<Point3d>> uDiv = new List<List<Point3d>>();
            List<List<Point3d>> vDiv = new List<List<Point3d>>();
            List<List<Point3d>> wDiv = new List<List<Point3d>>();
            
            
            
            for (int i=0; i<4; i++)
            {
                Point3d[] uP = new Point3d[u+1];
                Point3d[] vP = new Point3d[v+1];
                Point3d[] wP = new Point3d[w+1];
                edges[i].DivideByCount(u, true, out uP);
                edges[i+4].DivideByCount(v, true, out vP);
                edges[i+8].DivideByCount(w, true, out wP);
                //List<Point2> lst = ints.OfType<int>().ToList();
                
                uDiv.Add(uP.ToList());
                vDiv.Add(vP.ToList());
                wDiv.Add(wP.ToList());

            }
           
            //W-dir

            for (int i =0; i<w; i++)
            {
                
                for (int j=0; j<v; j++)
                {
                    for (int k =0; k<u; k++)
                    {
                        points.AddRange(uDiv[k]);

                    }
                }
            }

            //V-dir

            //W-dir
            return points;
        }

        public Curve[] SortEdges(Curve[] edges)
        {
            Curve[] sortedEdges = new Curve[12];

            sortedEdges[0] = edges[7]; //u-dir
            sortedEdges[0].Reverse();
            sortedEdges[1] = edges[5];
            sortedEdges[2] = edges[1];
            sortedEdges[3] = edges[3];
            sortedEdges[2].Reverse();
            sortedEdges[4] = edges[4]; //v-dir
            sortedEdges[5] = edges[6];
            sortedEdges[5].Reverse();
            sortedEdges[6] = edges[2];
            sortedEdges[7] = edges[0];
            sortedEdges[7].Reverse();
            sortedEdges[8] = edges[8]; //w-dir
            sortedEdges[8].Reverse();
            sortedEdges[9] = edges[9];
            sortedEdges[9].Reverse();
            sortedEdges[10] = edges[10];
            sortedEdges[10].Reverse();
            sortedEdges[11] = edges[11];
            sortedEdges[11].Reverse();

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