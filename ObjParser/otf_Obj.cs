using ObjParser.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        //using a dictionary to avoid the penalty for resizing an array again and again but 
        //still be able to index quickly
        public Dictionary<int, Vertex> VertexList { get; private set; }

        public Action<Face, otf_Obj, double> OnNewFaceArrived;

        public double XExtents { get; private set; }

        protected int currentLastVertex = 0;

        double treshold = 0;

        private object vertexAddLocker = new object();

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
            //reading the vertex data in parallel.
            //note during this read the faces are not drawn.
            //that is due to the obj specification.
            //Parallel.ForEach(File.ReadLines(path), line =>
            //{
            //    ProcessLineForVertexData(line, ref min, ref max);
            //});
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
                        lock (vertexAddLocker)
                        {
                            Vertex v = new Vertex();
                            v.LoadFromStringArray(parts);
                            if (v.X > max)
                                max = v.X;
                            if (v.X < min)
                                min = v.X;
                            AddVertex(v);
                        }
                        
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
        /*
         * da master-worker pattern.
         * da master just reads in the face lines, literally without processing 
         * that is, "starts with f - it's a face."
         * then stores the lines and a new face instance in a concurrent bag
         * for the workers to munch on.
         * 
         * in the meantime the workers take whatever they find in the faces bag
         * again, execution order is not an issue here - the faces know which vertices
         * they need in order to form a face.
         * 
         * now the speed comes from several workers looking for the vertices of a face 
         * instead of one.
         */ 
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
                    while (faces.Count > 0 || !master.IsCompleted)
                    {
                        ProcessFace();
                    }
                    
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

        private void ProcessFace()
        {
            if (!faces.TryTake(out FaceDefinition fDef))
                return;

            fDef.face.LoadFromStringArray(fDef.parts);
            OnNewFaceArrived?.Invoke(fDef.face, this, treshold);        
        }
    }

    public class FaceDefinition
    {
        public string[] parts { get; set; }
        public Face face {get;set;}
    }
}
