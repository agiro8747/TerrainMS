using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MS_Lines.Drawing
{
    /*
     * drawing a map that fits the screen.         
     * 
     */
    public class Drawer
    {
        //break line after every breakLine processed grid cell.
        //just yet it's basically hardcoded.
        public static readonly int breakLine = 5;

        //determined by the given window size.
        public static int cellSize;

        //depricated
        public static void RepresentGrid(Canvas canvas, GridCell[] cells)
        {
            //going through all the grid cells, and draw 3 lines for each, it should be enough.
            //no matter if overdraw :P  

            //telling us which row we are in now.
            //on every line break we increase the row, so go to another row. 
            int rows = 0;
            
            //telling us which column we are in. 
            //on every line break reset. otherwose increase gradually.
            int cols = 0;


            cellSize = (int)canvas.Width / breakLine;

            for (int i = 0; i < cells.Length; i++)
            {

                //i counts the grid cells. 
                for (int j = 0; j < 3; j++)
                {
                    //j counts the lines for a cell.
                    var line = new Line();
                    line.Stroke = Brushes.LightSteelBlue;

                    //adding the offsets
                    line.X1 = cells[i].corners[j].X + cols * cellSize;
                    line.X2 = cells[i].corners[j + 1].X + cols * cellSize;
                    line.Y1 = cells[i].corners[j].Y + rows * cellSize;
                    line.Y2 = cells[i].corners[j + 1].Y + rows * cellSize;

                    line.StrokeThickness = 2;
                    canvas.Children.Add(line);

                    //now detecting if I reached the end of this column
                    if(i % (breakLine - 1) == 0)
                    {
                        rows++;
                        cols = 0;
                    }
                }
            }
        }

        public static void DrawGridCell(Canvas canvas, GridCell gc)
        {
            //i counts the grid cells. 
            for (int j = 0; j < 3; j++)
            {
                //j counts the lines for a cell.
                canvas.Children.Add(
                    PutLine(
                    gc.corners[j].X,
                    gc.corners[j + 1].X,
                    gc.corners[j].Y,
                    gc.corners[j + 1].Y,
                    Brushes.LightGray,
                    2)
                 );
            }
        }

        public static void DrawLines(Canvas canvas, List<Tuple<IntPoint, IntPoint>> endings)
        {
            foreach (var item in endings)
            {
                canvas.Children.Add(
                    PutLine(
                        item.Item1.X,
                        item.Item2.X,
                        item.Item1.Y,
                        item.Item2.Y,
                        Brushes.OrangeRed,
                        2
                        ));
            }
        }

        private static Line PutLine(double X1, double X2, double Y1, double Y2, Brush color, double thickness)
        {
            var line = new Line();
            line.Stroke = color;

            //adding the offsets
            line.X1 = X1;
            line.X2 = X2;
            line.Y1 = Y1;
            line.Y2 = Y2;

            line.StrokeThickness = thickness;
            return line;
        }
    }
}
