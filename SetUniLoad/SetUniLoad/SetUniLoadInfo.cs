using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SetUniLoad
{
    public class SetUniLoadInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SetUniLoad";
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
                return new Guid("c6046233-cc20-4cf2-ab4c-13af6a48f06a");
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
