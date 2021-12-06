using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;

[Serializable]
class SocketMessage {
    public string sender;
    public string data;
}

public class SocketManager : MonoBehaviour
{
    // AWS:   ws://18.232.126.27:5000/
    // Local: ws://2f6f-68-234-129-29.ngrok.io
    public string API_URL;
    public GameObject screen;
    
    private WebSocket _ws;
    private bool _newFrameAvailable;
    private string _base64;

    // Start is called before the first frame update
    void Start()
    {
        _ws = new WebSocket(API_URL);
        _ws.Connect();
        _ws.OnMessage += (sender, e) =>
        {
            SocketMessage message = JsonUtility.FromJson<SocketMessage>(e.Data);
            if (message.sender.Equals("camera"))
            {
                _base64 = message.data;
                _newFrameAvailable = true;
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if(_ws == null)
        {
            return;
        }
        
        // send controls
        // TODO: consume from queue
        string dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            id = "some guid",
            type = "button",
            data = "pressed"
        });
        
        _ws.Send(Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            sender = "vr-controller",
            data = dataStr
        }));
        
        // render new screen frame
        if (screen != null && _newFrameAvailable && _base64 != null)
        {
            byte[] bytes = Convert.FromBase64String(_base64);
            Texture2D tex = new Texture2D(1,1);
            tex.LoadImage(bytes);
            
            Material material = new Material(Shader.Find("Diffuse"));
            material.mainTexture = tex;
            screen.GetComponent<Renderer>().material = material;
            _newFrameAvailable = false;
        }
    }
}
