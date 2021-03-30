using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;


namespace Geometry
{
    class Geometry
    {
        // Create a DirectShape Sphere
        static public void CreateSphereDirectShape(Document doc)
        {
            List<Curve> profile = new List<Curve>();

            // first create sphere with 2' radius
            XYZ center = XYZ.Zero;
            double radius = 2.0;
            //XYZ profile00 = center;
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
        static public void CreateTessellatedShape(Document doc, ElementId materialId)
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
