using simcityView.ViewModel;
using simcityPersistance.Persistance;
using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace simcityView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region variables

        private SimCityViewModel _vm = null!;
        private MainWindow _view = null!;
        private SimCityModel _model = null!;

        #endregion


        #region constructor, appstart
        public App()
        {
            Startup += new StartupEventHandler(AppStart);
        }

        public void AppStart(object? s, StartupEventArgs e)
        {
            _model = new SimCityModel(new SimCityPersistance());

            _vm = new SimCityViewModel(_model);

            _view = new MainWindow();
            _view.DataContext = _vm;
            _view.Show();



        }
        #endregion
    }
}
