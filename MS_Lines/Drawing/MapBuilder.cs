using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Algo.Properties;
using ObjParser;
using ObjParser.Types;

namespace MS_Lines.Drawing
{
    public class MapBuilder
    {
        private Canvas mCanvas; 
        //determined by the given window size.
        public static int cellSize;

        public MapBuilder(Canvas c, double windowWidth)
        {
            mCanvas = c;
        }

        public void GenerateMap(Obj obj)
        {
            /*
             * first of all we have to stretch the obj file to fill the viewport. 
             * to do this, we use its extents, and compare that to the window size.
             * 
             * that is, cellSize = width of canvas / size of X = how many pixels for 1 blender world unit.
             * from that, the canvas place of e.g. P(-1.245,0) would be center of screen - 1.245 * cellSize and 0
             * (mesh exported with pivot on origo).
             */

            cellSize = (int)(mCanvas.ActualWidth / obj.Size.XSize);

            foreach (Face _face in obj.FaceList)
            {
                /*
                 * but first we have to reorder the vertices for our grid cell.
                 * that way the grid cell can figure out 
                 */
                List<Vertex> faceVertices = linkVertices(obj, _face);

                var cell = new GridCell(
                    new IntPoint[]
                    {
                        BlenderCoordToWpfPixel(faceVertices.OrderByDescending(x => x.Y).Take(2).OrderBy(y => y.X).FirstOrDefault()),
                        BlenderCoordToWpfPixel(faceVertices.OrderByDescending(x => x.Y).Take(2).OrderByDescending(y => y.X).FirstOrDefault()),
                        BlenderCoordToWpfPixel(faceVertices.OrderBy(x => x.Y).Take(2).OrderByDescending(y => y.X).FirstOrDefault()),
                        BlenderCoordToWpfPixel(faceVertices.OrderBy(x => x.Y).Take(2).OrderBy(y => y.X).FirstOrDefault())
                    }, new double[]
                    {

                        faceVertices.OrderByDescending(x => x.Y).Take(2).OrderBy(y => y.X).FirstOrDefault().Z,
                        faceVertices.OrderByDescending(x => x.Y).Take(2).OrderByDescending(y => y.X).FirstOrDefault().Z,
                        faceVertices.OrderBy(x => x.Y).Take(2).OrderByDescending(y => y.X).FirstOrDefault().Z,
                        faceVertices.OrderBy(x => x.Y).Take(2).OrderBy(y => y.X).FirstOrDefault().Z
                    }
                    );

                //now that we have the gridCell, go ahead and draw it.
                //Drawer.DrawGridCell(mCanvas, cell);
                Drawer.DrawLines(mCanvas, cell.GetLineEndings(0.01));
            }
        }

        private IntPoint BlenderCoordToWpfPixel(Vertex blenderCoord)
        {
            /*
             * the mesh exported centered. 
             * so its center is on the middle of the wpf viewport.
             * we have how many pixels in the current viewport it takes to represent one blender world unit (cellsize)
             * from the center of the screen we go cellsize times the current coord (and the coord's sign tells the direction)
             */ 
            return new IntPoint((int)(mCanvas.ActualWidth / 2) + (int)(blenderCoord.X * cellSize),
                (int)(mCanvas.ActualHeight / 2) + (int)(blenderCoord.Y * cellSize));
        }

        private List<Vertex> linkVertices(Obj obj, Face face)
        {
            //getting the actual vertex data from a face using its vertex indexes. 
            var vList = new List<Vertex>();
            for (int i = 0; i < face.VertexIndexList.Length; i++)
            {
                vList.Add(obj.VertexList[face.VertexIndexList[i] - 1]);
            }
            return vList;
        }
    }
}
