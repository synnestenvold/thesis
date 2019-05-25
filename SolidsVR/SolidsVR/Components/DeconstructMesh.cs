using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace SolidsVR
{
    public class DeconstructMesh : GH_Component
    {
        public DeconstructMesh()
          : base("DeconstructMesh", "Deconstruct Mesh",
              "Get information about mesh",
              "SolidsVR", "Deconstruct")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Nodes", "N", "List of all nodes in mesh", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "List of all elements in mesh", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Global coordinates", GH_ParamAccess.list);
            pManager.AddNumberParameter("SizeOfM", "S", "Size of matrices.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Connectivity", "C", "Connectivity for each element", GH_ParamAccess.tree);
            pManager.AddPointParameter("Element vertices", "V", "Vertices in each element", GH_ParamAccess.tree);
            pManager.AddLineParameter("Edges", "Edges", "Edges for each element", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Surfaces", "Surfaces", "Surfaces for each element", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            MeshGeometry mesh = new MeshGeometry();

            // --- input ---

            if (!DA.GetData(0, ref mesh)) return;

            // --- solve ---

            List<Node> nodes = mesh.GetNodeList();
            List<Element> elements = mesh.GetElements();
            List<Point3d> globalPoints = mesh.GetGlobalPoints();
            int sizeOfM = mesh.GetSizeOfMatrix();

            List<List<Point3d>> elementPoints = mesh.GetElementPoints();
            List<List<int>> connectivity = mesh.GetConnectivity();
            List<List<Line>> edgesMesh = mesh.GetEdges();
            List<List<Brep>> surfacesMesh = mesh.GetSurfaces();

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

            // --- output ---

            DA.SetDataList(0, nodes);
            DA.SetDataList(1, elements);
            DA.SetDataList(2, globalPoints);
            DA.SetData(3, sizeOfM);
            DA.SetDataTree(4, treeConnectivity);
            DA.SetDataTree(5, treePoints);
            DA.SetDataTree(6, treeEdges);
            DA.SetDataTree(7, treeSurfaces);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.decMesh;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e2f20dd4-1bc6-4650-a099-2d690d8824bc"); }
        }
    }
}