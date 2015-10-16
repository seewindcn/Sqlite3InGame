using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class sqliteTest : MonoBehaviour {

	// Use this for initialization
	private string logs="";
	void Start () {
		test();
	}

	void log(string s)
	{
		logs += s;
		Debug.Log(s.TrimEnd('\n'));
	}
	void test() {
		var store = new sqlite3.Store();
		var pf = Application.platform;
		string dbPath = Application.streamingAssetsPath + "/res.db";
		if (pf == RuntimePlatform.Android)
		{
			dbPath = Application.dataPath + "/res.db";
		}
		log("**********begin read db:" + dbPath);
		var err = store.Connect(dbPath, "1234567890123456");
		if (err != "")
		{
			log("!!!!!error:" + err);
			return;
		}

		for (var i = 0; i <= 3; i++)
		{
			var values = store.LoadFields("Role", "id=" + i.ToString(), "id", "Name", "RoleType");
			var s = string.Format("\t{0}\t{1}\t{2}\n", values[0], values[1], values[2]);
			log(s);
		}
		log("**********end read db value ***********");
	}

	void Update()
	{
		GetComponent<Text>().text = logs;
	}
}
