using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace simcityView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        ICommand? MoveUpDownCom;
        ICommand? MoveLeftRightCom;
        ICommand? ZoomCom;
        
        private float _moveSensitivity = 10.0f;
        private float _zoomSensitivity = 0.05f;

        private long _prevFrameTime = 0;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        


        public MainWindow()
        {
            InitializeComponent();
            
        }
        
       
         

        public void CamInit()
        {
           
            PropertyInfo? upDownInfo = DataContext.GetType().GetProperty("MovePlayFieldUpDown");
            PropertyInfo? leftRightInfo = DataContext.GetType().GetProperty("MovePlayFieldLeftRight");
            PropertyInfo? zoomInfo = DataContext.GetType().GetProperty("ZoomPlayField");
            CompositionTarget.Rendering -= KeyboardKeys;

            if (upDownInfo!=null && leftRightInfo!=null && zoomInfo!=null)
            {
                
                MoveUpDownCom = upDownInfo.GetValue(DataContext) as ICommand;
                MoveLeftRightCom = leftRightInfo.GetValue(DataContext) as ICommand;
                ZoomCom = zoomInfo.GetValue(DataContext) as ICommand;

                
                CompositionTarget.Rendering += KeyboardKeys;
                _stopwatch.Start();
            }
           
            
            
        }

        private void KeyboardKeys(object? sender, EventArgs e)
        {
           

            

            float calcMoveUpDown = 0;
            float calcMoveLeftRight = 0;
            float calcZoom = 0;

            float deltaTime = (_stopwatch.ElapsedMilliseconds - _prevFrameTime) / 10.0f;
            _prevFrameTime = _stopwatch.ElapsedMilliseconds;

            if (Keyboard.IsKeyDown(Key.W))
            {
                calcMoveUpDown += _moveSensitivity;
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                calcMoveUpDown -= _moveSensitivity;
            }
            
            MoveUpDownCom!.Execute(calcMoveUpDown * deltaTime);
            
            if (Keyboard.IsKeyDown(Key.A))
            {
                calcMoveLeftRight += _moveSensitivity;
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                calcMoveLeftRight -= _moveSensitivity;
            }
            
            MoveLeftRightCom!.Execute(calcMoveLeftRight * deltaTime);
            
            if (Keyboard.IsKeyDown(Key.Q))
            {
                calcZoom += _zoomSensitivity;
            }
            if (Keyboard.IsKeyDown(Key.E))
            {
                calcZoom -= _zoomSensitivity;
            }
            
            ZoomCom!.Execute(calcZoom* deltaTime);
            
        }

    }
}
