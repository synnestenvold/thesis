using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace StressDirectionSlider
{
    public class StressDirectionSliderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "StressDirectionSlider";
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
                return new Guid("3c174a40-579c-491e-9fb0-a044c34b2b96");
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
