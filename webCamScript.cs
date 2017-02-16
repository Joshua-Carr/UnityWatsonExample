using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;
using System.IO;

using UnityEngine.EventSystems;


public class webCamScript : MonoBehaviour	 {

	public static string textFromSpeech { get; set;}
	public static bool doConvo = false;
	private Conversation m_Conversation = new Conversation();
	private string m_WorkspaceID = "f4651e6e-7e76-4cbe-bfda-d07b637c7ce4";
	private MessageRequest m_Input = new MessageRequest();

	[SerializeField]
	private AudioClip m_AudioClip = new AudioClip();
	private SpeechToText m_SpeechToText = new SpeechToText();

	private VisualRecognition m_VisualRecognition = new VisualRecognition();


	TextToSpeech m_TextToSpeech = new TextToSpeech();


	public GameObject webCameraPlane; 
	public Button fireButton;



	private WebCamTexture webCameraTexture;
	// Use this for initialization
	void Start () {

		if (Application.isMobilePlatform) {
			GameObject cameraParent = new GameObject ("camParent");
			cameraParent.transform.position = this.transform.position;
			this.transform.parent = cameraParent.transform;
			cameraParent.transform.Rotate (Vector3.right, 90);
		}


		

		Input.gyro.enabled = true;

		webCameraTexture= new WebCamTexture();
		webCameraPlane.GetComponent<MeshRenderer>().material.mainTexture = webCameraTexture;
		webCameraTexture.Play();

	}





	public static void setText(string textFromSTT){
		textFromSpeech = textFromSTT;
		doConvo = true;
	
	}
		
	void convoFromSpeech(){
		m_Input.InputText = textFromSpeech;
		Debug.Log("User: " + m_Input.InputText);
		m_Conversation.Message(OnMessage, m_WorkspaceID, m_Input);
		doConvo = false;
	}


	void OnMessage (MessageResponse resp, string err)
	{
		
		m_Input.conversationID = resp.context.conversation_id;
		m_Input.ContextData = resp.context;
		Debug.Log("response: " + resp.output.text[0]);
		//string m_TestString = "<speak version=\"1.0\"><say-as interpret-as=\"letters\">I'm sorry</say-as>. <prosody pitch=\"150Hz\">This is Text to Speech!</prosody></express-as><express-as type=\"GoodNews\">I'm sorry. This is Text to Speech!</express-as></speak>";

		string m_TestString = resp.output.text[0];


		m_TextToSpeech.Voice = VoiceType.en_GB_Kate;

		m_TextToSpeech.ToSpeech(m_TestString, HandleToSpeechCallback, true);
	}

	// Update is called once per frame
	void Update () {

		Quaternion cameraRotation = new Quaternion (Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);
		this.transform.localRotation = cameraRotation;
		if (doConvo) {
			convoFromSpeech();
		}
		

	}

	public void photo(){
		TakePhoto (webCameraTexture, 0);
	}

	public void photoText(){
		TakePhoto (webCameraTexture, 1);
	}

	public void TakePhoto(WebCamTexture webCameraTexture, int flag)
	{

		// NOTE - you almost certainly have to do this here:
	//	yield return new WaitForEndOfFrame(); 

		// it's a rare case where the Unity doco is pretty clear,
		// http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
		// be sure to scroll down to the SECOND long example on that doco page 

		Texture2D photo = new Texture2D(webCameraTexture.width, webCameraTexture.height);
		photo.SetPixels(webCameraTexture.GetPixels());
		photo.Apply();

		//Encode to a PNG
		byte[] bytes = photo.EncodeToPNG();
		//Write out the PNG. Of course you have to substitute your_path for something sensible
		File.WriteAllBytes(Application.persistentDataPath + "/photo.png", bytes);


		string imagesPath = Application.persistentDataPath + "/photo.png";
		string[] owners = { "IBM", "me" };
		string[] classifierIDs = { "default", "" };

		if (flag == 1) {
				if (!m_VisualRecognition.RecognizeText (imagesPath, OnRecognizeText))
				Debug.Log("missed text");
			
		} else {
			if (!m_VisualRecognition.Classify(imagesPath, OnClassify, owners, classifierIDs, 0.5f))
				Debug.Log("ExampleVisualRecognition" + "Classify image failed!");
			
		}

	}


	private void OnRecognizeText(TextRecogTopLevelMultiple multipleImages, string data)
	{
		if (multipleImages != null)
		{
			foreach (TextRecogTopLevelSingle texts in multipleImages.images)
			{
				Debug.Log(texts.text);
				setText (texts.text);

			}
		}
		else
		{
			Debug.Log("ExampleVisualRecognition" + "RecognizeText failed!");
		}
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
					Debug.Log(string.Format( "{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));
				}
			}
		}
	} 



	private void OnClassify(ClassifyTopLevelMultiple classify, string data)
	{
		if (classify != null)
		{
			Debug.Log("ExampleVisualRecognition" + "images processed: " + classify.images_processed);
			foreach (ClassifyTopLevelSingle image in classify.images)
			{
				Debug.Log("ExampleVisualRecognition" + "\tsource_url: " + image.source_url + ", resolved_url: " + image.resolved_url);
				if (image.classifiers != null && image.classifiers.Length > 0)
				{
					
					foreach (ClassifyPerClassifier classifier in image.classifiers)
					{
						Debug.Log("ExampleVisualRecognition" + "\t\tclassifier_id: " + classifier.classifier_id + ", name: " + classifier.name);
						foreach (ClassResult classResult in classifier.classes) {
							Debug.Log ("class: " + classResult.m_class + ", score: " + classResult.score + ", type_hierarchy: " + classResult.type_hierarchy);
							if (classResult.m_class.Contains ("window")) {
								setText("I love you");
							}
						}
					}
				}
			}
		}
		else
		{
			Debug.Log("ExampleVisualRecognition" + "Classification failed!");
		}
	}



	void HandleToSpeechCallback (AudioClip clip)
	{
		PlayClip(clip);
		Debug.Log ("here");
	}

	private void PlayClip(AudioClip clip)
	{
		
		if (Application.isPlaying && clip != null)
		{
			GameObject audioObject = new GameObject("AudioObject");
			AudioSource source = audioObject.AddComponent<AudioSource>();
			source.spatialBlend = 0.0f;
			source.loop = false;
			source.clip = clip;
			Debug.Log ("play");
			source.Play();

			GameObject.Destroy(audioObject, clip.length);
		}
	}
}
