using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FEMeshTBrep
{
    public class FEMeshTBrepInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FEMeshTBrep";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("c9c932ae-0637-487e-b1cc-389281dd9f12");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "HP Inc.";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
