using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class sqliteTest : MonoBehaviour {

	// Use this for initialization
	static private string logs="";
	void Start () {
		Application.logMessageReceived += logCallback;
		StartCoroutine("test");
	}

	static void logCallback(string condition, string stackTrace, LogType type)
	{
		if (type > LogType.Warning) return;
		string msg = "";
		msg = string.Format("{0}{1}\n{2}", msg, condition, stackTrace);
		log(msg);
	}


	static void log(string s)
	{
		logs += s + "\n";
		Debug.Log(s);
	}
	IEnumerator test() {
		var store = new sqlite3.Store();
		var pf = Application.platform;
		string dbPath = Application.streamingAssetsPath + "/res.db";
		if (pf == RuntimePlatform.Android)
		{
			var source = dbPath;

			dbPath = Application.persistentDataPath + "/res.db";
			//if (!File.Exists(dbPath))
			{
				log("copy: " + source + " to " + dbPath);
				var www = new WWW(source);
				yield return www;
				if (www.isDone)
				{
					log("res.db:" + www.size.ToString());
					File.WriteAllBytes(dbPath, www.bytes);
				}
				else
				{
					log("error:" + www.error);
					yield break;
				}
			}
		}

		log("**********begin read db:" + dbPath);
		var err = store.Connect(dbPath, "1234567890123456");
		if (err != "")
		{
			log("!!!!!error:" + err);
			yield break;
		}

		for (var i = 0; i <= 3; i++)
		{
			var values = store.LoadFields("Role", "id=" + i.ToString(), "id", "Name", "RoleType");
			var s = string.Format("\t{0}\t{1}\t{2}", values[0], values[1], values[2]);
			log(s);
		}
		log("**********end read db value ***********");
	}

	void Update()
	{
		GetComponent<Text>().text = logs;
	}
}
