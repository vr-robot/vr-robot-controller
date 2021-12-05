using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class SocketManager : MonoBehaviour
{
    WebSocket ws;
    private string API_URL = "wss://4f4a-68-234-129-29.ngrok.io";

    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket(API_URL);
        ws.Connect();
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message Received from "+((WebSocket)sender).Url+", Data : "+e.Data);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if(ws == null)
        {
            return;
        }
        
        ws.Send("Hello");
    }
}
