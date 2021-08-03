using Autodesk.Revit.UI;
using NodeTools.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools
{
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            NodeRibbon.AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
