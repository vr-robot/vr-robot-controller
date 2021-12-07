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
    // socket server url
    public string API_URL;
    
    // screen to draw frame onto
    public GameObject screen;
    
    // socket and frame variables
    private WebSocket _ws;
    private string _base64;
    
    // queue to consume when sending data to python client
    Queue _messageQueue;

    // method to add to queue
    public void AddToMessageQueue(string message)
    {
        if (_messageQueue != null)
        {
            _messageQueue.Enqueue(message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // initialize message queue object
        _messageQueue = new Queue();
        
        // string dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(new
        // {
        //     id = "some guid",
        //     type = "button",
        //     data = "pressed"
        // });
        //
        // AddToMessageQueue(dataStr);
        
        // connect to socket server
        _ws = new WebSocket(API_URL);
        _ws.Connect();
        
        // callback for new socket messages
        _ws.OnMessage += (sender, e) =>
        {
            SocketMessage message = JsonUtility.FromJson<SocketMessage>(e.Data);
            if (message.sender.Equals("camera"))
            {
                _base64 = message.data;
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
        
        try
        {
            // request for new frame from camera
            // if (_ws != null)
            // {
            //     _ws.Send(Newtonsoft.Json.JsonConvert.SerializeObject(new
            //     {
            //         sender = "vr-controller",
            //         data = Newtonsoft.Json.JsonConvert.SerializeObject(new
            //         {
            //             type = "frame-request",
            //             data = "none",
            //             name = "socket_manager"
            //         })
            //     }));
            // }
            
            // send controls
            while (_ws != null && _messageQueue != null && _messageQueue.Count > 0)
            {
                try
                {
                    _ws.Send(Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        sender = "vr-controller",
                        data = _messageQueue.Dequeue()
                    }));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        
            // render new screen frame
            if (screen != null && _base64 != null)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(_base64);
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(bytes);

                    Material material = new Material(Shader.Find("Diffuse"));
                    material.mainTexture = tex;
                    screen.GetComponent<Renderer>().material = material;
                    _base64 = null;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Source);
            Debug.LogError(e.Message);
            // attempt to reconnect to socket server if error
            _ws = new WebSocket(API_URL);
            _ws.Connect();
        }
    }
}
