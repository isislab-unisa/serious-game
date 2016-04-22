#undef OLD

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using VR = UnityEngine.VR;

//-------------------------------------------------------------------------------------
/// <summary>
/// Shows debug information on a heads-up display.
/// </summary>
public class OVRShowInfo : MonoBehaviour
{
	public string InfoUIManagerPrefabName;
	public int fontSize = 20;

	//enable and disable msg
	bool msgEnabled = true;
    Text text;
    SpriteRenderer spriteRenderer;
	int lastMsgPriority = 2;

	/// <summary>
	/// Initialization
	/// </summary>
	void Awake()
    {
		GameObject prefab = Resources.Load(InfoUIManagerPrefabName) as GameObject;
		GameObject msgUIManager = Instantiate(prefab);
		msgUIManager.transform.SetParent(GetComponentInChildren<Camera>().transform);
		msgUIManager.transform.localPosition = prefab.transform.localPosition;
		text = msgUIManager.GetComponentInChildren<Text>();
        spriteRenderer = msgUIManager.GetComponentInChildren<SpriteRenderer>();
    }

#if OLD
	#region GameObjects for Debug Information UIs   
	GameObject msgUIManager; // remove
	GameObject msgUIObject; // remove
	GameObject texts; // remove
	#endregion

	/// <summary>
	/// UIs Y offset
	/// </summary>
	float offsetY = 55.0f;

	float posY = 0.0f;


	void Awake()
	{
		#region Create and Add MsgManager
		// Create canvas for using new GUI
		msgUIManager = new GameObject();
		msgUIManager.name = "InfoUIManager";
		msgUIManager.transform.parent = GameObject.Find("CenterEyeAnchor").transform;

		RectTransform rectTransform = msgUIManager.AddComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(100f, 100f);
		rectTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		rectTransform.localPosition = new Vector3(0.01f, 0.01f, 0.53f);
		//rectTransform.localPosition = new Vector3(0.01f, 0.17f, 0.53f);
		rectTransform.localEulerAngles = Vector3.zero;

		Canvas canvas = msgUIManager.AddComponent<Canvas>();

		canvas.renderMode = RenderMode.WorldSpace;

		//canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.pixelPerfect = true;

		/*-------------------------*/

		msgUIObject = new GameObject();
		msgUIObject.name = "ShowInfo";
		msgUIObject.transform.parent = GameObject.Find("InfoUIManager").transform;
		msgUIObject.transform.localPosition = new Vector3(0.0f, 100.0f, 0.0f);
		msgUIObject.transform.localEulerAngles = Vector3.zero;
		msgUIObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		GameObject GO = new GameObject();
		GO.AddComponent<RectTransform>();
		GO.AddComponent<CanvasRenderer>();
		//GO.AddComponent<Image>();
		GO.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 200f);
		//GO.GetComponent<Image>().color = new Color(7f / 255f, 45f / 255f, 71f / 255f, 200f / 255f);

		//GO.GetComponent<Image>().sprite

		GO.transform.SetParent(msgUIObject.transform);
		GO.name = "RiftPresent";
		RectTransform rectTransform2 = GO.GetComponent<RectTransform>();
		rectTransform2.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		rectTransform2.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		rectTransform2.localEulerAngles = Vector3.zero;

		texts = new GameObject();
		texts.AddComponent<RectTransform>();
		texts.AddComponent<CanvasRenderer>();
		texts.AddComponent<Text>();
		texts.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 200f);
		//texts.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		texts.GetComponent<Text>().font = Resources.Load("Fonts/GOTHIC") as Font;
		texts.GetComponent<Text>().fontSize = 20;
		texts.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

		texts.transform.SetParent(GO.transform);
		texts.name = "TextBox";
		texts.GetComponentInChildren<Text>().text = "";

		RectTransform rectTransform3 = texts.GetComponent<RectTransform>();
		rectTransform3.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		rectTransform3.localPosition = new Vector3(0.0f, posY -= offsetY, 0.0f);
		rectTransform3.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		rectTransform3.localEulerAngles = Vector3.zero;

		text = texts.GetComponentInChildren<Text>();
		//Oculus text overlay
		Material textMaterial = Resources.Load("Materials/OculusText", typeof(Material)) as Material;
		text.material = textMaterial;

		#endregion
	}
	/// <summary>
	/// Updating VR variables and managing UI present
	/// </summary>

	void Update()
    {

    }

#endif

#region DisplayMsg
	public void displayMsg(string msg, float msgTime, int priority, Sprite sprite)
    {
        if (msgEnabled)
        {
            if ((priority <= lastMsgPriority) || (text.text.Equals("")))
            {
                StopAllCoroutines();
                StartCoroutine(displayMsg2(msg, msgTime, priority,sprite));
            }
        }
    }

    IEnumerator displayMsg2(string msg, float msgTime, int priority, Sprite sprite){
        text.fontSize = fontSize;
        if ((priority <= lastMsgPriority) || (text.text.Equals(""))){
            text.text = msg;
            spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(msgTime);
            text.text = "";
            spriteRenderer.sprite = null;
        }
    }

	public void cleanmsg()
    {
        text.text = "";
    }

    public void msgEnable(bool msgState)
    {
        msgEnabled = msgState;
    }
#endregion

}