using Microsoft.VisualStudio.TestTools.UnitTesting;
using simcityModel.Model;
using Moq;
using simcityPersistance.Persistance;

namespace simcityTest
{
    [TestClass]
    public class SimCityTest
    {
        private SimCityModel _model = null!;
        private Field[,] _mockedField = null!;
        private Mock<Field[,]> _mock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _model = new SimCityModel(new FileDataAccess());
        }

        [TestMethod]
        public void AdvanceTimeTest()
        {
            DateTime currentGameTime = _model.GameTime;
            _model.AdvanceTime();
            currentGameTime = currentGameTime.AddDays(1);
            Assert.AreEqual(currentGameTime, _model.GameTime);
        }

        #region MakeZone tests

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOutOfFieldTest1()
        {
            _model.MakeZone(-1, _model.GameSize / 2, FieldType.GeneralField);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOutOfFieldTest2()
        {
            _model.MakeZone(_model.GameSize, _model.GameSize / 2, FieldType.GeneralField);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOutOfFieldTest3()
        {
            _model.MakeZone(_model.GameSize / 2, -1, FieldType.GeneralField);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOutOfFieldTest4()
        {
            _model.MakeZone(_model.GameSize / 2, _model.GameSize, FieldType.GeneralField);
        }

        [TestMethod]
        public void MakeZoneOnGeneralFieldTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            Assert.AreEqual(FieldType.ResidentalZone, _model.Fields[0, 0].Type);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOnZoneFieldTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeZone(0, 0, FieldType.OfficeZone);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeZoneOnFieldWithBuildingTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
        }

        #endregion

        #region MakeBuilding tests

        [TestMethod]
        public void BuildRoadThatWouldKeepTheRoadNetworkConnectedTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            Assert.IsNotNull(_model.Fields[0, 0].Building);
            Assert.AreEqual(BuildingType.Road, _model.Fields[0, 0].Building!.Type);

            _model.MakeBuilding(0, 1, BuildingType.Road);
            Assert.IsNotNull(_model.Fields[0, 1].Building);
            Assert.AreEqual(BuildingType.Road, _model.Fields[0, 1].Building!.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void BuildRoadThatWouldMakeTheRoadNetworkUnconnectedTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 2, BuildingType.Road);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOutOfFieldTest1()
        {
            _model.MakeBuilding(-1, _model.GameSize / 2, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOutOfFieldTest2()
        {
            _model.MakeBuilding(_model.GameSize, _model.GameSize / 2, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOutOfFieldTest3()
        {
            _model.MakeBuilding(_model.GameSize / 2, -1, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOfFieldTest4()
        {
            _model.MakeBuilding(_model.GameSize / 2, _model.GameSize, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingThatWouldNotBeAdjacentWithAnyRoadTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingThatWouldExtendBeyondTheFieldsTest()
        {
            _model.MakeBuilding(_model.GameSize - 2, _model.GameSize - 1, BuildingType.Road);
            _model.MakeBuilding(_model.GameSize - 1, _model.GameSize - 1, BuildingType.Stadium);
        }

        [TestMethod]
        public void MakeBuildingOnGeneralFieldTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 1, BuildingType.PoliceStation);
            Assert.IsNotNull(_model.Fields[0, 1].Building);
            Assert.AreEqual(BuildingType.PoliceStation, _model.Fields[0, 1].Building!.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOnZoneFieldTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 0, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingOnAnotherBuildingTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 0, BuildingType.PoliceStation);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotBuildException))]
        public void MakeBuildingThatWouldExtendToAnotherBuilding()
        {
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(1, 1, BuildingType.Stadium);
            _model.MakeBuilding(0, 0, BuildingType.Stadium);
        }

        #endregion

        #region Destroy tests

        [TestMethod]
        public void DestroyZoneWithNoBuildingTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.Destroy(0, 0);
            Assert.AreEqual(FieldType.GeneralField, _model.Fields[0, 0].Type);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotDestroyException))]
        public void DestroyBuildingWithPeopleInsideTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeZone(0, 2, FieldType.OfficeZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            for (int i = 0; i < 5; i++) _model.AdvanceTime();

            _model.Destroy(0, 0);
        }

        [TestMethod]
        public void DestroyBuildingWithoutPeopleInsideTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeZone(0, 2, FieldType.OfficeZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            for (int i = 0; i < 5; i++) _model.AdvanceTime();
            ((PeopleBuilding)_model.Fields[0, 0].Building!).People.Clear();

            _model.Destroy(0, 0);
            Assert.IsNull(_model.Fields[0, 0].Building);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotDestroyException))]
        public void DestroyRoadThatWouldMakeTheRoadNetworkUnconnectedTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(0, 2, BuildingType.Road);
            _model.Destroy(0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(CannotDestroyException))]
        public void DestroyRoadThatWouldMakeABuildingInaccessibleTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 1, BuildingType.PoliceStation);
            _model.Destroy(0, 0);
        }

        [TestMethod]
        public void DestroyServiceBuildingTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 1, BuildingType.FireStation);
            _model.MakeBuilding(1, 0, BuildingType.Stadium);

            _model.Destroy(0, 1);
            Assert.IsNull(_model.Fields[0, 1].Building);

            List<(int x, int y)> buildingCoords = _model.Fields[1, 0].Building!.Coordinates;

            _model.Destroy(1, 0);
            foreach((int x, int y) coords in buildingCoords)
            {
                Assert.IsNull(_model.Fields[1, 0].Building);
            }
        }

        #endregion
    }
}