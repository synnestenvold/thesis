using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Linq;
using System.Drawing;

namespace SolidsVR.Components
{
    public class FEMSolver : GH_Component
    {
        public FEMSolver()
          : base("FEMSolver", "FEMSolver",
              "Component for FEA",
              "SolidsVR", "Analyze")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
            pManager.AddTextParameter("Boundary conditions", "BC", "Nodes that are constrained", GH_ParamAccess.list);
            pManager.AddTextParameter("PointLoads", "PL", "Input loads", GH_ParamAccess.list);
            pManager.AddTextParameter("PreDeformations", "PreDef", "Input deformations", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.tree);
            pManager.AddTextParameter("Text", "Text", "Text for headline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Color T", "Colors for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            MeshGeometry mesh = new MeshGeometry();
            List<string> bctxt = new List<string>();
            List<string> loadtxt = new List<string>();
            List<string> deftxt = new List<string>();
            Material material = new Material();
            Boolean opt = false;


            // --- input ---

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, bctxt)) return;
            if (!DA.GetDataList(2, loadtxt)) return;
            if (!DA.GetDataList(3, deftxt)) return;
            if (!DA.GetData(4, ref material)) return;

            // --- setup ---

            List<List<int>> connectivity = mesh.GetConnectivity();
            List<Point3d> globalPoints = mesh.GetGlobalPoints();
            List<Node> nodes = mesh.GetNodeList();
            BrepGeometry brp = mesh.GetBrep();
            Brep origBrep = brp.GetBrep();
            
            int sizeOfMatrix = mesh.GetSizeOfMatrix();
            int removableElements = FindRemovableElements(nodes, mesh.GetElements());
            int totalElements = mesh.GetElements().Count;
            int numberElements = mesh.GetElements().Count;
            double removableVolume = mesh.GetOptVolume();
            double minElements = totalElements - Math.Floor(totalElements * removableVolume / 100);

            // --- solve ---

            if (removableVolume != 0) opt = true;

            Boolean first = true;
            double max = 0;
            int removeElem = -1;
            List<int> removeNodeNr = new List<int>();
            DataTree<double> defTree = new DataTree<double>();

