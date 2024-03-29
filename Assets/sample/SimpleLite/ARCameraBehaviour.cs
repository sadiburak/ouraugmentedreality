using UnityEngine;
using System;
using System.Collections;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;
using NyARUnityUtils;
using System.IO;

/// <summary>
/// AR camera behaviour.
/// This sample shows simpleLite demo.
/// 1.Connect webcam to your computer.
/// 2.Start sample program
/// 3.Take a "HIRO" marker on capture image
/// 
/// </summary>
public class ARCameraBehaviour : MonoBehaviour
{
	private NyARUnityMarkerSystem _ms;
	private NyARUnityWebCam _ss;
	private int midHiro;//marker id of Hiro
	private int midKanji;//marker id of Kanji
	private GameObject _bg_panel;
	private DateTime time;
	private int fps = 0;
	private int last_c = 0;
	
	
	// Use this for initialization
	void Awake()
	{
		//setup unity webcam
		WebCamDevice[] devices= WebCamTexture.devices;
		WebCamTexture w;
		
		if (devices.Length > 0){
			w=new WebCamTexture(320, 240, 15);
			this._ss=new NyARUnityWebCam(w);
			
			
			NyARMarkerSystemConfig config = new NyARMarkerSystemConfig(w.requestedWidth,w.requestedHeight);
			
			this._ms=new NyARUnityMarkerSystem(config);
			//mid=this._ms.addARMarker("./Assets/Data/patt.hiro",16,25,80);
			//This line loads a marker from texture
			midHiro=this._ms.addARMarker((Texture2D)(Resources.Load("MarkerHiro", typeof(Texture2D))),16,25,80);
			midKanji=this._ms.addARMarker((Texture2D)(Resources.Load("MarkerKanji", typeof(Texture2D))),16,25,80);

			//setup background
			this._bg_panel=GameObject.Find("Plane");
			this._bg_panel.renderer.material.mainTexture=w;
			this._ms.setARBackgroundTransform(this._bg_panel.transform);
			
			//setup camera projection
			this._ms.setARCameraProjection(this.camera);
			
		}else{
			Debug.LogError("No Webcam.");
		}
	}
	// Use this for starting
	void Start ()
	{
		this.time = DateTime.Now;
		//start sensor
		this._ss.start();
	}
	// Update is called once per frame
	void Update ()
	{
		
		if( DateTime.Now.Second != this.time.Second)
		{
			this.time = DateTime.Now;
			this.fps = c - last_c;
			this.last_c = c;
		}
		
		this.time = DateTime.Now;
		//Update SensourSystem
		this._ss.update();
		//Update marker system by ss
		this._ms.update(this._ss);
		//update Gameobject transform
		int found = 0;
		if(this._ms.isExistMarker(midHiro) ){
			this._ms.setMarkerTransform(midHiro,GameObject.Find("MarkerObject").transform);
			//Debug.Log(c+":"+this._ms.getConfidence(midHiro));
			
		}else
		{
			GameObject.Find("MarkerObject").transform.localPosition=new Vector3(0,0,-100);
		}
		if(this._ms.isExistMarker(midKanji))
		{
			this._ms.setMarkerTransform(midKanji,GameObject.Find("MarkerObject2").transform);
			//Debug.Log(c+":"+this._ms.getConfidence(midKanji));
			
		}
		else
		{
			GameObject.Find("MarkerObject2").transform.localPosition=new Vector3(0,0,-100);
		}
		c++;
	}
	static int c=0;
}
