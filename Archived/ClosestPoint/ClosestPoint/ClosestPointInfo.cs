﻿using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ClosestPoint
{
    public class ClosestPointInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ClosestPoint";
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
                return new Guid("eab1cca2-a868-45e1-8a0b-270367da7032");
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
