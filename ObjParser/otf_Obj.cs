using ObjParser.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        /*
         * the parallel solution solvers:
         */
        public ConcurrentBag<FaceDefinition> faces = new ConcurrentBag<FaceDefinition>();

        public Dictionary<int, Vertex> VertexList { get; private set; }

        public Action<Face, otf_Obj, double> OnNewFaceArrived;

        public double XExtents { get; private set; }

        protected int currentLastVertex = 0;

        double treshold = 0;

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
                    ProcessLineForVertexData(line, ref min, ref max);
                }
            }
            return max - min;
        }

        private void ProcessLineForVertexData(string line, ref double min, ref double max)
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
        /*
         * basically this reader is a master. 
         * this is the one that produces the jobs for the workers.
         * where the jobs themselves are the faces to draw the height data from.
         * as soon as a new face arrives, the face is stored and a task is a notified. 
         * 
         */
        private void ProcessLine(string line, double treshold)
        {
            string[] _parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (_parts.Length > 0)
            {
                switch (_parts[0])
                {
                    case "f":
                        Face f = new Face();
                        //parallel solution here
                        /*
                         * so basically when we found a face, it means we already have its
                         * vertices read to a collection. that's because in an obj file
                         * the vertices preceed the faces.
                         * but we still have to find the vertices that match a face.
                         */
                        faces.Add(new FaceDefinition()
                        {
                            face = f,
                            parts = _parts
                        });
                        //f.LoadFromStringArray(parts);
                        //OnNewFaceArrived?.Invoke(f, this, treshold);

                        //now notify the workers that there is a new face.


                        break;
                }
            }
        }

        public void OnTheFlyProcess(string path, double treshold)
        {
            this.treshold = treshold;
            Task master = new Task(() =>
            {
                using (var reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ProcessLine(line, treshold);
                    }
                }
            }, TaskCreationOptions.LongRunning);

            List<Task> workers = new List<Task>();
            for (int i = 0; i < 4; i++)
            {
                workers.Add(new Task(() =>
                {
                    ProcessFace(master);
                }));
            }

            Stopwatch sw = new Stopwatch();
            Task.WhenAll(workers).ContinueWith(x => {
                sw.Stop();
                Console.WriteLine("Draw done. Time elapsed: " + sw.Elapsed);
            });

            sw.Start();

            master.Start();
            foreach (Task t in workers)
                t.Start();
        }

        private void ProcessFace(Task master)
        {
            while (faces.Count > 0 || !master.IsCompleted)
            {
                FaceDefinition fDef;
                if (!faces.TryTake(out fDef))
                    return;

                fDef.face.LoadFromStringArray(fDef.parts);
                OnNewFaceArrived?.Invoke(fDef.face, this, treshold);
            }
            
        }
    }

    public class FaceDefinition
    {
        public string[] parts { get; set; }
        public Face face {get;set;}
    }
}
