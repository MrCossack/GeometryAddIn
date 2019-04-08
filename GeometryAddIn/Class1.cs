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

namespace GeometryAddIn
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

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
                CreateSphereDirectShape(doc);
            } 
            else if (TaskDialogResult.CommandLink2 == tResult)
            {
                CreateTessellatedShape(doc, new ElementId(BuiltInCategory.OST_Materials));
            }

            return Result.Succeeded;
        }

        // Create a DirectShape Sphere
        public void CreateSphereDirectShape(Document doc)
        {
            List<Curve> profile = new List<Curve>();

            // first create sphere with 2' radius
            XYZ center = XYZ.Zero;
            double radius = 2.0;
            XYZ profile00 = center;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            CurveLoop curveLoop = CurveLoop.Create(profile);
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == true)
            {
                Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                {
                    t.Start();
                    // create direct shape and assign the sphere shape
                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(new GeometryObject[] { sphere });
                    t.Commit();
                }
            }
        }

        // Create a pyramid-shaped DirectShape using given material for the faces
        public void CreateTessellatedShape(Document doc, ElementId materialId)
        {
            List<XYZ> loopVertices = new List<XYZ>(4);

            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            builder.OpenConnectedFaceSet(true);
            // create a pyramid with a square base 4' x 4' and 5' high
            double length = 4.0;
            double height = 5.0;

            XYZ basePt1 = XYZ.Zero;
            XYZ basePt2 = new XYZ(length, 0, 0);
            XYZ basePt3 = new XYZ(length, length, 0);
            XYZ basePt4 = new XYZ(0, length, 0);
            XYZ apex = new XYZ(length / 2, length / 2, height);

            loopVertices.Add(basePt1);
            loopVertices.Add(basePt2);
            loopVertices.Add(basePt3);
            loopVertices.Add(basePt4);
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            loopVertices.Clear();
            loopVertices.Add(basePt1);
            loopVertices.Add(apex);
            loopVertices.Add(basePt2);
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            loopVertices.Clear();
            loopVertices.Add(basePt2);
            loopVertices.Add(apex);
            loopVertices.Add(basePt3);
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            loopVertices.Clear();
            loopVertices.Add(basePt3);
            loopVertices.Add(apex);
            loopVertices.Add(basePt4);
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            loopVertices.Clear();
            loopVertices.Add(basePt4);
            loopVertices.Add(apex);
            loopVertices.Add(basePt1);
            builder.AddFace(new TessellatedFace(loopVertices, materialId));

            builder.CloseConnectedFaceSet();
            builder.Target = TessellatedShapeBuilderTarget.Solid;
            builder.Fallback = TessellatedShapeBuilderFallback.Abort;
            builder.Build();

            TessellatedShapeBuilderResult result = builder.GetBuildResult();

            using (Transaction t = new Transaction(doc, "Create tessellated direct shape"))
            {
                t.Start();

                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";

                ds.SetShape(result.GetGeometricalObjects());
                t.Commit();
            }
        }
    }
}
