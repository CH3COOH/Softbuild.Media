//
// AutoColoringEffect.cs
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
using Softbuild.Media.Effects.GiCoCu;

namespace Softbuild.Media.Effects
{
    /// <summary>
    /// 疑似的に着色をおこなうクラス
    /// </summary>
    public class AutoColoringEffect : IEffect
    {
        /// <summary>
        /// 
        /// </summary>
        private Curve Curve { get; set; }

        /// <summary>
        /// AutoColoringEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="strm"></param>
        /// <param name="type"></param>
        public AutoColoringEffect(Stream strm, CurveTypes type)
        {
            Curve = new Curve(strm, type);
        }
        
        /// <summary>
        /// AutoColoringEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="curve"></param>
        public AutoColoringEffect(Curve curve)
        {
            Curve = curve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public byte[] Effect(int width, int height, byte[] source)
        {
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルの各ARGB要素を取得する
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // マッピングされたカーブテーブルを元に疑似着色を実施する
                int da = Curve.Data[4, a];
                int dr = Curve.Data[0, Curve.Data[1, r]];
                int dg = Curve.Data[0, Curve.Data[2, g]];
                int db = Curve.Data[0, Curve.Data[3, b]];

                // 処理後のピクセルの各ARGB要素を取得する
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 3] = (byte)Math.Min(255, Math.Max(0, da));
            }

            return dest;
        }
    }
}
