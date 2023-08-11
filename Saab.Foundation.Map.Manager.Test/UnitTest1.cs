using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;
using GizmoSDK.Coordinate;


namespace Saab.Foundation.Map.Test
{
    [TestClass]
    [Ignore] // Right now when we dont have a map purl
    public class TestLoadMap
    {
        static Node _node;

        [AssemblyInitialize]
        public static void TestInit(TestContext tc)
        {
            GizmoSDK.Gizmo3D.Platform.Initialize();

            DbManager.Initialize();

            Message.SetMessageLevel(MessageLevel.DEBUG);

            _node = DbManager.LoadDB("purl:maps,sweden/eksjo/data/map.gzd");

            Assert.IsNotNull(_node);
            Assert.IsTrue(_node.IsValid());
        }

        [AssemblyCleanup]
        public static void TestCleanup()
        {
            GizmoSDK.Gizmo3D.Platform.Uninitialize();
        }

        [TestMethod]
        public void TestRotations()
        {
            Matrix3 mat = Matrix3.CreateFrom_Euler_YXZ(0.5f, 0, 0);

            Vec3 pos = mat * new Vec3(0, 0, 1);

            Quaternion quat = Quaternion.CreateFrom_Euler_YXZ(0.5f,0,0);

        }

        [TestMethod]
        public void TestUTMMap()
        {
            MapControl controller = new MapControl
            {
                CurrentMap = _node
            };

            Assert.IsTrue(controller.MapType == MapType.UTM);
        }

        [TestMethod]
        public void TestGroundClamp()
        {

            MapControl controller = new MapControl
            {
                CurrentMap = _node,
            };

            Assert.IsTrue(controller.GetPosition(new LatPos(1.0084718541, 0.24984267815, 0), out MapPos pos,GroundClampType.GROUND,ClampFlags.WAIT_FOR_DATA));

            controller.LocalToWorld(pos, out LatPos latpos);
        }

        private static double getElevation(LatPos position)
        {
            var adapter = SerializeAdapter.GetURLAdapter("pipe:Elevation?retry=2");

            if (!adapter.IsValid())
            {
                return position.Altitude;
            }

            DynamicType dynlat = position.Latitude;
            DynamicType dynlon = position.Longitude;

            adapter.Write(2537);
            adapter.Write(dynlat.GetDataSize() + dynlon.GetDataSize());
            adapter.Write(dynlat);
            adapter.Write(dynlon);

            DynamicType res = new DynamicType();

            while (!adapter.HasData())
            {
                System.Threading.Thread.Sleep(10);
                // todo, quit after some time...
            }

            adapter.Read(res);

            return res.GetNumber();
        }


        [TestMethod]
        public void TestAltitude()
        {

            MapControl controller = new MapControl
            {
                CurrentMap = _node,
                //RoiPosition = new GizmoSDK.GizmoBase.Vec3D(0, 0, 0);
            };

            double altitude=controller.GetAltitude(new LatPos(1.0084718541, 0.24984267815, 0),ClampFlags.WAIT_FOR_DATA|ClampFlags.ISECT_LOD_QUALITY);

            double compareAltitude = getElevation(new LatPos(1.0084718541, 0.24984267815, 0));

            double renaAltitude = getElevation(new LatPos(61.147953* Coordinate.DEG2RAD, 11.392666*Coordinate.DEG2RAD, 0));
        }

        [TestMethod]
        public void TestCamera()
        {
            
            Camera cam = new PerspCamera();

            cam.Position = new Vec3D(100, 100, 100);
            cam.RoiPosition = true;

            MapControl controller = new MapControl
            {
                CurrentMap = _node,
                Camera = cam
            };

            controller.GetScreenVectors(10, 10, 100, 100, out Vec3D position, out Vec3 direction);
        }

        [TestMethod]
        public void TestFrustrumGroundclamp()
        {
           
            Camera cam = new PerspCamera();

            cam.Position = new Vec3D(100, 100, 100);
            cam.RoiPosition = true;

            MapControl controller = new MapControl
            {
                CurrentMap = _node,
                Camera = cam
            };

            controller.GetScreenVectors(10, 10, 100, 100, out Vec3D position, out Vec3 direction);

            MapPos pos=controller.GlobalToLocal(position);

            pos.local_orientation = new Matrix3(new Vec3(1, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0));

            controller.UpdatePosition(pos, GroundClampType.GROUND, ClampFlags.DEFAULT|ClampFlags.WAIT_FOR_DATA);

           // Assert.IsTrue(controller.GetPosition(new LatPos(1.0084718541, 0.24984267815), out MapPos pos, GroundClampType.GROUND, ClampFlags.WAIT_FOR_DATA));
        }
        

    }
}
