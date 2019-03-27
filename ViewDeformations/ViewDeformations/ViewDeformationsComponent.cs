using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;



namespace ViewDeformations
{
    public class ViewDeformationsComponent : GH_Component
    {
        Dictionary<Brep, Color> models = new Dictionary<Brep, Color>();

        public ViewDeformationsComponent()
          : base("ViewDeformations", "ViewDef",
              "Description",
              "Category3", "Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connectivity", "C", "", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points for Breps", "N", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scaling", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "3d Model", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>(); 
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> treeConnect = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Number> treeDef = new GH_Structure<GH_Number>();
            double scale = 1;
            
            if (!DA.GetDataTree(1, out treePoints)) return;
            if (!DA.GetDataTree(0, out treeConnect)) return;
            if (!DA.GetDataTree(2, out treeDef)) return;
            if (!DA.GetData(3, ref scale)) return;
            
            Vector3d[] defVectors = new Vector3d[treeDef.PathCount];
            defVectors = CreateVectors(treeDef, scale);
            breps = CreateDefBreps(treePoints, treeConnect, defVectors);
            
            
            //Coloring
            Color color = Color.White;
            for (int i = 0; i < breps.Count; i++)
            {
                //if (defVectors[i].Length < limit) color = Color.Green;
                //else color = Color.Red;
                tmpModels[breps[i]] = color;
            }
            
            //Output
            foreach (var m in tmpModels)
            {
                models[m.Key] = m.Value;
            }

            DA.SetDataList(0, tmpModels.Keys);
        }
        

        public Vector3d[] CreateVectors(GH_Structure<GH_Number> treeDef, double scale)
        {
            int number = treeDef.PathCount;
            Vector3d[] vectors = new Vector3d[number];
            for (int i = 0; i< number; i++)
            {
                //List<GH_Number> def = (List<GH_Number>)treeDef.get_Branch(i);
                //double def1 = def[0].Value;
                List<GH_Number> def = (List<GH_Number>)treeDef.get_Branch(i);
                Vector3d vector = new Vector3d((def[0].Value)*scale, (def[1].Value)*scale, (def[2].Value)*scale);
                vectors[i] = vector;
            }
            return vectors;
        }

        public List<Brep> CreateDefBreps(GH_Structure<GH_Point> treePoints, GH_Structure<GH_Integer> treeConnect, Vector3d[] defVectors)
        {
            List<Brep> breps = new List<Brep>();
            for (int j = 0; j < treePoints.PathCount; j++)
            {
                List<GH_Point> vertices = (List<GH_Point>)treePoints.get_Branch(j);
                var mesh = new Mesh();
                List<GH_Integer> connect = (List<GH_Integer>)treeConnect.get_Branch(j);

                for (int i = 0; i < vertices.Count; i++)
                {
                    GH_Point p = vertices[i];
                    Point3d new_p = Point3d.Add(p.Value, defVectors[connect[i].Value]);
                    mesh.Vertices.Add(new_p);
                }
                mesh.Faces.AddFace(0, 1, 5, 4);
                mesh.Faces.AddFace(1, 2, 6, 5);
                mesh.Faces.AddFace(2, 3, 7, 6);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(4, 5, 6, 7);
                mesh.Faces.AddFace(0, 1, 2, 3);

                Brep new_brep = Brep.CreateFromMesh(mesh, false);
                breps.Add(new_brep);
                
            }
            return breps;
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
            get { return new Guid("0391f003-161b-49e1-931b-71ca89c69aa6"); }
        }

        public override void ExpireSolution(bool recompute)
        {
            models.Clear();
            base.ExpireSolution(recompute);
        }

        //public override BoundingBox ClippingBox => models.Keys.ToList()[0].GetBoundingBox(false);
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            foreach (var m in models)
            {
                args.Display.DrawBrepShaded(m.Key, new Rhino.Display.DisplayMaterial(m.Value));
            }
            //base.DrawViewportMeshes(args);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            //base.DrawViewportWires(args);
        }
    }
}
