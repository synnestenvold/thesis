using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class PointsToGeometry : GH_Component
    {
       
        public PointsToGeometry()
          : base("PointsToGeometry", "P2G",
              "Created brep from 8 points",
              "SolidsVR", "Geometry")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "8 points in right order", GH_ParamAccess.list);
        }

      
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Geometry for analysis", GH_ParamAccess.item);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            List<Point3d> points = new List<Point3d>();

            //---input---

            if (!DA.GetDataList(0, points)) return;

            //---solve---

            Mesh mesh = new Mesh();

            for (int i = 0; i < points.Count; i++)
            {
                mesh.Vertices.Add(points[i]);
            }
            mesh.Faces.AddFace(0, 1, 5, 4); //Front
            mesh.Faces.AddFace(1, 2, 6, 5); //Right
            mesh.Faces.AddFace(2, 3, 7, 6); //Back
            mesh.Faces.AddFace(0, 4, 7, 3); //Left
            mesh.Faces.AddFace(0, 3, 2, 1); //Bottom
            mesh.Faces.AddFace(4, 5, 6, 7); //Top

            Brep brep = Brep.CreateFromMesh(mesh, true);
            Curve[] curves = brep.DuplicateEdgeCurves();
            
            //---output---

            DA.SetData(0, brep);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.pointsToBrep;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("96ec01d3-1b69-4d00-9104-c18bf3f07d6d"); }
        }
    }
}