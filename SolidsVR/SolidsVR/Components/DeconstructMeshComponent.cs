using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;


namespace SolidsVR
{
    public class DeconstructMeshComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructMesh class.
        /// </summary>
        public DeconstructMeshComponent()
          : base("DeconstructMesh", "Deconstruct Mesh",
              "Deconstruct Mesh",
              "SolidsVR", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Element vertices", "V", "Vertices in each element", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Connectivity", "C", "Connectivity for each element", GH_ParamAccess.tree);
            pManager.AddLineParameter("Edges", "E", "Edges for each element", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Surfaces", "S", "Surfaces for each element", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "P", "Global coordinates", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "N", "Nodes class", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "Elements in mesh", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            MeshGeometry mesh = new MeshGeometry();

            if (!DA.GetData(0, ref mesh)) return;

            List<List<Point3d>> elementPoints = mesh.GetElementPoints();
            List<List<int>> connectivity = mesh.GetConnectivity();
            List<List<Line>> edgesMesh = mesh.GetEdges();
            List<List<Brep>> surfacesMesh = mesh.GetSurfaces();
            Point3d[] globalPoints = mesh.GetGlobalPoints();
            List<Node> nodes = mesh.GetNodeList();
            List<Element> elements = mesh.GetElements();

            DataTree<Point3d> treePoints = new DataTree<Point3d>();
            DataTree<int> treeConnectivity = new DataTree<int>();
            DataTree<Line> treeEdges = new DataTree<Line>();
            DataTree<Brep> treeSurfaces = new DataTree<Brep>();

            
            for(int i = 0; i < elementPoints.Count; i++)
            {
                treePoints.AddRange(elementPoints[i], new GH_Path(new int[] { 0, i }));
                treeConnectivity.AddRange(connectivity[i], new GH_Path(new int[] { 0, i }));
                treeEdges.AddRange(edgesMesh[i], new GH_Path(new int[] { 0, i }));
                treeSurfaces.AddRange(surfacesMesh[i], new GH_Path(new int[] { 0, i }));
            }


            DA.SetDataTree(0, treePoints);
            DA.SetDataTree(1, treeConnectivity);
            DA.SetDataTree(2, treeEdges);
            DA.SetDataTree(3, treeSurfaces);
            DA.SetDataList(4, globalPoints);
            DA.SetDataList(5, nodes);
            DA.SetDataList(6, elements);

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
                return SolidsVR.Properties.Resource1.decMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e2f20dd4-1bc6-4650-a099-2d690d8824bc"); }
        }
    }
}