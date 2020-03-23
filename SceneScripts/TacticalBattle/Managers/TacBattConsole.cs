using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacBattConsole : MonoBehaviour{
	public Text consoleText;

	List<string> outputs;

	static int numLines = 25;
	static int maxCharsPerLine = 85;

	// bool first = true;

    // Start is called before the first frame update
    void Start(){
        GRefs.tacBattConsole = this;
		outputs = new List<string>();
    }

	public void PostMessage(string msg){
		if(msg.Contains("\n")){
			string[] msgs = msg.Split('\n');
			for(int i = 0; i<msgs.Length;i++){
				if(msgs[i].Length > 0)
					PostMessage(msgs[i]);
			} 
		}

		if(msg.Length > maxCharsPerLine){
			string[] words = msg.Split(' ');
			string s = "";
			for(int i = 0;i<words.Length;i++){
				if( (s + " " + words[i]).Length > maxCharsPerLine){
					outputs.Add(s.Trim());
					s = words[i];
				}else
					s += " " + words[i];
			}
			outputs.Add(s.Trim());
		}else
			outputs.Add(msg.Trim());
	}

    // Update is called once per frame
    void Update(){
		consoleText.text = GetStringForConsole();
    }

	string GetStringForConsole(){
		string[] temps = outputs.ToArray();
		string sout = "";
		
		if(outputs.Count<numLines){
			for(int i = 0; i < numLines - outputs.Count; i++)
				sout += "\n";

			for(int i = 0; i<temps.Length; i++)
				sout += temps[i] + "\n";
		}else{
			int ii = temps.Length - numLines;
			for(int i = 0; i<numLines; i++)
				sout += temps[ii+i] + "\n";
		}

		return sout;
	}
}
