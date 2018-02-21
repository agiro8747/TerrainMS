using ObjParser.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjParser
{
    public class otf_Obj
    {
        //standing for on the fly obj.
        /*
         * the goal is not to store everything in a collection, 
         * instead, as soon as we have a square, go ahead and draw it.
         * this way we save a lot on memory. 
         */

        //will have to make this one concurrent. 
        //face data relies on this, and is not read in order. 
        //as a result I can't eliminate the storage of vertices altogether :( 
        //for now, as it will grow a lot but still has to be indexed changed from list to 
        //a has based coll to get rid of
            //the excessive resize overhead.

        public Dictionary<int,Vertex> VertexList { get; private set; }

        public Action<Face, otf_Obj, double> OnNewFaceArrived;

        public double XExtents { get; private set; }

        protected int currentLastVertex = 0;

        public otf_Obj(Action<Face, otf_Obj, double> _onNewFace)
        {
            OnNewFaceArrived += _onNewFace;
            VertexList = new Dictionary<int, Vertex>();
        }

        public otf_Obj(Action<Face, otf_Obj, double> _onNewFace, string path)
        {
            OnNewFaceArrived += _onNewFace;
            VertexList = new Dictionary<int, Vertex>();
            XExtents = GetXExtentsofObj(path);
        }

        protected void AddVertex(Vertex v)
        {
            VertexList.Add(++currentLastVertex, v);
        }
        
	    private double GetXExtentsofObj(string path)
        {
            double min = 0, max = 0;
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    processLineForVertexData(line, ref min, ref max);
                }
            }
            return max - min;
        }

        private void processLineForVertexData(string line, ref double min, ref double max)
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                switch (parts[0])
                {
                    case "v":
                        Vertex v = new Vertex();
                        v.LoadFromStringArray(parts);
                        if (v.X > max)
                            max = v.X;
                        if (v.X < min)
                            min = v.X;
                        AddVertex(v);
                        break;
                }
            }
        }

        private void processLine(string line, double treshold)
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                switch (parts[0])
                {
                    case "f":
                        Face f = new Face();
                        f.LoadFromStringArray(parts);
                        OnNewFaceArrived?.Invoke(f, this, treshold);
                        break;
                }
            }
        }

        public void OnTheFlyProcess(string path, double treshold)
        {
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    processLine(line, treshold);
                }
            }
        }
    }
}
