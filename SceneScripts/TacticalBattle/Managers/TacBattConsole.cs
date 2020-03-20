using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacBattConsole : MonoBehaviour{
	public Text consoleText;

	List<string> outputs;

	static int numLines = 5;
	static int maxCharsPerLine = 170;

	// bool first = true;

    // Start is called before the first frame update
    void Start(){
        GRefs.tacBattConsole = this;
		outputs = new List<string>();
    }

	public void PostMessage(string msg){
		if(msg.Length > maxCharsPerLine){
			int start = 0;
			while(true){
				bool final = false;
				if(start + maxCharsPerLine >= msg.Length)
					final = true;

				string sub = "";
				char[] a = msg.ToCharArray();
				for(int i =start; i<start+maxCharsPerLine && i<a.Length;i++)
					sub += a[i];
				PostMessage(sub);
				if(final)
					break;

				start += maxCharsPerLine;
			}
		}else
			outputs.Add(msg);
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
