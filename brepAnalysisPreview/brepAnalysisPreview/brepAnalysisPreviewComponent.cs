using System;
using System.Collections.Generic;
using System.Collections;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Collections;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;


namespace brepAnalysisPreview
{
    public class PTK_PreviewElementStructural : GH_Component
    {
        Dictionary<Brep, Color> models = new Dictionary<Brep, Color>();

        public PTK_PreviewElementStructural()
          : base("Color breps", "coloring",
              "Description",
              "Category3", "Preview")
        {
           // Message = CommonProps.initialMessage;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points for Breps", "P", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stresses for Breps", "S", "Stresses for each brep", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connectivity", "C", "Connectivity matrix", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "3d model", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<Brep> breps = new List<Brep>();
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>();

            GH_Structure<GH_Number> treeStress= new GH_Structure<GH_Number>();
            GH_Structure<GH_Integer> treeConnectivity = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();

            // --- input --- 
            if (!DA.GetDataTree(0, out treePoints)) return;
            if (!DA.GetDataTree(1, out treeStress)) return;
            if (!DA.GetDataTree(2, out treeConnectivity)) return;
            //Element1D element = gElement.Value;

            double maxStress = double.NegativeInfinity;
            double minStress = double.PositiveInfinity;

            //Find min and max stresses for solid
            for(int i = 0; i < treeStress.PathCount; i++)
            {
                double stress = treeStress[i][0].Value;

                if (stress > maxStress) maxStress = stress;
                if (stress < minStress) minStress = stress;
            }

            double range = (maxStress - minStress)/9;


            List<Mesh> meshes = new List<Mesh>();
            

            for (int i = 0; i < treePoints.PathCount; i++)
            {
                List<GH_Point> vertices = (List<GH_Point>)treePoints.get_Branch(i);
                var mesh = new Mesh();

                

                foreach (GH_Point p in vertices)
                {
                    mesh.Vertices.Add(p.Value);
                    //mesh.VertexColors.Add(color);
                }

                mesh.Faces.AddFace(0, 1, 5, 4);
                mesh.Faces.AddFace(1, 2, 6, 5);
                mesh.Faces.AddFace(2, 3, 7, 6);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(4, 5, 6, 7);
                mesh.Faces.AddFace(0, 1, 2, 3);

                Brep brep = Brep.CreateFromMesh(mesh,false);

                breps.Add(brep);
            }

            Color color = Color.White;

            for (int i = 0; i < breps.Count; i++){
                
                if (treeStress[i][0].Value < minStress + range) color = Color.Blue;
                else if (treeStress[i][0].Value < minStress + 2 * range) color = Color.Aqua;
                else if (treeStress[i][0].Value < minStress + 3 * range) color = Color.Teal;
                else if (treeStress[i][0].Value < minStress + 4 * range) color = Color.Olive;
                else if (treeStress[i][0].Value < minStress + 5 * range) color = Color.Green;
                else if (treeStress[i][0].Value < minStress + 6 * range) color = Color.Lime;
                else if (treeStress[i][0].Value < minStress + 7 * range) color = Color.Yellow;
                else if (treeStress[i][0].Value < minStress + 8 * range) color = Color.Orange;
                else color = Color.Red;


                tmpModels[breps[i]] = color;
            }
     

            /*
            // --- solve ---
            Dictionary<Curve, Color> sectionCurves = new Dictionary<Curve, Color>();
            Vector3d localY = element.CroSecLocalPlane.XAxis;
            Vector3d localZ = element.CroSecLocalPlane.YAxis;
            Point3d originSection = element.CroSecLocalPlane.Origin;

            if (element.CrossSection is Composite comp)
            {
                List<Tuple<CrossSection, Alignment>> secs = comp.RecursionCrossSectionSearch();
                foreach (var s in secs)
                {
                    Point3d originSubSection = originSection + s.Item2.OffsetY * localY + s.Item2.OffsetZ * localZ;

                    Plane localPlaneSubSection = new Plane(originSubSection, localY, localZ);

                    sectionCurves[new Rectangle3d(
                                localPlaneSubSection,
                                new Interval(-s.Item1.GetWidth() / 2, s.Item1.GetWidth() / 2),
                                new Interval(-s.Item1.GetHeight() / 2, s.Item1.GetHeight() / 2)).ToNurbsCurve()]
                                = Color.GhostWhite;
                }
            }
            else
            {
                sectionCurves[new Rectangle3d(
                                element.CroSecLocalPlane,
                                new Interval(-element.CrossSection.GetWidth() / 2, element.CrossSection.GetWidth() / 2),
                                new Interval(-element.CrossSection.GetHeight() / 2, element.CrossSection.GetHeight() / 2)).ToNurbsCurve()]
                                = Color.GhostWhite;
            }

            foreach (Curve s in sectionCurves.Keys)
            {
                Curve c = element.BaseCurve;
                if (c.IsLinear())
                {
                    Line l = new Line(c.PointAtStart, c.PointAtEnd);
                    Brep brep = Extrusion.CreateExtrusion(s, l.Direction).ToBrep();
                    //brep = brep.CapPlanarHoles(CommonProps.tolerances);
                    tmpModels[brep] = sectionCurves[s];
                }
                else
                {
                    Brep[] breps = Brep.CreateFromSweep(c, s, true, CommonProps.tolerances);
                    foreach (var brep in breps)
                    {
                        tmpModels[brep] = sectionCurves[s];
                    }
                }
            }
            */

             // --- output ---
             foreach (var m in tmpModels)
             {
                 models[m.Key] = m.Value;
             }
             
            DA.SetDataList(0, tmpModels.Keys);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //return Properties.Resources.PreElement;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7da0c2a7-ccb0-4f9e-b383-43b74bf51111"); }
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