using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PreDef
{
    public class PreDefInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PreDef";
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
                return new Guid("ae3ce8aa-6905-49ad-98e3-a7c91f8f4636");
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
