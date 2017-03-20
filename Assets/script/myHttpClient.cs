using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;

public class myHttpClient : MonoBehaviour {

	// デリゲートの定義（接続完了後のコールバック呼び出し用）
	// tagは、ハンドラがなんのHTTP通信の完了を受け取ったか判断するのに用いる
	// 任意の文字列。必要なければ無視してください。

	// GET(tagなし)
	public delegate void OnCompletionGet(UnityWebRequest request, string siteName);
	public delegate void OnErrorGet(UnityWebRequest request, string siteName);

	// GET(tagなし)の開始処理
	public void StartGet (OnCompletionGet completion,
		OnErrorGet errorHandler,
		string url,
		string siteName)
	{
		Debug.Log ("start coroutine " + siteName);
		StartCoroutine(HttpGetCoroutine(
			completion,
			errorHandler,
			url,
			siteName
		));
	}

	// GETを実際に処理してるコルーチン
	private IEnumerator HttpGetCoroutine (
		OnCompletionGet successFunction,
		OnErrorGet errorFunction,
		string url,
		string siteName)
	{
		Debug.Log ("start coroutine in " + siteName);
		UnityWebRequest request = UnityWebRequest.Get (url);

		// リクエスト送信
		yield return request.Send ();

		// 通信エラーチェック
		if (request.isError) {
			//エラー処理
			errorFunction (request,siteName);
		} else {
			if (request.responseCode == 200) {
				// UTF8文字列として取得する
				string text = request.downloadHandler.text;
				//接続成功
				successFunction (request,siteName);
			}
		}
	}
}
