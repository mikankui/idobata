using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class changeMessage : MonoBehaviour {

	//GameObject CountTex;
	Queue<NewsBody> NewsQueue = new Queue<NewsBody>();
	Coroutine retC;
	Dictionary<string,string> RLRdict;
	RssListReader RLR;
	public GameObject title;
	public GameObject body;
	public GameObject link;
	public GameObject siteName;

	// Use this for initialization
	void Start () {

		//start 
		Debug.Log("start!!");
		// MESSAGE表示フィールドを取得
		//CountTex = GameObject.Find ("message");
		// ニュース自動切り替え
		Time.timeScale = 0.5f;
		retC = StartCoroutine (waitingNextNews());
		RssListReader RLR = new RssListReader ();
		RLRdict = RLR.ReadList ();
	}

	// Update is called once per frame
	void Update () {

		//キューにニュースがない場合、ニュースを取得
		try {
			if(NewsQueue.Count < 10 && RLRdict.Any()){
				//GetRSS();
			}
		}
		catch( Exception ex ) {
			Debug.Log( $"エラー : {ex.Message}" );
		}

		TouchInfo info = AppUtil.GetTouch();
		switch(info){
		case TouchInfo.Began:
			if(NewsQueue.Count < 10 && RLRdict.Any()){
				GetRSS();
			}
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
			title.GetComponent<UnityEngine.UI.Text>().text = nb.Title;  
			body.GetComponent<UnityEngine.UI.Text>().text = nb.Body;
			link.GetComponent<UnityEngine.UI.Text>().text = nb.Link;
			siteName.GetComponent<UnityEngine.UI.Text>().text = nb.SiteName;
			//自動切り替え時間のリセット
			StopCoroutine(retC);
			retC = StartCoroutine (waitingNextNews());

			break;
		}
	}

	IEnumerator waitingNextNews(){
		while (true) {
			yield return new WaitForSecondsRealtime (4.0f);
			//ニュースの更新
			NewsBody nb = NewsQueue.Dequeue ();
			title.GetComponent<UnityEngine.UI.Text>().text = nb.Title;  
			body.GetComponent<UnityEngine.UI.Text>().text = nb.Body;
			link.GetComponent<UnityEngine.UI.Text>().text = nb.Link;
			siteName.GetComponent<UnityEngine.UI.Text>().text = nb.SiteName;
		}
	}

	void GetRSS() {
		//List<string> siteList = new List<string> ();
		List<NewsBody> newsBody = new List<NewsBody> ();
		var site = RLRdict.First();
		RLRdict.Remove(site.Key);
		Debug.Log ("GET RSS START [{0}]",site.Key);
		int itemCount = 0;

		try {

			// リクエスト送信
			WwwClient client = new WwwClient();
			var result = client.GetSample(site.Value);

			// 呼び出し後、結果を受けてif/switchで処理を分ける。
			if (result.IsSuccess == true)
			{
				var xmldoc = XDocument.Parse(result.Html);
				var spxItems = xmldoc.Root.Descendants("item");

				itemCount = spxItems.Count();
				foreach( var item in spxItems ) {
					string title = TryGetElementValue(item,"title");
					string discription = TryGetElementValue(item,"description" );
					//string pubDate = TryGetElementValue(item,"pubDate" );
					string link = TryGetElementValue(item,"link" );
					newsBody.Add(new NewsBody(title,discription,link,site.Key));
				}

				Debug.Log(string.Format("Key : {0} / COUNT {1}", site.Key, itemCount));

				foreach (var nb in newsBody){
					NewsQueue.Enqueue(nb);
				}
			}
			else
			{
				Debug.Log("失敗時の処理");
				// ※ 「サーバーとの接続に失敗しました。時間をおいてリトライしてください」的なメッセージを表示する処理を入れる 
			}


		}
		catch( Exception ex ) {
			Console.WriteLine( $"エラー : {ex.Message}" );
		}
			
	}

	string TryGetElementValue(XElement parentEl, string elementName, string defaultValue = null) 
	{
		var foundEl = parentEl.Element(elementName);

		if (foundEl != null)
		{
			return foundEl.Value;
		}

		return defaultValue;
	}
		
}

public class RssListReader{
	public Dictionary<string, string> ReadList()
	{
		// Read each line of the file into a string array. Each element
		// of the array is one line of the file.
		string[] lines = System.IO.File.ReadAllLines(@"Assets/script/rsslist.txt");
		Dictionary<string, string> links = new Dictionary<string, string>();

		// Display the file contents by using a foreach loop.
		foreach (string line in lines)
		{
			// Use a tab to indent each line of the file.
			if(! line.StartsWith("#")){
			    var link = line.Split(',');
				links.Add (link [0].Trim(new char[] { '"' }), link [1].Trim(new char[] { '"' }));
				//Debug.Log("["+link [0] +"]["+ link [1]+"]");
			}
		}
		Debug.Log("LINKS COUNT:"+links.Count);
		return links;
	}
}

class NewsBody
{
	private string title = "";
	private string body = "";
	private string link="";
	private string siteName="";

	public string Title
	{
		get {return this.title;}
	}
	public string Body
	{
		get {return this.body;}
	}
	public string Link
	{
		get {return this.link;}
	}
	public string SiteName
	{
		get {return this.siteName;}
	}

	public NewsBody(string title, string body, string link, string siteName)
	{
		this.title = title;
		this.body = body;
		this.link = link;
		this.siteName = siteName;
	}
	public void Print()
	{
		//Debug.Log("{0} : {1} : {2} :{3}",title,body,url,siteName);
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

////
/// 


public class WwwClient
{
	public WwwClientGetResult GetSample(string url)
	{
		var result = new WwwClientGetResult();

		try
		{
			// 接続に失敗するとWebExceptionが飛ぶ
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream stream = response.GetResponseStream();
			var html = new StreamReader(stream).ReadToEnd();

			result.IsSuccess = true;
			result.Html = html;
			return result;
		}
		catch (WebException e)
		{
			// 失敗したらここでキャッチして呼び出し元にExceptionが伝播しないようにする。
			result.IsSuccess = false;
			result.Html = "";
			Debug.Log (e.Message);
			return result;
		}
	}
}

public class WwwClientGetResult
{
	public bool IsSuccess;
	public string Html;
}