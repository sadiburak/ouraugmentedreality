using UnityEngine;
using System;
using System.Collections;
using NyARUnityUtils;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;

namespace NyARUnityUtils
{
	public class NyARUnityMarkerSystem:NyARMarkerSystem
	{
		private Matrix4x4 _projection_mat;	
	
		public NyARUnityMarkerSystem(INyARMarkerSystemConfig i_config):base(i_config)
		{
		}
		
		
		protected override void initInstance(INyARMarkerSystemConfig i_config)
		{
			base.initInstance(i_config);		
			this._projection_mat=new Matrix4x4();
		}

		
		public Matrix4x4 getUnityProjectionMatrix()
		{
			return this._projection_mat;
		}
		
		
		public override void setProjectionMatrixClipping(double i_near,double i_far)
		{
			base.setProjectionMatrixClipping(i_near,i_far);
			NyARUnityUtil.toCameraFrustumRH(this._ref_param,1,i_near,i_far,ref this._projection_mat);
			
		}
		/** <summary>
		* Gets the Unity form marker matrix.
		* </summary>
		* <returns>
		* The marker matrix.
		* </returns>
		* <param name='i_id'>
		* I_id.
		* </param>
		* <param name='i_buf'>
		* I_buf.
		* </param>
		**/
		public void getMarkerMatrix(int i_id,ref Matrix4x4 i_buf)
		{
			NyARUnityUtil.toCameraViewRH(base.getMarkerMatrix(i_id),1,ref i_buf);
		}
		
		public Matrix4x4 getUnityMarkerMatrix(int i_id)
		{
			Matrix4x4 buf=new Matrix4x4();
			NyARUnityUtil.toCameraViewRH(base.getMarkerMatrix(i_id),1,ref buf);
			return buf;
		}

		public void getMarkerPlanePos(int id,int i_x,int i_y,ref Vector3 i_out)
		{
			NyARDoublePoint3d p=new NyARDoublePoint3d();
			this.getMarkerPlanePos(id,i_x,i_y,p);
			i_out.x=-(float)p.x;
			i_out.y=(float)p.y;
			i_out.z=(float)p.z;
		}
		
		
        /** <summary>
        * {@link #addARMarker(INyARRgbRaster, int, int, double)}It is a wrapper. I make a marker pattern from Bitmap.
        * The arguments are{@link #addARMarker(INyARRgbRaster, int, int, double)}Please refer to the.
        * 
        * </summary>
        * <param name="i_img"></param>
        * <param name="i_patt_resolution">I specify the resolution of the marker to be generated.</param>
        * <param name="i_patt_edge_percentage">Specifies the percentage of the edge region of the image.</param>
        * <param name="i_marker_size">I specify the physical size of the marker.</param>
        * <returns></returns>
		**/
        public int addARMarker(Texture2D i_img, int i_patt_resolution, int i_patt_edge_percentage, double i_marker_size)
        {
            int w = i_img.width;
            int h = i_img.height;
            NyARUnityRaster ur = new NyARUnityRaster(i_img);
			NyARCode c = new NyARCode(i_patt_resolution, i_patt_resolution);
            //Marker pattern cut out from a raster
            INyARPerspectiveCopy pc = (INyARPerspectiveCopy)ur.createInterface(typeof(INyARPerspectiveCopy));
            NyARRgbRaster tr = new NyARRgbRaster(i_patt_resolution, i_patt_resolution);
            pc.copyPatt(0, 0, w, 0, w, h, 0, h, i_patt_edge_percentage, i_patt_edge_percentage, 4, tr);
            //Set the pattern cut
            c.setRaster(tr);
            return base.addARMarker(c, i_patt_edge_percentage, i_marker_size);
        }
        /** <summary>
        * This function is{@link #getMarkerPlaneImage(int, NyARSensor, int, int, int, int, int, int, int, int, INyARRgbRaster)}
        * It is a wrapper. The acquired image{@link #BufferedImage}I returned in the form.
        * </summary>
        * <param name="i_id"></param>
        * <param name="i_sensor"></param>
        * <param name="i_x1"></param>
        * <param name="i_y1"></param>
        * <param name="i_x2"></param>
        * <param name="i_y2"></param>
        * <param name="i_x3"></param>
        * <param name="i_y3"></param>
        * <param name="i_x4"></param>
        * <param name="i_y4"></param>
        * <param name="i_img"></param>
        * <returns></returns>
		**/
		
