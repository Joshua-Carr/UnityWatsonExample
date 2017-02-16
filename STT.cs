using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;


public class STT : MonoBehaviour {

	[SerializeField]
	private AudioClip m_AudioClip = new AudioClip();
	private SpeechToText m_SpeechToText = new SpeechToText();

	public AudioClip statics = new AudioClip();


	public void onDown(){

		AudioSource aud = GetComponent<AudioSource>();
		aud.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
		m_AudioClip = aud.clip;

	}


	public void onUp(){
		playStatic ();
		m_SpeechToText.Recognize(m_AudioClip, HandleOnRecognize);	

	}
	void playStatic()
	{

		AudioSource.PlayClipAtPoint (statics, new Vector3(5, 1, 2));

	}


	void HandleOnRecognize (SpeechRecognitionEvent result)
	{
		Debug.Log ("STT here");
		if (result != null && result.results.Length > 0)
		{
			foreach( var res in result.results )
			{	
				foreach( var alt in res.alternatives )
				{
					string text = alt.transcript;
					webCamScript.setText(text);
				}
			}
		}

	} 


	
}



