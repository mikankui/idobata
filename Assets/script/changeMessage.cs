using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using UnityEngine.UI;
using System.ServiceModel.Syndication;

public class changeMessage : MonoBehaviour {

	//GameObject CountTex;
	Queue<NewsBody> NewsQueue = new Queue<NewsBody>();
	Coroutine retC;
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
		Time.timeScale = 0.1f;
		retC = StartCoroutine (waitingNextNews());

	}

	// Update is called once per frame
	void Update () {

		//キューにニュースがない場合、ニュースを取得
		try {
			if(NewsQueue.Count < 2){
				var results = GetFromYahoo();
				foreach (var nb in results){
					NewsQueue.Enqueue(nb);
				}
			}

		}
		catch( Exception ex ) {
			//CountTex.GetComponent<TextMesh> ().text = "--エラー--";
			Debug.Log( $"エラー : {ex.Message}" );
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
			yield return new WaitForSecondsRealtime (2.0f);
			//ニュースの更新
			NewsBody nb = NewsQueue.Dequeue ();
			title.GetComponent<UnityEngine.UI.Text>().text = nb.Title;  
			body.GetComponent<UnityEngine.UI.Text>().text = nb.Body;
			link.GetComponent<UnityEngine.UI.Text>().text = nb.Link;
			siteName.GetComponent<UnityEngine.UI.Text>().text = nb.SiteName;

		}
	}

	IEnumerable<NewsBody> GetFromYahoo() {
		List<string> siteList = new List<string> ();

		//プロトコル違い
		//siteList.Add ("feed://www3.nhk.or.jp/rss/news/cat0.xml");

		//文字コードがSJIS
		//siteList.Add("http://feed.rssad.jp/rss/news24/index.rdf");

		//アルファ 解析がうまくいっていない？
		//siteList.Add ("http://alfalfalfa.com/index.rdf");

		//はてブ 解析がうまくいっていない？
		//http://sprint-life.hatenablog.com/entry/2014/01/15/203535
		siteList.Add ("http://b.hatena.ne.jp/entrylist.rss");

		//

		//siteList.Add ("http://news.yahoo.co.jp/pickup/world/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/domestic/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/economy/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/entertainment/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/sports/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/computer/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/science/rss.xml");
		//siteList.Add ("http://news.yahoo.co.jp/pickup/local/rss.xml");

		foreach (var site in siteList) {
			//var results = GetRSSstring (site);

			//foreach (string r in results) {
			using (XmlReader rdr = XmlReader.Create(site))
			{
				SyndicationFeed feed = SyndicationFeed.Load(rdr);

				//XDocument xdoc = XDocument.Parse(r);
				// 子要素を取得
				//var items = xdoc.Root.Descendants("item");
				//foreach (XElement item in items) {
				//	string title = item.Element ("title").Value;
				//	if (title.StartsWith("[PR]") == false) {
				//		string link = item.Element("link").Value;
				//		string  discription = item.Element("title").Value;
				//		yield return new NewsBody(title,discription,link,"Yahoo!");
				//	}
				//}
				foreach (SyndicationItem item in feed.Items) {
					string title =  item.Title.Text;
					string discription =  item.Summary.Text;
					string link =(item.Links.Count > 0
						? item.Links[0].Uri.AbsolutePath : "");

					yield return new NewsBody(title,discription,link,"");
				}
			}
		}
	}

	IEnumerable GetRSSstring(string site) {
		WebClient wc = new WebClient();
		wc.Encoding = System.Text.Encoding.UTF8;

		Uri url = new Uri(site);
		string result = "";
		//CountTex.GetComponent<TextMesh> ().text = site;
		try{
			result = wc.DownloadString(url);
			Debug.Log( result );
		}catch( Exception ex ) {
			//CountTex.GetComponent<TextMesh> ().text = ex.Message;
			Debug.Log( $"エラー : {ex.Message}" );
		}
		yield return result;
	}
	//IEnumerator WaitForRequest(string url)
	//{
	//
	//	UnityWebRequest www = UnityWebRequest.Get(url);
	//	www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
	//	www.SetRequestHeader("Pragma", "no-cache");
	//	www.SetRequestHeader("Accept-Language", "ja");
	//	www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
	//
	//	yield return www.Send();
	//	if (www.isError)
	//	{
	//		CountTex.GetComponent<TextMesh> ().text = www.error;
	//		Debug.Log(www.error);
	//	}
	//	else
	//	{
	//		if (www.responseCode == 200) {
	//			// UTF8文字列として取得する
	//			//Debug.Log(www.responseCode.ToString() + ":" + www.downloadHandler.text);
	//		}
	//	}
	//}
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