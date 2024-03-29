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
 * Copyright (C)2008-2009 Ryo Iizuka
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
using System.IO;
using System;
using System.Collections.Generic;
namespace jp.nyatla.nyartoolkit.cs.core
{








    /**
     * このクラスは、NyARToolkitの環境パラメータを格納します。
     * 環境パラメータは、ARToolKitのパラメータと同一です。
     * パラメータの要素には、以下のものがあります。
     * <ul>
     * <li>樽型歪みパラメータ - 入力画像の樽型歪みパラメータです。
     * <li>スクリーンサイズ - 入力画像の解像度です。
     * <li>透視変換パラメータ - 4x4行列です。
     * </ul>
     */
    public class NyARParam
    {
        /** スクリーンサイズです。*/
        protected NyARIntSize _screen_size = new NyARIntSize();
        private const int SIZE_OF_PARAM_SET = 4 + 4 + (3 * 4 * 8) + (4 * 8);
        private NyARCameraDistortionFactor _dist = new NyARCameraDistortionFactor();
        private NyARPerspectiveProjectionMatrix _projection_matrix = new NyARPerspectiveProjectionMatrix();
        /**
         * テストに使用するための、カメラパラメータ値をロードします。
         * このパラメータは、ARToolKit2.7に付属しているカメラパラメータファイルの値です。
         */
        public void loadDefaultParameter()
        {
            double[] tmp = { 318.5, 263.5, 26.2, 1.0127565206658486 };
            this._screen_size.setValue(640, 480);
            this._dist.setValue(tmp);
            this._projection_matrix.m00 = 700.9514702992245;
            this._projection_matrix.m01 = 0;
            this._projection_matrix.m02 = 316.5;
            this._projection_matrix.m03 = 0;
            this._projection_matrix.m10 = 0;
            this._projection_matrix.m11 = 726.0941816535367;
            this._projection_matrix.m12 = 241.5;
            this._projection_matrix.m13 = 0.0;
            this._projection_matrix.m20 = 0.0;
            this._projection_matrix.m21 = 0.0;
            this._projection_matrix.m22 = 1.0;
            this._projection_matrix.m23 = 0.0;
            this._projection_matrix.m30 = 0.0;
            this._projection_matrix.m31 = 0.0;
            this._projection_matrix.m32 = 0.0;
            this._projection_matrix.m33 = 1.0;
        }

        public NyARIntSize getScreenSize()
        {
            return this._screen_size;
        }

        /**
         * この関数は、ARToolKit形式の透視変換行列を返します。
         * @return
         * [read only]透視変換行列を返します。
         */
        public NyARPerspectiveProjectionMatrix getPerspectiveProjectionMatrix()
        {
            return this._projection_matrix;
        }
        /**
         * この関数は、ARToolKit形式の歪み補正パラメータを返します。
         * @return
         * [read only]歪み補正パラメータオブジェクト
         */
        public NyARCameraDistortionFactor getDistortionFactor()
        {
            return this._dist;
        }
        /**
         * この関数は、配列から値を設定します。
         * @param i_factor
         * NyARCameraDistortionFactorにセットする配列を指定する。要素数は4であること。
         * @param i_projection
         * NyARPerspectiveProjectionMatrixセットする配列を指定する。要素数は12であること。
         */
        public void setValue(double[] i_factor, double[] i_projection)
        {
            this._dist.setValue(i_factor);
            this._projection_matrix.setValue(i_projection);
            return;
        }
        /**
         * この関数は、現在のスクリーンサイズを変更します。
         * ARToolKitのarParamChangeSize関数に相当します。
         * @param i_xsize
         * 新しいサイズ
         * @param i_ysize
         * 新しいサイズ
         */
        public void changeScreenSize(int i_xsize, int i_ysize)
        {
            double scale = (double)i_xsize / (double)(this._screen_size.w);// scale = (double)xsize / (double)(source->xsize);
            //スケールを変更
            this._dist.changeScale(scale);
            this._projection_matrix.changeScale(scale);
            this._screen_size.w = i_xsize;// newparam->xsize = xsize;
            this._screen_size.h = i_ysize;// newparam->ysize = ysize;
            return;
        }
        public void changeScreenSize(NyARIntSize i_size)
        {
            this.changeScreenSize(i_size.w, i_size.w);
            return;
        }
        /**
         * この関数は、カメラパラメータから右手系の視錐台を作ります。
         * <p>注意 -
         * この処理は低速です。繰り返しの使用はできるだけ避けてください。
         * </p>
         * @param i_dist_min
         * 視錐台のnear point(mm指定)
         * @param i_dist_max
         * 視錐台のfar point(mm指定)
         * @param o_frustum
         * 視錐台を受け取る配列。
         * @see NyARPerspectiveProjectionMatrix#makeCameraFrustumRH
         */
        public void makeCameraFrustumRH(double i_dist_min, double i_dist_max, NyARDoubleMatrix44 o_frustum)
        {
            this._projection_matrix.makeCameraFrustumRH(this._screen_size.w, this._screen_size.h, i_dist_min, i_dist_max, o_frustum);
            return;
        }

