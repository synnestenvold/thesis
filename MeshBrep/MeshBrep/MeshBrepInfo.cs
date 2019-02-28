using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace MeshBrep
{
    public class MeshBrepInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "MeshBrep";
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
                return new Guid("12fdba69-c88d-404f-9c79-07818a41c14b");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
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
