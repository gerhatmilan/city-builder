﻿using simcityView.ViewModel;
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
using System.Diagnostics;

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
        private DispatcherTimer _vehicleTimer = null!;
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

            _vehicleTimer = new DispatcherTimer();
            _vehicleTimer.Interval = TimeSpan.FromMilliseconds(400);
            _vehicleTimer.Tick += VehicleTimer_Tick;

            _model = new SimCityModel(new FileDataAccess());
            _model.GameOver += M_GameOver;
            _model.GameLoaded += M_GameLoaded;

            _vm = new SimCityViewModel(_model);
            
            _vm.ChangeTime += Vm_ChangeTimer;
            _vm.SaveGameEvent += Vm_SaveGame;
            _vm.LoadGameEvent += Vm_LoadGame;
            _vm.NewGameEvent += Vm_NewGame;
            _vm.ShowHelpEvent += Vm_ShowHelp;

            _view = new MainWindow();
            _view.Activated += new EventHandler(View_FocusChanged);
            _view.Deactivated += new EventHandler(View_FocusChanged);
            _view.DataContext = _vm;
            _view.CamInit();
            _view.Show();
        }

        private void reset()
        {

            _timer.Stop();         


            _model.GameOver -= M_GameOver;
            _vm.ChangeTime -= Vm_ChangeTimer;
            _vm.SaveGameEvent -= Vm_SaveGame;
            _vm.LoadGameEvent -= Vm_LoadGame;
            _vm.NewGameEvent -= Vm_NewGame;


            _model = new SimCityModel(new FileDataAccess());
            _vm = new SimCityViewModel(_model);
            
            _model.GameOver += M_GameOver;
            _model.GameLoaded += M_GameLoaded;
            _vm.ChangeTime += Vm_ChangeTimer;
            _vm.SaveGameEvent += Vm_SaveGame;
            _vm.LoadGameEvent += Vm_LoadGame;
            _vm.NewGameEvent += Vm_NewGame;
            _vm.ShowHelpEvent += Vm_ShowHelp;


            _view.DataContext = _vm;
            _view.CamInit();
            

            GC.Collect();
            
        }
       
        #endregion
        #region events
        #region Timer events
        void Timer_Tick(object? s, EventArgs e)
        {
            _model.AdvanceTime();
        }
        void VehicleTimer_Tick(object? s, EventArgs e)
        {
            _model.MoveVehicles(this, e);
        }
        #endregion
        #region Vm events
        void Vm_ChangeTimer(object? s, int status)
        {
            switch (status)
            {
                case 0:
                    _timer.Stop();
                    _vehicleTimer.Stop();
                    break;
                default:
                    if (!_timer.IsEnabled)
                    {
                        _timer.Start();
                        _vehicleTimer.Start();
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
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "SimCity tábla betöltése";
                saveFileDialog.Filter = "SimCity tábla|*.sc";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveGameAsync(saveFileDialog.FileName);
                        _vm.ShowHelpEvent += Vm_ShowHelp;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Valami félrement mentés közben!\n" + ex.Message, "SimCity", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    
                    await _model.LoadGameAsync(openFileDialog.FileName);
                    _vm.ShowHelpEvent += Vm_ShowHelp;

                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("A fájl betöltése sikertelen!\n" + ex.Message, "SimCity", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Vm_ShowHelp(object? s, EventArgs e)
        {
            MessageBox.Show(
                "Irányítások:\n" +
                "W A S D - Mozgás a kamerával\n" +
                "Q E - Zoomolás a kamerával\n" +
                "Ha ég valami, akkor kattintással kiolthatod!"
                , "SimCity", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
        #region View events
        private void View_FocusChanged(object? s, EventArgs e)
        {
            _vm.inFocus = _view.IsActive;
        }
        #endregion
        #region Model events
        void M_GameOver(object? s, EventArgs e)
        {
            _vm.GameIsNotOver = false;
            _vm.TimeSet.Execute("0");
            MessageBox.Show("☠ Vége a játéknak! ☠", "SimCity", MessageBoxButton.OK);
        }

        void M_GameLoaded(object? sender, SimCityModel newModel)
        {
            _timer.Stop();
            _vehicleTimer.Stop();

            _model.GameOver -= M_GameOver;
            _vm.ChangeTime -= Vm_ChangeTimer;
            _vm.SaveGameEvent -= Vm_SaveGame;
            _vm.LoadGameEvent -= Vm_LoadGame;
            _vm.NewGameEvent -= Vm_NewGame;

            _model = newModel;
            _vm = new SimCityViewModel(_model);

            _model.GameOver += M_GameOver;
            _model.GameLoaded += M_GameLoaded;
            _vm.ChangeTime += Vm_ChangeTimer;
            _vm.SaveGameEvent += Vm_SaveGame;
            _vm.LoadGameEvent += Vm_LoadGame;
            _vm.NewGameEvent += Vm_NewGame;

            _view.DataContext = _vm;
            _view.CamInit();

            GC.Collect();
        }

        #endregion
        #endregion
    }
}