        public void loadARParam(StreamReader i_stream)
        {
            this.loadARParam(new BinaryReader(i_stream.BaseStream));
        }
        /**
         * この関数は、ストリームからARToolKit形式のカメラパラメーを1個目の設定をロードします。
         * @param i_stream
         * 読み込むストリームです。
         * @throws Exception
         */
        public void loadARParam(BinaryReader i_reader)
        {
            try
            {
                byte[] buf = new byte[SIZE_OF_PARAM_SET];
                double[] tmp = new double[16];

                // バッファを加工
                this._screen_size.w = endianConv(i_reader.ReadInt32());
                this._screen_size.h = endianConv(i_reader.ReadInt32());
                //double値を12個読み込む
                for (int i = 0; i < 12; i++)
                {
                    tmp[i] = endianConv(i_reader.ReadDouble());
                }
                //パディング
                tmp[12] = tmp[13] = tmp[14] = 0;
                tmp[15] = 1;
                //Projectionオブジェクトにセット
                this._projection_matrix.setValue(tmp);
                //double値を4個読み込む
                for (int i = 0; i < 4; i++)
                {
                    tmp[i] = endianConv(i_reader.ReadDouble());
                }
                //Factorオブジェクトにセット
                this._dist.setValue(tmp);
            }
            catch (Exception e)
            {
                throw new NyARException(e);
            }
            return;
        }
        /**
         * この関数は機能しません。
         * @param i_stream
         * 未定義
         * @throws Exception
         */
        public void saveARParam(StreamWriter i_stream)
        {
            NyARException.trap("未チェックの関数");
            /*            byte[] buf = new byte[SIZE_OF_PARAM_SET];
                        // バッファをラップ
                        ByteBuffer bb = ByteBuffer.wrap(buf);
                        bb.order(ByteOrder.BIG_ENDIAN);

                        // 書き込み
                        bb.putInt(this._screen_size.w);
                        bb.putInt(this._screen_size.h);
                        double[] tmp = new double[12];
                        //Projectionを読み出し
                        this._projection_matrix.getValue(tmp);
                        //double値を12個書き込む
                        for (int i = 0; i < 12; i++)
                        {
                            tmp[i] = bb.getDouble();
                        }
                        //Factorを読み出し
                        this._dist.getValue(tmp);
                        //double値を4個書き込む
                        for (int i = 0; i < 4; i++)
                        {
                            tmp[i] = bb.getDouble();
                        }
                        i_stream.write(buf);
                        return;
             */
        }
        private static double endianConv(double i_val)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return i_val;
            }
            byte[] ba = BitConverter.GetBytes(i_val);
            Array.Reverse(ba);
            return BitConverter.ToDouble(ba, 0);
        }
        private static int endianConv(int i_val)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return i_val;
            }
            byte[] ba = BitConverter.GetBytes(i_val);
            Array.Reverse(ba);
            return BitConverter.ToInt32(ba, 0);
        }
    }
}
