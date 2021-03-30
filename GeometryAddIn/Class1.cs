using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Architecture;

using Geometry;

namespace GeometryAddIn
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class Class1 : IExternalCommand
    {
        enum Selection
        {
            Sphere,
            Pyramid,
            dummy,
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            if (inputeDialog() == Selection.Sphere)
            {
                Geometry.Geometry.CreateSphereDirectShape(doc);
            } 
            else if (inputeDialog() == Selection.Pyramid)
            {
                Geometry.Geometry.CreateTessellatedShape(doc, new ElementId(BuiltInCategory.OST_Materials));
            }

            return Result.Succeeded;
        }

        private Selection inputeDialog()
        {
            // An option window for the user
            TaskDialog taskWindow = new TaskDialog("Choose a geometry: ");
            taskWindow.MainContent = "Which geometry would you like to draw?";

            // Add commmandLink options to task dialog
            taskWindow.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Sphere");
            taskWindow.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Pyramid");

            // Set common buttons and default button. If no CommonButton or CommandLink is added,
            // task dialog will show a Close button by default
            taskWindow.CommonButtons = TaskDialogCommonButtons.Close;
            taskWindow.DefaultButton = TaskDialogResult.Close;

            TaskDialogResult tResult = taskWindow.Show();

            if (TaskDialogResult.CommandLink1 == tResult)
            {
                return Selection.Sphere;
            } else if(TaskDialogResult.CommandLink2 == tResult)
            {
                return Selection.Pyramid;
            } else
            {
                return Selection.dummy;
            }
        }
    }
}
