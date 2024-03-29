/* 
 * PROJECT: NyARToolkitCS
 * --------------------------------------------------------------------------------
 * This work is based on the original ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 *
 * The NyARToolkitCS is Java edition ARToolKit class library.
 * Copyright (C)2008-2012 Ryo Iizuka
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * For further information please contact.
 *	http://nyatla.jp/nyatoolkit/
 *	<airmail(at)ebony.plala.or.jp> or <nyatla(at)nyatla.jp>
 * 
 */
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
namespace jp.nyatla.nyartoolkit.cs.core
{
    /**
     * {@link NyARCode}クラスの支援クラスです。
     * このクラスは、{@link NyARCode}のマーカファイル読み取り機能のうち、InputStreamからARToolkit形式のマーカデータを読み取って配列に格納する手順を実装します。
     * {@link NyARCode}以外から使用することはありません。
     */
    class NyARCodeFileReader
    {

        /**
         * ImputStreamからARToolKit形式のマーカデータを読み、o_raster[4]に格納します。
         * @param i_stream
         * 読出し元のストリームです。
         * @param o_raster
         * 出力先のラスタ配列です。
         * バッファ形式は形式はINT1D_X8R8G8B8_32であり、4要素、かつ全て同一なサイズである必要があります。
         * @
         */
        public static void loadFromARToolKitFormFile(StreamReader i_stream, NyARRaster[] o_raster)
        {
            Debug.Assert(o_raster.Length == 4);
            //4個の要素をラスタにセットする。
            try
            {
                string[] data = i_stream.ReadToEnd().Split(new Char[] { ' ', '\r', '\n' });
                //GBRAで一度読みだす。
                int idx = 0;
                for (int h = 0; h < 4; h++)
                {
                    Debug.Assert(o_raster[h].isEqualBufferType(NyARBufferType.INT1D_X8R8G8B8_32));
                    NyARRaster ra = o_raster[h];
                    idx = readBlock(data, idx, ra.getWidth(), ra.getHeight(), (int[])ra.getBuffer());
                }
            }
            catch (Exception e)
            {
                throw new NyARException(e);
            }
            return;
        }
        /**
         * ImputStreamからARToolKit形式のマーカデータを読み込み、o_codeオブジェクトへ格納します。
         * @param i_stream
         * 読出し元のストリームです。
         * @param o_code
         * 出力先の{@link NyARCode}オブジェクトです。
         * @
         */
        public static void loadFromARToolKitFormFile(StreamReader i_stream, NyARCode o_code)
        {
            int width = o_code.getWidth();
            int height = o_code.getHeight();
            NyARRgbRaster tmp_raster = new NyARRgbRaster(width, height, NyARBufferType.INT1D_X8R8G8B8_32);
            //4個の要素をラスタにセットする。
            try
            {
                int[] buf = (int[])tmp_raster.getBuffer();
                string[] data = i_stream.ReadToEnd().Split(new Char[] { ' ', '\r', '\n' });
                //GBRAで一度読みだす。
                int idx = 0;
                for (int h = 0; h < 4; h++)
                {
                    idx = readBlock(data, idx, width, height, buf);
                    //ARCodeにセット(カラー)
                    o_code.getColorData(h).setRaster(tmp_raster);
                    o_code.getBlackWhiteData(h).setRaster(tmp_raster);
                }
            }
            catch (Exception e)
            {
                throw new NyARException(e);
            }
            tmp_raster = null;//ポイ
            return;
        }
        /**
         * ストリームi_stから、1ブロック(1方位分)のXRGBデータをからo_bufへ読みだします。
         * @param i_data
         * 入力元のStreamTokenizerを指定します。
         * i_stの読み取り位置は更新されます。
         * @param i_width
         * パターンの横解像度(pixel)です。
         * @param i_height
         * パターンの縦解像度(pixel)です。
         * @param o_buf
         * 読み取った値を格納する配列です。
         * @
         */
        private static int readBlock(string[] i_data, int i_idx, int i_width, int i_height, int[] o_buf)
        {
            int idx = i_idx;
            try
            {
                int pixels = i_width * i_height;
                for (int i3 = 0; i3 < 3; i3++)
                {
                    for (int i2 = 0; i2 < pixels; i2++)
                    {
                        //数値のみ読み出す(空文字は読み飛ばし！)
                        for (; ; )
                        {
                            if (i_data[idx].Length > 0)
                            {
                                break;
                            }
                            idx++;
                        }
                        o_buf[i2] = (o_buf[i2] << 8) | ((0x000000ff & (int)int.Parse(i_data[idx])));
                        idx++;
                    }
                }
                //GBR→RGB
                for (int i3 = 0; i3 < pixels; i3++)
                {
                    o_buf[i3] = ((o_buf[i3] << 16) & 0xff0000) | (o_buf[i3] & 0x00ff00) | ((o_buf[i3] >> 16) & 0x0000ff);
                }
            }
            catch (Exception e)
            {
                throw new NyARException(e);
            }
            return idx;
        }
    }

