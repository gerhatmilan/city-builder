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
using System.Timers;

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
        private DispatcherTimer _timer;
        #endregion


        #region constructor, appstart
        public App()
        {
            Startup += new StartupEventHandler(AppStart);
        }

        public void AppStart(object? s, StartupEventArgs e)
        {
            /* only for testing purposes for now */
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            

            _model = new SimCityModel(new FileDataAccess());
            _vm = new SimCityViewModel(_model);
            _vm.ChangeTime += Vm_ChangeTimer;
            _view = new MainWindow();
            _view.DataContext = _vm;
            _view.CamInit();
            _view.Show();

            _model.InitializeGame();

            

        }
        #endregion
        #region events
        #region Timer events
        void Timer_Tick(object? s, EventArgs e)
        {
            _model.AdvanceTime();
        }
        #endregion
        #region View events
        void Vm_ChangeTimer(object? s, int status)
        {
            switch (status)
            {
                case 0: _timer.Stop(); break;
                default:
                    if (!_timer.IsEnabled)
                    {
                        _timer.Start();
                    }
                    _timer.Interval = TimeSpan.FromMilliseconds(status);
                    
                    break;
            }
        }
        #endregion
        #endregion
    }
}
