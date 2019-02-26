using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace MeshBox
{
    public class MeshBoxComponent : GH_Component
    {
  
        public MeshBoxComponent()
          : base("MeshBox", "MeshB",
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
            pManager.AddNumberParameter("Nodes", "N", "List of new node numbering", GH_ParamAccess.tree);
         
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            Brep brp = new Brep();
            double u = 1;
            double v = 1;
            double w = 1;
            
       

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            //Point3d[] global_nodes = brp.DuplicateVertices();

            if (u < 1 || v < 1 || w < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            List<List<int>> global_numbering = CreateNewBreps(brp, u, v, w);

            
            DataTree<int> tree = new DataTree<int>();
            int i = 0;
            foreach (List<int> innerList in global_numbering)
            {
                tree.AddRange(innerList, new GH_Path(new int[] { 0, i }));
                i++;
            }

            DA.SetDataTree(0, tree);
        }

        private List<List<int>> CreateNewBreps(Brep brp, double u, double v, double w)
        {
            Point3d[] nodes = brp.DuplicateVertices();
            double lx = nodes[0].DistanceTo(nodes[1]);
            double ly = nodes[0].DistanceTo(nodes[3]);
            double lz = nodes[0].DistanceTo(nodes[4]);

            double lx_new = lx / u;
            double ly_new = lx / v;
            double lz_new = lx / w;
            List<Brep> breps = new List<Brep>();
            List<Point3d> global_nodes = new List<Point3d>();

            for (int k = 0; k <= u; k++)
            {
                for (int j = 0; j <= v; j++)
                {
                    for (int i = 0; i <= w; i++)
                    {
                        if (i < u && j < v && k < w)
                        {
                            Interval x = new Interval(lx_new * i, lx_new * (i + 1));
                            Interval y = new Interval(ly_new * j, ly_new * (j + 1));
                            Interval z = new Interval(lz_new * k, lz_new * (k + 1));
                            Box box_new = new Box(Plane.WorldXY, x, y, z);
                            Brep brep_new = box_new.ToBrep();
                            breps.Add(brep_new); //Adds the smaller breps to the list
                        }
                        
                        Point3d node = new Point3d(lx_new*i, ly_new*j, lz_new*k);
                        global_nodes.Add(node); //Adds each point to the list of global nodes
                    }
                }
            }
            List<List<int>> global_numbering = new List<List<int>>();

            for (int b = 0; b< breps.Count; b++) //for each smaller brep...
            {
                Point3d[] brep_nodes = breps[b].DuplicateVertices();
                List<int> brep_numbering = new List<int>();
                for (int n = 0; n < brep_nodes.Length; n++) //and each node in the smaller brep...
                {
                    for (int m = 0; m < global_nodes.Count; m++)
                    {
                        if (brep_nodes[n].Equals(global_nodes[m])) //compare the point to the global node list
                        {
                            brep_numbering.Add(m + 1); //and add the global node-index to the list
                        }
                    }
                }
                global_numbering.Add(brep_numbering); //putting all the lists in a list
            }

            return global_numbering;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
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
            get { return new Guid("b0c2516b-1b1a-4e27-a2e5-6c4e01655683"); }
        }
    }
}
