using Microsoft.VisualStudio.TestTools.UnitTesting;
using simcityModel.Model;
using Moq;
using simcityPersistance.Persistance;
using System.Linq;

namespace simcityTest
{
    [TestClass]
    public class SimCityTest
    {
        private SimCityModel _model = null!;
        private Mock<IDataAccess> _mock = new Mock<IDataAccess>();


        [TestInitialize]
        public void Initialize()
        {
            _model = new SimCityModel(_mock.Object);
        }

        #region AdvanceTime test

        [TestMethod]
        public void AdvanceTimeTest()
        {
            DateTime currentGameTime = _model.GameTime;
            _model.AdvanceTime();
            currentGameTime = currentGameTime.AddDays(1);
            Assert.AreEqual(currentGameTime, _model.GameTime);
        }

        #endregion

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
        public void DestroyRoadThatWouldKeepTheRoadNetworkConnectedTest()
        {
            _model.MakeBuilding(0, 0, BuildingType.Road);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.Destroy(0, 1);
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

        #region Service building effect tests

        [TestMethod]
        public void StadionEffectTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeZone(0, 2, FieldType.OfficeZone);
            _model.AdvanceTime();

            double oldHappiness = _model.People[0].Happiness;

            _model.MakeBuilding(1, 0, BuildingType.Stadium);

            _model.AdvanceTime();

            Assert.IsTrue(_model.People[0].Happiness > oldHappiness);
        }

        [TestMethod]
        public void PoliceStationEffectTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeZone(0, 2, FieldType.OfficeZone);
            _model.AdvanceTime();

            double oldHappiness = _model.People[0].Happiness;

            _model.MakeBuilding(1, 1, BuildingType.PoliceStation);

            _model.AdvanceTime();

            Assert.IsTrue(_model.People[0].Happiness > oldHappiness);
        }

        [TestMethod]
        public void FireStationEffectTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeZone(0, 2, FieldType.OfficeZone);
            _model.AdvanceTime();

            double oldFireProb = _model.Fields[0, 0].Building!.FireProbability;

            _model.MakeBuilding(1, 1, BuildingType.FireStation);

            _model.AdvanceTime();

            Assert.IsTrue(_model.Fields[0, 0].Building!.FireProbability < oldFireProb);
        }

        #endregion

        #region SendFireTruck tests

        [TestMethod]
        [ExpectedException(typeof(CannotSendFireTruckException))]
        public void FireTruckDestinationOutOfFieldTest()
        {
            _model.SendFireTruck((-1, -1));
        }

        [TestMethod]
        [ExpectedException(typeof(CannotSendFireTruckException))]
        public void FireTruckDestinationNoBuildingTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.SendFireTruck((0, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(CannotSendFireTruckException))]
        public void FireTruckDestinationBuildingNotOnFireTest()
        {
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(0, 2, BuildingType.PoliceStation);
            _model.SendFireTruck((0, 2));
        }

        [TestMethod]
        [ExpectedException(typeof(CannotSendFireTruckException))]
        public void CannotSendFireTruckWhenZeroFireStationsTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeZone(0, 2, FieldType.OfficeZone);

            _model.AdvanceTime();
            while (!_model.Buildings.Any(building => building.OnFire)) _model.AdvanceTime();
            foreach (Building b in _model.Buildings)
            {
                if (b.OnFire)
                {
                    _model.SendFireTruck(b.TopLeftCoordinate);
                    break;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CannotSendFireTruckException))]
        public void CannotSendFireTruckWhenNoFireStationAvailableTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(1, 1, BuildingType.Road);
            _model.MakeBuilding(0, 2, BuildingType.FireStation);
            _model.MakeZone(1, 0, FieldType.OfficeZone);

            _model.AdvanceTime();

            IEnumerable<Building> filteredList = _model.Buildings.Where(building => building.Type != BuildingType.Road && building.Type != BuildingType.FireStation);
            while (!filteredList.All(building => building.OnFire)) _model.AdvanceTime();

            _model.SendFireTruck((0, 0));
            _model.SendFireTruck((0, 2));
        }

        [TestMethod]
        public void FireTruckSpawnsAfterUserInteractionTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(1, 1, BuildingType.Road);
            _model.MakeBuilding(2, 1, BuildingType.FireStation);
            _model.MakeZone(0, 2, FieldType.OfficeZone);

            _model.AdvanceTime();
            while (!_model.Buildings.Any(building => building.OnFire)) _model.AdvanceTime();
            foreach (Building b in _model.Buildings)
            {
                if (b.OnFire)
                {
                    _model.SendFireTruck(b.TopLeftCoordinate);
                    _model.AdvanceTime();

                    Assert.AreEqual(VehicleType.Firecar, ((Road)_model.Fields[1, 1].Building!).Vehicles[0].Type);
                    break;
                }
            }
        }

        [TestMethod]
        public void BuildingIsNotOnFireAfterFireTruckArrivesTest()
        {
            _model.MakeZone(0, 0, FieldType.ResidentalZone);
            _model.MakeBuilding(0, 1, BuildingType.Road);
            _model.MakeBuilding(1, 1, BuildingType.Road);
            _model.MakeBuilding(2, 1, BuildingType.FireStation);
            _model.MakeZone(0, 2, FieldType.OfficeZone);

            _model.AdvanceTime();
            while (!_model.Buildings.Any(building => building.OnFire)) _model.AdvanceTime();

            Building building = null;

            foreach (Building b in _model.Buildings)
            {
                if (b.OnFire)
                {
                    building = b;
                    _model.SendFireTruck(b.TopLeftCoordinate);              
                    break;
                }
            }

            _model.AdvanceTime();
            while (_model.Vehicles.Count > 0) _model.MoveVehicles(this, EventArgs.Empty);

            Assert.IsFalse(building!.OnFire);
        }

        #endregion
    }
}