using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;


namespace MeshBox
{
    public class MeshBrepComponent : GH_Component
    {

        public MeshBrepComponent()
          : base("MeshBrep", "MeshB",
              "Description",
              "Category3", "Subcategory3")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Inupt Brep as a cube", GH_ParamAccess.item);
            pManager.AddNumberParameter("U Count", "U", "Number of quads in U direction", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("V Count", "V", "Number of quads in V direction", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("W Count", "W", "Number of quads in W direction", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Nodes", "N", "List of new node numbering", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Lengths", "L", "lx, ly and lz for the cubes", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brp = new Brep();
            double u = 1;
            double v = 1;
            double w = 1;

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            //Finding the length of the new elements
            Point3d[] nodes = brp.DuplicateVertices();
            double lx_new = (nodes[0].DistanceTo(nodes[1])) / u;
            double ly_new = (nodes[0].DistanceTo(nodes[3])) / v;
            double lz_new = (nodes[0].DistanceTo(nodes[4])) / w;
            List<double> lengths = new List<double> { lx_new, ly_new, lz_new };

            //
            List<List<int>> global_numbering = CreateNewBreps(brp, u, v, w, lx_new, ly_new, lz_new);
            DataTree<int> tree = new DataTree<int>();
            int i = 0;

            //Create a tree structure of the list of new brep-nodes
            foreach (List<int> innerList in global_numbering)
            {
                tree.AddRange(innerList, new GH_Path(new int[] { 0, i }));
                i++;
            }

            DA.SetDataTree(0, tree);
            DA.SetDataList(1, lengths);
        }

        private List<List<int>> CreateNewBreps(Brep brp, double u, double v, double w, double lx_new, double ly_new, double lz_new)
        {
            //Creating a list of brep-elements
            List<Brep> brep_elem = new List<Brep>();

            //Creating list of the coordinates of the big cube structure
            List<Point3d> all_nodes = new List<Point3d>();

            //Create brep-elements
            for (int k = 0; k <= u; k++)
            {
                for (int j = 0; j <= v; j++)
                {
                    for (int i = 0; i <= w; i++)
                    {
                        if (i < w && j < v && k < u)
                        {
                            Interval x = new Interval(lx_new * i, lx_new * (i + 1));
                            Interval y = new Interval(ly_new * j, ly_new * (j + 1));
                            Interval z = new Interval(lz_new * k, lz_new * (k + 1));
                            Box box_new = new Box(Plane.WorldXY, x, y, z);
                            Brep brep_new = box_new.ToBrep();
                            brep_elem.Add(brep_new); //Adds the smaller breps to the list
                        }


                        Point3d node = new Point3d(lx_new * i, ly_new * j, lz_new * k);
                        all_nodes.Add(node); //Adds each point to the list of nodes
                    }
                }
            }
            //We also want to relation between the local nodes in the brep-elements and the global nodes
            List<List<int>> global_numbering = new List<List<int>>();

            for (int b = 0; b < brep_elem.Count; b++) //For each smaller brep...
            {
                Point3d[] brep_nodes = brep_elem[b].DuplicateVertices();
                List<int> brep_numbering = new List<int>();
                for (int n = 0; n < brep_nodes.Length; n++) //And each node in the smaller brep...
                {
                    for (int m = 0; m < all_nodes.Count; m++)
                    {
                        if (brep_nodes[n].Equals(all_nodes[m])) //Compare the local node to the global node list
                        {
                            brep_numbering.Add(m); //And add the global node-index to the list. NB: Starts at 0, not 1.
                        }
                    }
                }
                global_numbering.Add(brep_numbering); //Putting all the lists in a list
            }

            return global_numbering;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ceb23bca-cd95-49ab-9580-01f32e152c2c"); }
        }
    }
}
