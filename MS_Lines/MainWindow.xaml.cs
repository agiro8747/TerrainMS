using MS_Lines.Drawing;
using System.Linq;
using System.Windows.Input;

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
            builder = new MapBuilder(Canvas, ActualWidth);
        }

        //reading the .obj file on demand using external lib
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Canvas.Children.Remove(Message);
                //read obj file here.
                var obj = new ObjParser.Obj();
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = ".obj";

                bool? res = dialog.ShowDialog();
                string filename;
                if (res == false)
                    throw new System.Exception("Couldn't open yo dialog."); 
                
                filename = dialog.FileName;
                obj.LoadObj(filename);

                builder.GenerateMap(obj);
            }
        }
    }
}