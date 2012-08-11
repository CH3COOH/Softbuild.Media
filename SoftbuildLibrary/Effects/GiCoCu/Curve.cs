//
// Curve.cs
//
// original work is E-Male.
// http://avisynth.org.ru/docs/english/externalfilters/gicocu.htm
// http://avisynth.org/warpenterprises/files/gicocu_25_dll_20050620.zip
//
// Modified for work by Kenji Wada, http://ch3cooh.jp/
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files 
// (the "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

namespace Softbuild.Media.Effects.GiCoCu
{
    /// <summary>
    /// 
    /// </summary>
    class CRMatrix
    {
        public float[,] Data { get; set; }

        public CRMatrix()
        {
            Data = new float[4, 4];
        }

        public CRMatrix(float[,] data)
        {
            Data = data;
        }

        public CRMatrix Compose(CRMatrix b)
        {
            var result = new CRMatrix();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    result.Data[i, j] = 
                        Data[i, 0] * b.Data[0, j] + 
                        Data[i, 1] * b.Data[1, j] + 
                        Data[i, 2] * b.Data[2, j] + 
                        Data[i, 3] * b.Data[3, j];
                }
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Curve
    {

#region Constructor
        
        /// <summary>
        /// 
        /// </summary>
        public Curve()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strm"></param>
        public Curve(Stream strm) : this(strm, CurveTypes.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strm"></param>
        /// <param name="type"></param>
        public Curve(Stream strm, CurveTypes type)
        {
            Curve temp = null;
            if (type == CurveTypes.Photoshop)
            {
                throw new NotImplementedException();
            }
            else if (type == CurveTypes.Gimp)
            {
                temp = Curve.GetGimpCurve(strm);
            }
            else
            {
                throw new ArgumentException("invalid is CurveTypes.");
            }

            Data = temp.Data;
            Points = temp.Points;
        }
#endregion

        private static Curve GetGimpCurve(Stream strm)
        {
            var curves = new Curve();

            var index = new int[5, 17];
            var value = new int[5, 17];

            var reader = new StreamReader(strm);

            // ヘッダーがGIMP形式になっているかチェック
            var header = reader.ReadLine();
            if ("# GIMP Curves File" != header)
            {
                throw new IOException("not gimp curves file");
            }

            for (int i = 0; i < 5; i++)
            {
                string line = reader.ReadLine();
                var values = line.Split(' ');

                for (int j = 0, k = 0; j < 17; j++)
                {
                    index[i, j] = int.Parse(values[k++]);
                    value[i, j] = int.Parse(values[k++]);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    curves.Points[i, j, 0] = index[i, j];
                    curves.Points[i, j, 1] = value[i, j];
                }
            }

            // make LUTs
            for (int i = 0; i < 5; i++)
            {
                curves.Calculate(i);
            }

            return curves;
        }

        private int[, ,] _points = new int[5, 17, 2];
        public int[, ,] Points
        {
            get { return _points; }
            set { _points = value; }
        }

        private int[,] _data = new int[5, 256];
        public int[,] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private readonly int MaxPoint = 17;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public void Calculate(int channel)
        {
            var points = new int[MaxPoint];

            int num_pts = 0;
            for (int i = 0; i < MaxPoint; i++)
            {
                if (Points[channel, i, 0] != -1)
                {
                    points[num_pts++] = i;
                }
            }

            if (num_pts != 0)
            {
                for (int i = 0; i < Points[channel, points[0], 0]; i++)
                {
                    Data[channel, i] = Points[channel, points[0], 1];
                }
                for (int i = Points[channel, points[num_pts - 1], 0]; i < 256; i++)
                {
                    Data[channel, i] = Points[channel, points[num_pts - 1], 1];
                }
            }

            for (int i = 0; i < num_pts - 1; i++)
            {
                int p1 = (i == 0) ? points[i] : points[(i - 1)];
                int p2 = points[i];
                int p3 = points[(i + 1)];
                int p4 = (i == (num_pts - 2)) ? points[(num_pts - 1)] : points[(i + 2)];

                plotCurve(channel, p1, p2, p3, p4);
            }

            for (int i = 0; i < num_pts; i++)
            {
                int x = Points[channel, points[i], 0];
                int y = Points[channel, points[i], 1];
                Data[channel, x] = y;
            }
        }

        private void plotCurve(int channel, int p1, int p2, int p3, int p4)
        {
            // construct the geometry matrix from the segment
            var geometry = new CRMatrix();
            for (int i = 0; i < 4; i++)
            {
                geometry.Data[i, 2] = 0;
                geometry.Data[i, 3] = 0;
            }
            for (int i = 0; i < 2; i++)
            {
                geometry.Data[0, i] = Points[channel, p1, i];
                geometry.Data[1, i] = Points[channel, p2, i];
                geometry.Data[2, i] = Points[channel, p3, i];
                geometry.Data[3, i] = Points[channel, p4, i];
            }

            // subdivide the curve 1000 times
            // n can be adjusted to give a finer or coarser curve
            float d = 1.0f / 1000;
            float d2 = d * d;
            float d3 = d * d * d;

            // construct a temporary matrix for determining the forward differencing deltas
            var tmp2 = new CRMatrix();
            tmp2.Data[0, 3] = 1;
            tmp2.Data[1, 0] = d3;
            tmp2.Data[1, 1] = d2;
            tmp2.Data[1, 2] = d;
            tmp2.Data[2, 0] = 6 * d3;
            tmp2.Data[2, 1] = 2 * d2;
            tmp2.Data[3, 0] = 6 * d3;

            var basis = new CRMatrix(new float[,] {
                { -0.5f, 1.5f, -1.5f, 0.5f },
                { 1.0f, -2.5f, 2.0f, -0.5f },
                { -0.5f, 0.0f, 0.5f, 0.0f },
                { 0.0f, 1.0f, 0.0f, 0.0f }
            });

            // compose the basis and geometry matrices
            var tmp1 = basis.Compose(geometry);

            // compose the above results to get the deltas matrix
            var deltas = tmp2.Compose(tmp1);

            // extract the x deltas
            float x = deltas.Data[0, 0];
            float dx = deltas.Data[1, 0];
            float dx2 = deltas.Data[2, 0];
            float dx3 = deltas.Data[3, 0];

            // extract the y deltas
            float y = deltas.Data[0, 1];
            float dy = deltas.Data[1, 1];
            float dy2 = deltas.Data[2, 1];
            float dy3 = deltas.Data[3, 1];

            int lastx = Math.Max(0, Math.Min(255, (int)Math.Round(x)));
            int lasty = Math.Max(0, Math.Min(255, (int)Math.Round(y)));

            Data[channel, lastx] = lasty;

            // loop over the curve
            for (int i = 0; i < 1000; i++)
            {
                // increment the x values
                x += dx;
                dx += dx2;
                dx2 += dx3;

                // increment the y values
                y += dy;
                dy += dy2;
                dy2 += dy3;

                int newx = Math.Max(0, Math.Min(255, (int)Math.Round(x)));
                int newy = Math.Max(0, Math.Min(255, (int)Math.Round(y)));

                // if this point is different than the last one...then draw it
                if ((lastx != newx) || (lasty != newy))
                {
                    Data[channel, newx] = newy;
                }

                lastx = newx;
                lasty = newy;
            }
        }
    }
}
