﻿//
// ImageDirectories.cs
//
// Copyright (c) 2012 Kenji Wada, http://ch3cooh.jp/
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
 
#if NETFX_CORE
using Windows.Storage;
#endif

namespace Softbuild.Media
{
    /// <summary>
    /// 保存、読み込み先の種別
    /// </summary>
    public enum ImageDirectories
    {
        /// <summary>
        /// ピクチャー ライブラリ
        /// </summary>
        PicturesLibrary,

        /// <summary>
        /// ドキュメント ライブラリ
        /// </summary>
        DocumentsLibrary,

        /// <summary>
        /// アプリケーション ローカル
        /// </summary>
        InApplicationLocal,

        /// <summary>
        /// アプリケーション ローミング
        /// </summary>
        InApplicationRoaming,

        /// <summary>
        /// アプリケーション テンポラリ
        /// </summary>
        InApplicationTemporary
    }

#if NETFX_CORE
    public static class ImageImageDirectoryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static StorageFolder GetStorageFolder(this ImageDirectories directory)
        {
            var folder = default(StorageFolder);
            switch (directory)
            {
                case ImageDirectories.PicturesLibrary:
                    folder = KnownFolders.PicturesLibrary;
                    break;
                case ImageDirectories.DocumentsLibrary:
                    folder = KnownFolders.DocumentsLibrary;
                    break;
                case ImageDirectories.InApplicationLocal:
                    folder = ApplicationData.Current.LocalFolder;
                    break;
                case ImageDirectories.InApplicationRoaming:
                    folder = ApplicationData.Current.RoamingFolder;
                    break;
                case ImageDirectories.InApplicationTemporary:
                    folder = ApplicationData.Current.TemporaryFolder;
                    break;
                default:
                    throw new ArgumentException();
            }
            return folder;
        }
    }
#endif
}
