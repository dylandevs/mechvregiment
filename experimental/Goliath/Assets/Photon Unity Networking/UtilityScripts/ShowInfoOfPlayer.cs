using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
/// <summary>
/// Can be attached to a GameObject to show info about the owner of the PhotonView.
/// </summary>
/// <remarks>
/// This is a Photon.Monobehaviour, which adds the property photonView (that's all).
/// </remarks>
[RequireComponent(typeof(PhotonView))]
public class ShowInfoOfPlayer : Photon.MonoBehaviour
{
    private GameObject textGo;
    private TextMesh tm;
    private const int FontSize3D = 0;  // could be a variable, too

    public Font font;
    public bool DisableOnOwnObjects;

    void Start()
    {
        if (font == null)
        {
            #if UNITY_3_5
            font = (Font)FindObjectsOfTypeIncludingAssets(typeof(Font))[0];
            #else
            font = (Font)Resources.FindObjectsOfTypeAll(typeof(Font))[0];
            #endif
            Debug.LogWarning("No font defined. Found font: " + font);
        }

        if (tm == null)
        {
            textGo = new GameObject("3d text");
            textGo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            textGo.transform.parent = this.gameObject.transform;
            textGo.transform.localPosition = Vector3.zero;

            MeshRenderer mr = textGo.AddComponent<MeshRenderer>();
            mr.material = font.material;
            tm = textGo.AddComponent<TextMesh>();
            tm.font = font;
            tm.fontSize = FontSize3D;
            tm.anchor = TextAnchor.MiddleCenter;
        }

        if (!DisableOnOwnObjects && this.photonView.isMine)
        {
            // this script runs on an object that this client owns. skip the label (but you could modify it here, too)
            this.enabled = false;   // stop Update() calls
        }
    }

    void OnEnable()
    {
        if (textGo != null) textGo.SetActive(true);
    }
    void OnDisable()
    {
        if (textGo != null) textGo.SetActive(false);
    }
    
    void Update()
    {
        if (DisableOnOwnObjects)
        {
            this.enabled = false;
            if (textGo != null) textGo.SetActive(false);
            return;
        }

        PhotonPlayer owner = this.photonView.owner;
        if (owner != null)
        {
            tm.text = (string.IsNullOrEmpty(owner.name)) ? "n/a" : owner.name;
        }
        else if (this.photonView.isSceneView)
        {
            if (!DisableOnOwnObjects && this.photonView.isMine)
            {
                // On a room's Master Client, isMine includes game objects owned by the scene.
                // If the Master Client leaves, the next will take over control.
                // If this client became the Master Client somehow, we can now drop the labels.
                this.enabled = false;   // stop Update() calls
                
                #if UNITY_3_5
                textGo.active = false;  // hide 3d label
                #else
                textGo.SetActive(false);
                #endif

                return;
            }

            tm.text = "scn";
        }
        else
        {
            tm.text = "n/a";
        }
    }
}
