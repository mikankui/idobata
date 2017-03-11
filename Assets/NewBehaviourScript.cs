﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Xml.Linq;

public class NewBehaviourScript : MonoBehaviour {

	GameObject CountTex;
	Queue<NewsBody> NewsQueue = new Queue<NewsBody>();
	Coroutine retC;

	// Use this for initialization
	void Start () {

		//start 
		Debug.Log("start!!");
		// MESSAGE表示フィールドを取得
		CountTex = GameObject.Find ("message");
		// ニュース自動切り替え
		Time.timeScale = 0.1f;
		retC = StartCoroutine (waitingNextNews());

	}
	
	// Update is called once per frame
	void Update () {

		//キューにニュースがない場合、ニュースを取得
		try {
			if(NewsQueue.Count < 2){
				var results = GetWeatherReportFromYahoo();
				foreach (var s in results){
					NewsQueue.Enqueue(s);
				}
			}
		}
		catch( Exception ex ) {
			Console.WriteLine( $"エラー : {ex.Message}" );
		}

		TouchInfo info = AppUtil.GetTouch();
		switch(info){
		case TouchInfo.Began:
			break;
		case TouchInfo.Moved:
			//aaVector3 delta = AppUtil.GetDeltaPosition();
			//float xMove = delta.x * speed *10;
			//float yMove = delta.y * speed *10;

			//float xAngle = yMove;
			//float yAngle = -xMove;

			// ターゲットのGameObjectを回転させたりするつもりのコード
			//target.Rotate(xAngle, yAngle, 0.0f, Space.World);
			break;
		case TouchInfo.Ended:

			//ニュースの更新
			NewsBody nb = NewsQueue.Dequeue();
			CountTex.GetComponent<TextMesh> ().text = nb.Body;
			//自動切り替え時間のリセット
			StopCoroutine(retC);
			retC = StartCoroutine (waitingNextNews());

			break;
		}
	}

	IEnumerator waitingNextNews(){
		
		while (true) {
			yield return new WaitForSecondsRealtime (2.0f);
			//ニュースの更新
			NewsBody nb = NewsQueue.Dequeue ();
			CountTex.GetComponent<TextMesh> ().text = nb.Body;
		}
	}

	private static IEnumerable<NewsBody> GetWeatherReportFromYahoo() {
		List<string> siteList = new List<string> ();

		siteList.Add ("http://news.yahoo.co.jp/pickup/world/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/domestic/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/economy/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/entertainment/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/sports/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/computer/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/science/rss.xml");
		siteList.Add ("http://news.yahoo.co.jp/pickup/local/rss.xml");


		WebClient wc = new WebClient();
		wc.Encoding = System.Text.Encoding.UTF8;
		wc.Headers.Add("Content-type", "charset=UTF-8");

		foreach (var site in siteList) {
			
			Uri url = new Uri(site);
			string result = wc.DownloadString(url);
			XDocument xdoc = XDocument.Parse(result);
			var nodes = xdoc.Root.Descendants("title");
			foreach (var node in nodes) {
				if (node.Value.StartsWith("[PR]") == false) {
					string s = node.Value.Replace(" - Yahoo!天気・災害", "");
					yield return new NewsBody("",s,site,"Yahoo");
				}
			}
		}

	}
}

class NewsBody
{
	private string title = "";
	private string body = "";
	private string url="";
	private string siteName="";

	public string Title
	{
		get {return this.title;}
	}
	public string Body
	{
		get {return this.body;}
	}
	public string Url
	{
		get {return this.url;}
	}
	public string SiteName
	{
		get {return this.siteName;}
	}

	public NewsBody(string title, string body, string url, string siteName)
	{
		this.title = title;
		this.body = body;
		this.url = url;
		this.siteName = siteName;
	}
	public void Print()
	{
		Console.WriteLine("{0} : {1} : {2} :{3}",title,body,url,siteName);
	}
}

