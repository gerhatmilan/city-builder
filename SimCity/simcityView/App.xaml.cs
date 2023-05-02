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
using Microsoft.Win32;

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
        private DispatcherTimer _timer = null!;
        #endregion


        #region constructor, appstart
        public App()
        {
            Startup += new StartupEventHandler(AppStart);
        }

        public void AppStart(object? s, StartupEventArgs e)
        {
            Init();
        }
        #endregion
        #region Private functions
        private void Init()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;


            _model = new SimCityModel(new FileDataAccess());
            _model.GameOver += M_GameOver;

            _vm = new SimCityViewModel(_model);
            
            _vm.ChangeTime += Vm_ChangeTimer;
            _vm.SaveGameEvent += Vm_SaveGame;
            _vm.LoadGameEvent += Vm_LoadGame;
            _vm.NewGameEvent += Vm_NewGame;
            
            _view = new MainWindow();
            _view.DataContext = _vm;
            _view.CamInit();
            _view.Show();
           

            _model.InitializeGame();
        }

        private void reset()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;


            _model = new SimCityModel(new FileDataAccess());
            
            _model.GameOver += M_GameOver;

            _vm = new SimCityViewModel(_model);

            _vm.ChangeTime += Vm_ChangeTimer;
            _vm.SaveGameEvent += Vm_SaveGame;
            _vm.LoadGameEvent += Vm_LoadGame;
            _vm.NewGameEvent += Vm_NewGame;

            
            _view.DataContext = _vm;
            _view.CamInit();

            _model.InitializeGame();
            GC.Collect();
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
        private void Vm_NewGame(object? s, EventArgs e)
        {
            reset();
        }
        private async void Vm_SaveGame(object? s, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog(); // dialógablak
                saveFileDialog.Title = "SimCity tábla betöltése";
                saveFileDialog.Filter = "SimCity tábla|*.sc";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        //await _model.Save(saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Valami félrement mentés közben!", "SimCity", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("A fájl mentése sikertelen!", "SimCity", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void Vm_LoadGame(object? s, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "SimCity tábla betöltése";
                openFileDialog.Filter = "SimCity tábla|*.sc";
                if (openFileDialog.ShowDialog() == true)
                {
                    //await _model.Load(openFileDialog.FileName);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A fájl betöltése sikertelen!", "SimCity", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
        #region Model events
        void M_GameOver(object? s, EventArgs e)
        {
            _vm.GameIsNotOver = false;
            _vm.TimeSet.Execute("0");
            MessageBox.Show("☠ Vége a játéknak! ☠", "SimCity", MessageBoxButton.OK);
        }
        #endregion
        #endregion
    }
}
