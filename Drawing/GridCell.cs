using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Drawing
{
    public class GridCell
    {
        /*
         * will have 4 points, the corners, and 4 other points, the halfway between the corners, to draw
         * the marching squares lines.
         */

        /*
         *      *    *       0    1     this is the order of corners.
         *     
         *      *    *       3    2
         *      
         *         *            1
         *      
         *      *     *      0     2    data for the halfways.
         *         
         *         *            3
         */ 
        //coordinates in wpf pixels
        public IntPoint[] corners;
        //coordinates in blender viewport units to represent the given vertex's height (Z data)
        public double[] cornerValues;
        //the halfway between the corners, calculating beforehand to have them when the lines are drawn.
        public IntPoint[] halfways;

        public GridCell(IntPoint[] corners, double[] cornerValues)
        {
            //not minding null safety and length mismatch

            this.corners = new IntPoint[4];
            for (int i = 0; i < this.corners.Length; i++)
            {
                this.corners[i] = corners[i];
            }

            this.cornerValues = new double[4];
            for (int i = 0; i < this.cornerValues.Length; i++)
            {
                this.cornerValues[i] = cornerValues[i];
            }

            //now calculating the halfway points. 
            halfways = new IntPoint[4];

            //between corner 0 and 3
            halfways[0] = new IntPoint(corners[0].X, (corners[0].Y + corners[3].Y) / 2);
            //between corner 0 and 1
            halfways[1] = new IntPoint((corners[0].X + corners[1].X) / 2, corners[0].Y);
            //between corner 1 and 2
            halfways[2] = new IntPoint(corners[1].X, (corners[1].Y + corners[2].Y) / 2);
            //between corner 2 and 3
            halfways[3] = new IntPoint((corners[2].X + corners[3].X) / 2, corners[2].Y);
        }

        public List<Tuple<IntPoint, IntPoint>> GetLineEndings(double tresholdInBlenderCoords)
        {
            /*
             * getting the coordinate pairs between whom the drawer has to draw a line.
             */

            var pairs = new List<Tuple<IntPoint, IntPoint>>();
            bool[] _tresholded = new bool[cornerValues.Length];
            for (int i = 0; i < cornerValues.Length; i++)
            {
                _tresholded[i] = cornerValues[i].IsOutOfTreshold(tresholdInBlenderCoords);
            }

            //we have them. now for an ugly mapping:
            
            if(!_tresholded[0] && !_tresholded[1] && !_tresholded[2] && !_tresholded[3])
            {
                //no line drawn here
            }else if(!_tresholded[0] && !_tresholded[1] && !_tresholded[2] && _tresholded[3])
            {
                //line between 0 and 3 halfway points.
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[3]));
            }else if(!_tresholded[0] && !_tresholded[1] && _tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[2], halfways[3]));
            }
            else if(!_tresholded[0] && !_tresholded[1] && _tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[2]));
            }
            else if(!_tresholded[0] && _tresholded[1] && !_tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[1], halfways[2]));
            }
            else if(!_tresholded[0] && _tresholded[1] && !_tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[1]));
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[2], halfways[3]));
            }
            else if(!_tresholded[0] && _tresholded[1] && _tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[1], halfways[3]));
            }
            else if(!_tresholded[0] && _tresholded[1] && _tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[1]));
            }
            else if(_tresholded[0] && !_tresholded[1] && !_tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[1]));
            }
            else if(_tresholded[0] && !_tresholded[1] && !_tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[1], halfways[3]));
            }
            else if(_tresholded[0] && !_tresholded[1] && _tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[3]));
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[1], halfways[2]));
            }
            else if(_tresholded[0] && !_tresholded[1] && _tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[1], halfways[2]));
            }
            else if(_tresholded[0] && _tresholded[1] && !_tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[2]));
            }
            else if(_tresholded[0] && _tresholded[1] && !_tresholded[2] && _tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[2], halfways[3]));
            }
            else if(_tresholded[0] && _tresholded[1] && _tresholded[2] && !_tresholded[3])
            {
                pairs.Add(new Tuple<IntPoint, IntPoint>(halfways[0], halfways[3]));
            }
            else if(_tresholded[0] && _tresholded[1] && _tresholded[2] && _tresholded[3])
            {
                //no line here either.
            }

            return pairs;
        }
    }

    public struct IntPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public IntPoint(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