    /**
     * このクラスは、ARToolKitのマーカーパターン1個のデータを格納します。
     * マーカーパターンのプロパティと、データのロード機能を提供します。
     */
    public class NyARCode
    {
        private NyARMatchPattDeviationColorData[] _color_pat = new NyARMatchPattDeviationColorData[4];
        private NyARMatchPattDeviationBlackWhiteData[] _bw_pat = new NyARMatchPattDeviationBlackWhiteData[4];
        private int _width;
        private int _height;

        /**
         * 指定したdirection(方位)の{@link NyARMatchPattDeviationColorData}オブジェクトの参照値を返します。
         * @param i_index
         * 方位インデクスの値を指定します。
         * 範囲は、0&lt;=n&lt;=3の数値です。
         * @return
         * 指定した方位の{@link NyARMatchPattDeviationColorData}オブジェクトを返します。
         */
        public NyARMatchPattDeviationColorData getColorData(int i_index)
        {
            return this._color_pat[i_index];
        }
        /**
         * 指定したdirection(方位)の{@link NyARMatchPattDeviationBlackWhiteData}オブジェクトの参照値を返します。
         * @param i_index
         * 方位インデクスの値を指定します。
         * 範囲は、0&lt;=n&lt;=3の数値です。
         * @return
         * 指定した方位の{@link NyARMatchPattDeviationBlackWhiteData}オブジェクトを返します。
         */
        public NyARMatchPattDeviationBlackWhiteData getBlackWhiteData(int i_index)
        {
            return this._bw_pat[i_index];
        }
        /**
         * ARマーカの横解像度を返します。
         * @return
         * 解像度値
         */
        public int getWidth()
        {
            return _width;
        }

        /**
         * ARマーカの縦解像度を返します。
         * @return
         * 解像度値
         */
        public int getHeight()
        {
            return _height;
        }


        /**
         * コンストラクタです。
         * 空のNyARCodeオブジェクトを作成します。
         * @param i_width
         * 作成するマーカパターンの横解像度
         * @param i_height
         * 作成するマーカパターンの縦解像度
         * @
         */
        public NyARCode(int i_width, int i_height)
        {
            this._width = i_width;
            this._height = i_height;
            //空のラスタを4個作成
            for (int i = 0; i < 4; i++)
            {
                this._color_pat[i] = new NyARMatchPattDeviationColorData(i_width, i_height);
                this._bw_pat[i] = new NyARMatchPattDeviationBlackWhiteData(i_width, i_height);
            }
            return;
        }
        /**
         * inputStreamから、ARToolKit形式のパターンデータをロードします。
         * ロードするパターンデータの縦横解像度は、このインスタンスの値と同じである必要があります。
         * @param i_stream
         * 読出し元のStreamオブジェクト
         * @
         */
        public void loadARPatt(StreamReader i_stream)
        {
            //ラスタにパターンをロードする。
            NyARCodeFileReader.loadFromARToolKitFormFile(i_stream, this);
            return;
        }
        /**
         * 4枚のラスタオブジェクトから、マーカーパターンを生成して格納します。
         * @param i_raster
         * パターンデータを格納したラスタオブジェクト配列を指定します。
         * ラスタは同一な解像度であり、かつこのインスタンスと同じ解像度である必要があります。
         * 格納順は、パターンの右上が、1,2,3,4象限になる順番です。
         * @
         */
        public void setRaster(INyARRgbRaster[] i_raster)
        {
            Debug.Assert(i_raster.Length != 4);
            //ラスタにパターンをロードする。
            for (int i = 0; i < 4; i++)
            {
                this._color_pat[i].setRaster(i_raster[i]);
            }
            return;
        }
        /**
         * 1枚のラスタオブジェクトから、マーカーパターンを生成して格納します。
         * 残りの3枚のデータは、関数がi_rasterを回転させて求めます。
         * @param i_raster
         * パターンデータを格納したラスタオブジェクトを指定します。
         * ラスタは、このインスタンスと同じ解像度である必要があります。
         * @
         */
        public void setRaster(INyARRgbRaster i_raster)
        {
            //ラスタにパターンをロードする。
            for (int i = 0; i < 4; i++)
            {
                this._color_pat[i].setRaster(i_raster, i);
            }
            return;
        }
    }
}
