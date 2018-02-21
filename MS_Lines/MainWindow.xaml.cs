using MS_Lines.Drawing;
using System.Linq;
using System.Windows.Input;
using ObjParser;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace MS_Lines
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MapBuilder builder;
        public MainWindow()
        {
            InitializeComponent();
            builder = new MapBuilder(Canvas, Canvas.ActualWidth);
        }

        //reading the .obj file on demand using external lib
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Message.Text = "";
                //read obj file here.
                
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = ".obj";

                bool? res = dialog.ShowDialog();
                string filename;
                if (res == false)
                    throw new System.Exception("Couldn't open yo dialog."); 
                
                filename = dialog.FileName;
                var obj = new otf_Obj(builder.MapStep,filename);
                builder.Init(obj.XExtents);
                //this ain's parallel just yet of course.
                //da is like the stuff draws right in front of u

                Task t2 = new Task(() =>
                {
                    obj.OnTheFlyProcess(filename, 0.03);
                }, TaskCreationOptions.LongRunning);

                Task t = new Task(() =>
                {
                    obj.OnTheFlyProcess(filename, 0.01);
                    
                },TaskCreationOptions.LongRunning);

                t.ContinueWith((_t2) =>
                {
                    t2.Start();
                });
                t.Start();

                
                //da is like u wait for 4 mins and da image pops up
                //obj.OnTheFlyProcess(filename);
            }
            
        }
    }
}