            while (numberElements > minElements && max < material.GetY() || first) //Requirements for removal                                                                
            {
                first = false;

                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].CleanStress();
                }

                List<Element> elements = mesh.GetElements();

                //Remove selected element from last iterations, and update afftected nodes
                if (removeElem != -1 && opt == true)
                {
                    RemoveElementAndUpdateNodes(elements, removeElem, removeNodeNr);
                }

                //Create Ktot and B
                Matrix<double> Ktot = CreateGlobalStiffnessMatrix(sizeOfMatrix, material, elements);

                //Create boundary condition list AND predeformations
                (List<int> bcNodes, List<double> reatrains) = CreateBCList(bctxt, globalPoints);

                (List<int> predefNodes, List<double> predef) = CreateBCList(deftxt, globalPoints);

                //Setter 0 i hver rad med bc og predef, og diagonal til 1.
                Ktot = ApplyBCrow(Ktot, bcNodes);
                Ktot = ApplyBCrow(Ktot, predefNodes);

                //Needs to take the predefs into account
                Vector<double> Rdef = Vector<double>.Build.Dense(sizeOfMatrix);
                if (deftxt.Any()) Rdef = ApplyPreDef(Ktot, predefNodes, predef, sizeOfMatrix);

                //double[] R_array = SetLoads(sizeOfM, loadtxt);
                double[] Rload = AssignLoadsDefAndBC(loadtxt, predefNodes, predef, bcNodes, globalPoints);

                //Adding R-matrix for pre-deformations.
                var V = Vector<double>.Build;
                Vector<double> R = (V.DenseOfArray(Rload)).Subtract(Rdef);

                //Apply boundary condition and predeformations (Puts 0 in columns of K)
                Ktot = ApplyBCcol(Ktot, bcNodes);
                Ktot = ApplyBCcol(Ktot, predefNodes);

                //Removing row and column in K and R, and nodes with removeNodeNr
                if (opt == true)
                {
                    List<Node> nodes_removed = mesh.GetNodeList().ConvertAll(x => x);
                    (Ktot, R, nodes) = UpdateK(removeNodeNr, Ktot, R, nodes_removed);
                }

                //Inverting K matrix. Singular when all elements belonging to a node is removed
                Matrix<double> KtotInverse = Ktot.Inverse();

                //Caluculation of the displacement vector u
                Vector<double> u = KtotInverse.Multiply(R);

                //Creating tree for output of deformation. Structured in x,y,z for each node. As well as asigning deformation to each node class
                defTree = DefToTree(u, nodes);

                //Calculatin strains for each node in elements
                CalcStrainAndStress(elements, material);

                //Calculate global stresses from strain
                CalcStrainAndStressGlobal(nodes, material);
                SetAverageStresses(elements);

                //Find element to be removed next
                if (opt == true)
                {
                    (max, removeElem) = mesh.RemoveOneElement();
                    numberElements = mesh.GetElements().Count;
                }
            }
            DataTree<double> strainTree = new DataTree<double>();
            DataTree<double> stressTree = new DataTree<double>();

            for (int i = 0; i < nodes.Count; i++)
            {
                strainTree.AddRange(nodes[i].GetGlobalStrain(), new GH_Path(new int[] { 0, i }));
                stressTree.AddRange(nodes[i].GetGlobalStress(), new GH_Path(new int[] { 0, i }));
            }

            //FOR PREVIEW OF HEADLINE

            //Setting up reference values
            (double refLength, Point3d centroid ) = GetRefValues(origBrep);

            //Creating text-information for showing in VR
            (string headText, double headSize, Plane headPlane, Color headColor) = CreateHeadline(centroid, refLength);

            //---output---

            DA.SetDataTree(0, defTree);
            DA.SetDataTree(1, strainTree);
            DA.SetDataTree(2, stressTree);
            DA.SetData(3, headText);
            DA.SetData(4, headSize);
            DA.SetData(5, headPlane);
            DA.SetData(6, headColor);

        }

        public void RemoveElementAndUpdateNodes(List<Element> elements, int removeElem, List<int> removeNodeNr)
        {
            List<Node> nodeElem = elements[removeElem].GetVertices();
            int removeElemNr = elements[removeElem].GetElementNr();
            for (int i = 0; i < nodeElem.Count; i++)
            {
                nodeElem[i].GetElementNr().RemoveAll(item => item == removeElemNr);
                if (nodeElem[i].GetElementNr().Count == 0)
                {
                    removeNodeNr.Add(nodeElem[i].GetNodeNr());
                }
            }
            elements.RemoveAt(removeElem);

        }

        public Matrix<double> CreateGlobalStiffnessMatrix(int sizeOfMatrix, Material material, List<Element> elements)
        {
            Matrix<double> Ktot = Matrix<double>.Build.Dense(sizeOfMatrix, sizeOfMatrix);
            double E = material.GetE();
            double nu = material.GetNu();
            StiffnessMatrix sm = new StiffnessMatrix(E, nu);

            for (int i = 0; i < elements.Count; i++)
            {
                List<int> connectedNodes = elements[i].GetConnectivity();
                List<Node> nodes = elements[i].GetVertices();

                (Matrix<double> Ke, List<Matrix<Double>> Be) = sm.CreateMatrix(nodes);

                elements[i].SetStiffnessMatrix(Ke);
                elements[i].SetBMatrices(Be);

                Ktot = AssemblyMatrix(Ktot, Ke, connectedNodes);
            }
            return Ktot;
        }

        public Matrix<double> AssemblyMatrix(Matrix<double> K_tot, Matrix<double> K_e, List<int> connectivity)
        {
            for (int i = 0; i < connectivity.Count; i++)
            {
                for (int j = 0; j < connectivity.Count; j++)
                {
                    //Inserting 3x3 stiffness matrix
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            K_tot[3 * connectivity[i] + k, 3 * connectivity[j] + e] = K_tot[3 * connectivity[i] + k, 3 * connectivity[j] + e] + Math.Round(K_e[3 * i + k, 3 * j + e], 8);
                        }
                    }
                }
            }
            return K_tot;
        }

        public (List<int>, List<double>) CreateBCList(List<string> bctxt, List<Point3d> points)
        {
            List<int> BC = new List<int>();
            List<double> BCPoints = new List<double>();
            List<double> restrains = new List<double>();

            foreach (string s in bctxt)
            {
                string coordinate = (s.Split(';'))[0];
                string iBC = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iBCs = (iBC.Split(','));

                BCPoints.Add(Math.Round(double.Parse(coord[0]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[1]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[2]), 8));

                restrains.Add(Math.Round(double.Parse(iBCs[0]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[1]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[2]), 8));
            }

            int index = 0;

            foreach (Point3d p in points)
            {

                for (int j = 0; j < BCPoints.Count / 3; j++)
                {
                    if (BCPoints[3 * j] == Math.Round(p.X, 8) && BCPoints[3 * j + 1] == Math.Round(p.Y, 8) && BCPoints[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        BC.Add(index);
                        BC.Add(index + 1);
                        BC.Add(index + 2);

                    }
                }
                index += 3;
            }

            return (BC, restrains);
        }

        public Matrix<double> ApplyBCrow(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {

                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[bcNodes[i], j] = 0;
                    }
                    else
                    {
                        K[bcNodes[i], j] = 1;
                    }

                }
            }
            return K;
        }

        public Vector<double> ApplyPreDef(Matrix<double> K_tot, List<int> predefNodes, List<double> predef, int sizeOfM)
        {
            if (predefNodes.Count == 0) return Vector<double>.Build.Dense(sizeOfM);

            //Pick the parts of K that are prescribed a deformation
            Matrix<double> K_red = Matrix<double>.Build.Dense(sizeOfM, predefNodes.Count);
            int n = 0;
            foreach (int dof in predefNodes)
            {
                for (int j = 0; j < sizeOfM; j++)
                {
                    K_red[j, n] = K_tot[j, dof];
                }

                n++;
            }

            //Create a vector of the deformations
            Vector<double> d = Vector<double>.Build.Dense(predefNodes.Count);
            for (int i = 0; i < predefNodes.Count; i++)
            {
                d[i] = predef[i];
            }

            //Multiply this with K_red
            Vector<double> R_def = K_red.Multiply(d);
            for (int i = 0; i < predefNodes.Count; i++)
            {
                R_def[predefNodes[i]] = 0;
            }


            return R_def;
        }

        public double[] AssignLoadsDefAndBC(List<string> pointLoads, List<int> predefNodes, List<double> predef, List<int> bcNodes, List<Point3d> points)
        {
            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();

            double[] R = new double[points.Count * 3];

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                loadCoord.Add(Math.Round(double.Parse(coord[0]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[1]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[2]), 8));

                pointValues.Add(Math.Round(double.Parse(iLoads[0]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[1]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[2]), 8));
            }

            int index = 0;

            foreach (Point3d p in points)
            {

                for (int j = 0; j < loadCoord.Count / 3; j++)
                {
                    if (loadCoord[3 * j] == Math.Round(p.X, 8) && loadCoord[3 * j + 1] == Math.Round(p.Y, 8) && loadCoord[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        R[index] = pointValues[3 * j];
                        R[index + 1] = pointValues[3 * j + 1];
                        R[index + 2] = pointValues[3 * j + 2];
                    }
                }
                index += 3;
            }
            //Corresponding value in R is set to 0 if it is a BC/predef here. Ref page 309 in FEM book.
            foreach (int bc in bcNodes)
            {
                R[bc] = 0;
            }
            for (int i = 0; i < predefNodes.Count; i++)
            {
                R[predefNodes[i]] = predef[i];
            }

            return R;
        }

        public Matrix<double> ApplyBCcol(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {

                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[j, bcNodes[i]] = 0;
                    }

                }
            }
            return K;
        }

        public (Matrix<double>, Vector<double>, List<Node>) UpdateK(List<int> removeNodeNr, Matrix<double> K_tot, Vector<double> R, List<Node> nodes_removed)
        {
            removeNodeNr.Sort();
            removeNodeNr.Reverse();

            for (int i = 0; i < removeNodeNr.Count; i++)
            {
                K_tot = K_tot.RemoveColumn(3 * removeNodeNr[i]);
                K_tot = K_tot.RemoveColumn(3 * removeNodeNr[i]);
                K_tot = K_tot.RemoveColumn(3 * removeNodeNr[i]);
                K_tot = K_tot.RemoveRow(3 * removeNodeNr[i]);
                K_tot = K_tot.RemoveRow(3 * removeNodeNr[i]);
                K_tot = K_tot.RemoveRow(3 * removeNodeNr[i]);

                List<double> R_removed = R.ToList();

                R_removed.RemoveAt(3 * removeNodeNr[i]);
                R_removed.RemoveAt(3 * removeNodeNr[i]);
                R_removed.RemoveAt(3 * removeNodeNr[i]);

                var V = Vector<double>.Build;

                R = (V.DenseOfArray(R_removed.ToArray()));

                nodes_removed.RemoveAt(removeNodeNr[i]);
            }
            return (K_tot, R, nodes_removed);
        }

        public DataTree<double> DefToTree(Vector<double> u, List<Node> nodes)
        {
            DataTree<double> defTree = new DataTree<double>();
            int n = 0;
            for (int i = 0; i < u.Count; i += 3)
            {
                List<double> u_node = new List<double>(3);
                u_node.Add(u[i]);
                u_node.Add(u[i + 1]);
                u_node.Add(u[i + 2]);
                //sett til riktig node, ta hensyn til de vi har fjernet.
                nodes[i / 3].SetDeformation(u_node);

                defTree.AddRange(u_node, new GH_Path(new int[] { 0, n }));
                n++;
            }

            return defTree;
        }

        public void CalcStrainAndStress(List<Element> elements, Material material)
        {
            double E = material.GetE();
            double nu = material.GetNu();
            Cmatrix C = new Cmatrix(E, nu);
            Matrix<double> C_matrix = C.CreateMatrix();

            List<Matrix<double>> B_e = new List<Matrix<double>>();
            List<int> c_e = new List<int>();
            List<Node> nodes_e = new List<Node>();

            for (int i = 0; i < elements.Count; i++)
            {
                B_e = elements[i].GetBMatrices();
                nodes_e = elements[i].GetVertices();

                Calculations(B_e, nodes_e, C_matrix);
            }

        }

        public void Calculations(List<Matrix<double>> B_e, List<Node> nodes_e, Matrix<double> C_Matrix)
        {
            List<Vector<double>> elementStrain = new List<Vector<double>>();
            List<Vector<double>> elementStress = new List<Vector<double>>();
            Vector<double> u_e = Vector<double>.Build.Dense(24);

            double g = Math.Sqrt(3);
            List<List<double>> gaussPoints = new List<List<double>>()
            {
                new List<double>() { -g, -g, -g },
                new List<double>() { g, -g, -g },
                new List<double>() { g, g, -g },
                new List<double>() { -g, g, -g },
                new List<double>() { -g, -g, g },
                new List<double>() { g, -g, g },
                new List<double>() { g, g, g },
                new List<double>() { -g, g, g },
            };

            for (int i = 0; i < nodes_e.Count; i++)
            {
                List<double> deformations = nodes_e[i].GetDeformation();
                u_e[i * 3] = deformations[0];
                u_e[i * 3 + 1] = deformations[1];
                u_e[i * 3 + 2] = deformations[2];
            }

            for (int j = 0; j < B_e.Count; j++)
            {
                Vector<double> nodeStrain = B_e[j].Multiply(u_e); /// IN GAUSS POINTS
                Vector<double> nodeStress = C_Matrix.Multiply(nodeStrain); /// IN GAUSS POINTS
                elementStrain.Add(nodeStrain);
                elementStress.Add(nodeStress);
            }


            for (int i = 0; i < elementStress.Count; i++)
            {
                Vector<double> intStress = InterpolateStress(elementStress, gaussPoints[i]);

                nodes_e[i].SetStress(intStress); //INTERPOLATED TO NODES
            }
        }

        public Vector<double> InterpolateStress(List<Vector<double>> gaussStress, List<double> gaussPoints)
        {
            double r = gaussPoints[0];
            double s = gaussPoints[1];
            double t = gaussPoints[2];

            List<double> shapeF = new List<double> {
            (double)1 / 8 * ((1 - r) * (1 - s) * (1 - t)),
            (double)1 / 8 * ((1 + r) * (1 - s) * (1 - t)),
            (double)1 / 8 * ((1 + r) * (1 + s) * (1 - t)),
            (double)1 / 8 * ((1 - r) * (1 + s) * (1 - t)),
            (double)1 / 8 * ((1 - r) * (1 - s) * (1 + t)),
            (double)1 / 8 * ((1 + r) * (1 - s) * (1 + t)),
            (double)1 / 8 * ((1 + r) * (1 + s) * (1 + t)),
            (double)1 / 8 * ((1 - r) * (1 + s) * (1 + t)),
            };

            Vector<double> nodeStress = Vector<double>.Build.Dense(6);

            for (int i = 0; i < nodeStress.Count; i++)
            {
                for (int j = 0; j < shapeF.Count; j++)
                {
                    nodeStress[i] += gaussStress[j][i] * shapeF[j];
                }

            }
            return nodeStress;
        }


        public void CalcStrainAndStressGlobal(List<Node> nodes, Material material)
        {
            double E = material.GetE();
            double nu = material.GetNu();
            Cmatrix C = new Cmatrix(E, nu);
            Matrix<double> C_matrix = C.CreateMatrix();

            for (int i = 0; i < nodes.Count; i++)
            {
                List<Vector<double>> stress = nodes[i].GetStress();

                double amount = stress.Count;
                Vector<double> globalStress = Vector<double>.Build.Dense(6);
                for (int k = 0; k < stress.Count; k++)
                {
                    globalStress[0] += stress[k][0];
                    globalStress[1] += stress[k][1];
                    globalStress[2] += stress[k][2];
                    globalStress[3] += stress[k][3];
                    globalStress[4] += stress[k][4];
                    globalStress[5] += stress[k][5];
                }

                for (int j = 0; j < globalStress.Count; j++)
                {
                    globalStress[j] = (double)globalStress[j] / amount;
                }

                nodes[i].SetGlobalStrain(C_matrix.Inverse().Multiply(globalStress));

                //Adding Mises
                var tempStressVec = Vector<double>.Build.Dense(globalStress.Count + 1);
                globalStress.Storage.CopySubVectorTo(tempStressVec.Storage, 0, 0, 6);
                double Sxx = globalStress[0];
                double Syy = globalStress[1];
                double Szz = globalStress[2];
                double Sxy = globalStress[3];
                double Sxz = globalStress[4];
                double Syz = globalStress[5];
                double mises = Math.Sqrt(0.5 * (Math.Pow(Sxx - Syy, 2) + Math.Pow(Syy - Szz, 2) + Math.Pow(Szz - Sxx, 2)) + 3 * (Math.Pow(Sxy, 2) + Math.Pow(Sxz, 2) + Math.Pow(Syz, 2)));
                tempStressVec.At(6, mises);

                nodes[i].SetGlobalStress(tempStressVec);
            }
        }

        public void SetAverageStresses(List<Element> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetAverageValuesStress();
            }
        }


        public int FindRemovableElements(List<Node> nodes, List<Element> elements)
        {
            int count = 0;
            for (int i = 0; i < elements.Count; i++)
            {
                List<Node> checkNodes = elements[i].GetVertices();
                for (int j = 0; j < checkNodes.Count; j++)
                {
                    if (!(checkNodes[j].IsRemovable()))
                    {
                        elements[i].SetRemovable(false);
                    }
                }
                if (elements[i].IsRemovable()) { count++; }
            }
            return count;
        }

        public (double, Point3d) GetRefValues(Brep origBrep)
        {
            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);
            Point3d centroid = vmp.Centroid;
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            return (refLength, centroid);
        }

        public (string, double, Plane, Color) CreateHeadline(Point3d centroid, double refLength)
        {
            string headText = "Model";

            double headSize = (double)refLength / 1.5;

            Point3d p0 = centroid;
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane headPlane = new Plane(p0, p1, p2);
            headPlane.Translate(new Vector3d(0, -headSize, 3.5 * refLength));

            Color headColor = Color.FromArgb(0, 100, 255);

            return (headText, headSize, headPlane, headColor);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.analyze;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f0cb3c4f-cdc0-4e69-b554-84d9e3e112f4"); }
        }
    }
}