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
using System.Windows.Threading;

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
            /* only for testing purposes for now */
            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            _model = new SimCityModel(new FileDataAccess());
            _vm = new SimCityViewModel(_model);
            _view = new MainWindow();
            _view.DataContext = _vm;
            _view.CamInit();
            _view.Show();

            _model.InitializeGame();

            void Timer_Tick(object? s, EventArgs e)
            {
                _model.AdvanceTime();
            }

        }
        #endregion
    }
}