        public void getMarkerPlaneImage(int i_id, NyARSensor i_sensor, int i_x1, int i_y1, int i_x2, int i_y2, int i_x3, int i_y3, int i_x4, int i_y4, Texture2D i_img)
        {
            NyARUnityRaster bmr = new NyARUnityRaster(i_img);
            base.getMarkerPlaneImage(i_id, i_sensor, i_x1, i_y1, i_x2, i_y2, i_x3, i_y3, i_x4, i_y4, bmr);
            return;
        }
        /**
         * This function is{@link #getMarkerPlaneImage(int, NyARSensor, int, int, int, int, INyARRgbRaster)}
         * It is a wrapper. The acquired image{@link #BufferedImage}I returned in the form.
         * @param i_id
         * marker id
         * @param i_sensor
         * Object to retrieve the image sensor. Is usually{@link #update(NyARSensor)}I will be the same as it was entered into the function.
         * @param i_l
         * @param i_t
         * @param i_w
         * @param i_h
         * @param i_raster
         * Object where the output
         * @return
         * I have to store the result i_raster Object
         * @throws NyARException
         */
        public void getMarkerPlaneImage(int i_id, NyARSensor i_sensor, int i_l, int i_t, int i_w, int i_h, Texture2D i_img)
        {
            NyARUnityRaster bmr = new NyARUnityRaster(i_img.width,i_img.height,true);
            base.getMarkerPlaneImage(i_id, i_sensor, i_l, i_t, i_w, i_h, bmr);
			i_img.SetPixels32((Color32[])bmr.getBuffer());
			i_img.Apply();
			
            return;
        }		
		
		
		/** <summary>
		* This function is camera To object ProjectionMatrix I will specify.
		* </summary>
		**/
		public void setARCameraProjection(Camera i_camera)
		{
			NyARFrustum f=this.getFrustum();
			NyARFrustum.PerspectiveParam pp=f.getPerspectiveParam(new NyARFrustum.PerspectiveParam());
			//setup camera projection
			i_camera.nearClipPlane=(float)pp.near;
			i_camera.farClipPlane=(float)pp.far;
			i_camera.fieldOfView=(float)(360*pp.fovy/(2*Math.PI));
			i_camera.aspect=(float)(pp.aspect);
			i_camera.transform.LookAt(new Vector3(0,0,0),new Vector3(1,0,0));
		}
		/** <summary>
		* This function sets the position of the background image transform matrix members.
		* </summary>
		**/
		public void setARBackgroundTransform(Transform i_transform)
		{
			NyARFrustum f=this.getFrustum();
			NyARFrustum.FrustumParam fp=f.getFrustumParam(new NyARFrustum.FrustumParam());
			float bg_pos=(float)fp.far;
			i_transform.position=new Vector3(0,0,(float)bg_pos);
			double b=bg_pos/fp.near/10;// 10?
			i_transform.localScale=new Vector3((float)(-(fp.right-fp.left)*b),1f,-(float)((fp.top-fp.bottom)*b));
			i_transform.eulerAngles=new Vector3(-90,0,0);
		}
			
		/** <summary>
		* Gets the unity marker transform rotation and position.
		* </summary>
		* <param name='i_id'>
		* I_id.
		* </param>
		* <param name='o_pos'>
		* O_pos.
		* </param>
		* <param name='o_rotation'>
		* O_rotation.
		* </param>
		**/
		public void getMarkerTransform(int i_id,ref Vector3 o_pos,ref Quaternion o_rotation)
		{
			NyARUnityUtil.toCameraViewRH(this.getMarkerMatrix(i_id),1,ref o_pos,ref o_rotation);
		}
		
		/** <summary>
		* Sets marker matrix to unity transform
		* </summary>
		* <param name='i_id'>
		* I_id.
		* </param>
		* <param name='i_t'>
		* I_t.
		* </param>
		**/
		public void setMarkerTransform(int i_id,Transform i_t)
		{
			Vector3 p=new Vector3();
			Quaternion r=new Quaternion();
			NyARUnityUtil.toCameraViewRH(this.getMarkerMatrix(i_id),1,ref p,ref r);
			i_t.localPosition=p;
			i_t.localRotation=r;
		}		
		public void setMarkerTransform(int i_id,GameObject i_go)
		{
			this.setMarkerTransform(i_id,i_go.transform);
		}
	}
}