//http://qiita.com/JunSuzukiJapan/items/931776ecc2a545b87045#_reference-ab0e42d1db2ce02200bb
//
//public float speed = 1.0f;
//
//void Update () {
//	TouchInfo info = AppUtil.GetTouch();
//	switch(info){
//	case TouchInfo.Began:
//		break;
//	case TouchInfo.Moved:
//		Vector3 delta = AppUtil.GetDeltaPosition();
//		float xMove = delta.x * speed *10;
//		float yMove = delta.y * speed *10;
//
//		float xAngle = yMove;
//		float yAngle = -xMove;
//
//		// ターゲットのGameObjectを回転させたりするつもりのコード
//		target.Rotate(xAngle, yAngle, 0.0f, Space.World);
//		break;
//	case TouchInfo.Ended:
//		break;
//	}
//}

public static class AppUtil {
	private static Vector3 TouchPosition = Vector3.zero;
	private static Vector3 PreviousPosition = Vector3.zero;

	/// <summary>
	/// タッチ情報を取得(エディタと実機を考慮)
	/// </summary>
	/// <returns>タッチ情報。タッチされていない場合は null</returns>
	public static TouchInfo GetTouch() {
		if (Application.isEditor) {
			if (Input.GetMouseButtonDown(0)) { return TouchInfo.Began; }
			if (Input.GetMouseButton(0))     { return TouchInfo.Moved; }
			if (Input.GetMouseButtonUp(0))   { return TouchInfo.Ended; }
		} else {
			if (Input.touchCount > 0) {
				return (TouchInfo)((int)Input.GetTouch(0).phase);
			}
		}
		return TouchInfo.None;
	}

	/// <summary>
	/// タッチポジションを取得(エディタと実機を考慮)
	/// </summary>
	/// <returns>タッチポジション。タッチされていない場合は (0, 0, 0)</returns>
	public static Vector3 GetTouchPosition() {
		if (Application.isEditor){
			TouchInfo touch = AppUtil.GetTouch();
			if (touch != TouchInfo.None) {
				PreviousPosition = Input.mousePosition;
				return PreviousPosition;
			}
		} else {
			if (Input.touchCount > 0) {
				Touch touch = Input.GetTouch(0);
				TouchPosition.x = touch.position.x;
				TouchPosition.y = touch.position.y;
				return TouchPosition;
			}
		}
		return Vector3.zero;
	}

	public static Vector3 GetDeltaPosition(){
		if(Application.isEditor){
			TouchInfo info = AppUtil.GetTouch();
			if(info != TouchInfo.None){
				Vector3 currentPosition = Input.mousePosition;
				Vector3 delta = currentPosition - PreviousPosition;
				PreviousPosition = currentPosition;
				return delta;
			}
		}else{
			if(Input.touchCount > 0){
				Touch touch = Input.GetTouch(0);
				PreviousPosition.x = touch.deltaPosition.x;
				PreviousPosition.y = touch.deltaPosition.y;
				return PreviousPosition;
			}
		}
		return Vector3.zero;
	}

	/// <summary>
	/// タッチワールドポジションを取得(エディタと実機を考慮)
	/// </summary>
	/// <param name='camera'>カメラ</param>
	/// <returns>タッチワールドポジション。タッチされていない場合は (0, 0, 0)</returns>
	public static Vector3 GetTouchWorldPosition(Camera camera) {
		return camera.ScreenToWorldPoint(GetTouchPosition());
	}
}

/// <summary>
/// タッチ情報。UnityEngine.TouchPhase に None の情報を追加拡張。
/// </summary>
public enum TouchInfo {
	/// <summary>
	/// タッチなし
	/// </summary>
	None = -1,

	// 以下は UnityEngine.TouchPhase の値に対応
	/// <summary>
	/// タッチ開始
	/// </summary>
	Began = 0,
	/// <summary>
	/// タッチ移動
	/// </summary>
	Moved = 1,
	/// <summary>
	/// タッチ静止
	/// </summary>
	Stationary = 2,
	/// <summary>
	/// タッチ終了
	/// </summary>
	Ended = 3,
	/// <summary>
	/// タッチキャンセル
	/// </summary>
	Canceled = 4,
}