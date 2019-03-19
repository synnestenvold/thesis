using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SetUniBC
{
    public class SetUniBCInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SetUniBC";
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
                return new Guid("a830e5bf-0a89-4fb3-99a9-91faddf11378");
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
