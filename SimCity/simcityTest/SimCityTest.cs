using Microsoft.VisualStudio.TestTools.UnitTesting;
using simcityModel.Model;
using simcityPersistance.Persistance;

namespace simcityTest
{
    [TestClass]
    public class SimCityTest
    {
        [TestMethod]
        public void InitializeGameTest()
        {
            SimCityModel model = new SimCityModel(new FileDataAccess());
            model.InitializeGame();

            for (int i = 0; i < model.GameSize; i++)
            {
                for (int j = 0; j < model.GameSize; j++)
                {
                    Assert.IsTrue(model.Fields[i, j].Type == FieldType.GeneralField);
                    Assert.IsTrue(model.Fields[i, j].Building == null);
                }
            }

            Assert.AreEqual(0, model.Buildings.Count);
            Assert.AreEqual(0, model.People.Count);
            Assert.AreEqual(0, model.IncomeList.Count);
            Assert.AreEqual(0, model.ExpenseList.Count);
            Assert.AreEqual(0, model.Population);
            Assert.AreEqual(3000, model.Money);
            Assert.AreEqual(DateTime.Now.Year, model.GameTime.Year);
            Assert.AreEqual(DateTime.Now.Month, model.GameTime.Month);
            Assert.AreEqual(DateTime.Now.Day, model.GameTime.Day);
        }
    }
}