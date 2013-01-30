
namespace Softbuild.Media.Effects
{
    /// <summary>
    /// Hilditchの方法による二値画像の細線化をおこなうクラス
    /// </summary>
    public class ThinningEffect : IEffect
    {
        /// <summary>
        /// 二値化をおこなう際の閾値
        /// </summary>
        private int Threshold { get; set; }

        private byte Stroke { get; set; }

        /// <summary>
        /// ThinningEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        public ThinningEffect()
            : this(60, 0)
        {
        }

        /// <summary>
        /// ThinningEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="stroke"></param>
        public ThinningEffect(int threshold, byte stroke)
        {
            Threshold = threshold;
            Stroke = stroke;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        private void SetPixel(int width, int height, ref byte[] source, int x, int y, byte pixel)
        {
            int index = x * 4 + y * (width * 4);

            source[index] = pixel;
            source[index + 1] = pixel;
            source[index + 2] = pixel;
            source[index + 3] = 255;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private byte GetPixel(int width, int height, byte[] source, int x, int y)
        {
            int index = x * 4 + y * (width * 4);
            byte pixel = source[index];
            return pixel;
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
            
            var temp = new byte[source.Length];
            var temp1 = new byte[pixelCount];

            // まずは画像を閾値をThresholdで二値化する
            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルから各BGAR要素を取得する
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // 輝度を求める
                byte y = ((byte)(r * 0.2126 + g * 0.7152 + b * 0.0722));
                // 二値化する
                byte y2 = (y < Threshold) ? byte.MinValue : byte.MaxValue; 

                // 処理後のピクセルデータを出力用バッファへ格納する
                temp[index + 0] = y2;
                temp[index + 1] = y2;
                temp[index + 2] = y2;
                temp[index + 3] = a;
            }

            var dest = new byte[source.Length];
            var ia = new int[9];
            var ic = new int[9];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    // 処理前のピクセルから各BGAR要素を取得する
                    var gray = GetPixel(width, height, temp, x, y);
                    if (gray != byte.MaxValue) continue;

                    ia[0] = GetPixel(width, height, temp, x + 1, y);
                    ia[1] = GetPixel(width, height, temp, x + 1, y - 1);
                    ia[2] = GetPixel(width, height, temp, x, y - 1);
                    ia[3] = GetPixel(width, height, temp, x - 1, y - 1);
                    ia[4] = GetPixel(width, height, temp, x - 1, y);
                    ia[5] = GetPixel(width, height, temp, x - 1, y + 1);
                    ia[6] = GetPixel(width, height, temp, x, y + 1);
                    ia[7] = GetPixel(width, height, temp, x + 1, y + 1);

                    for (int i = 0; i < 8; i++)
                    {
                        if (ia[i] == Stroke)
                        {
                            ia[i] = byte.MaxValue;
                            ic[i] = 0;
                        }
                        else
                        {
                            if (ia[i] < byte.MaxValue)
                            {
                                ia[i] = 0;
                            }
                            ic[i] = ia[i];
                        }
                    }

                    ia[8] = ia[0];
                    ic[8] = ic[0];

                    if ((ia[0] + ia[2] + ia[4] + ia[6]) == byte.MaxValue * 4) continue;

                    int iv = 0, iw = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (ia[i] == byte.MaxValue) iv++;
                        if (ic[i] == byte.MaxValue) iw++;
                    }

                    if (iv <= 1) continue;
                    if (iw == 0) continue;

                    if (cconc(ia) != 1) continue;

                    var p = GetPixel(width, height, temp, x, y - 1);
                    if (p == Stroke)
                    {
                        ia[2] = 0;
                        if (cconc(ia) != 1) continue;
                        ia[2] = byte.MaxValue;
                    }
                    p = GetPixel(width, height, temp, x - 1, y);
                    if (p == Stroke)
                    {
                        ia[4] = 0;
                        if (cconc(ia) != 1) continue;
                        ia[4] = byte.MaxValue;
                    }
                    
                    // 処理後のピクセルデータを出力用バッファへ格納する
                    SetPixel(width, height, ref dest, x, y, Stroke);
                }
            }

            return dest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inb"></param>
        /// <returns></returns>
        private int cconc(int[] inb)
        {          
            int icn = 0;

            for (int i = 0; i < 8; i += 2)
            {
                if ((inb[i] == 0) && 
                    (inb[i + 1] == byte.MaxValue || inb[i + 2] == byte.MaxValue))
                {
                    icn++;
                }
            }
            return icn;
        }

    }
}
