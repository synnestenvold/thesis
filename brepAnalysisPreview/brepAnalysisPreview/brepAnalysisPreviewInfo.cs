using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace brepAnalysisPreview
{
    public class brepAnalysisPreviewInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "brepAnalysisPreview";
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
                return new Guid("df62246f-bb50-4269-91c4-1adfa34ea120");
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
