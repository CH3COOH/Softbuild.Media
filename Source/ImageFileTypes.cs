using System;

namespace Softbuild.Media
{
    /// <summary>
    /// 
    /// </summary>
    public enum ImageFileTypes
    {
        /// <summary>
        /// 標準的な画像ファイル
        /// </summary>
        Normal,

        /// <summary>
        /// 拡張GIF画像ファイル(ストローク情報付き)
        /// </summary>
        GifWithStrokes, // for Win8 InkManager
    }

}
