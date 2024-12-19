using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.SceneManagement;
using static NetworkServer;

//Script which will handle client side networking of the game.
public class NetworkClient : MonoBehaviour
{
    //Cache Variables
    [HideInInspector] public NetworkClient instance;
    
    //Variables
    private TcpClient tcpClient;
    private NetworkStream stream;
    [HideInInspector] public int port = 7777;
    [HideInInspector] public int port2 = 5001;
    [HideInInspector] public int port3 = 5002;

    private bool isConnected = false;

    [Header("Helper")]
    [SerializeField] private FirebaseManager firebaseManager;
    [HideInInspector] public PlayerMultiplayerController playerMultiplayerController;
    [HideInInspector] public PlayerController2 playerController;
    [HideInInspector] public PlayerAIController playerAIController;
    [HideInInspector] public MultiplayerGameManager multiplayerGameManager;

    [Header("UI")]
    [SerializeField] private GameObject clientUI;
    [SerializeField] private GameObject JoinUI;
    [SerializeField] private InputField ServerIp;
    [SerializeField] private InputField messageField;

    float TDPMessageTimer = 0;
    float UDPMessageTimer = 0;

    //Message struct,
    [System.Serializable]
    public class MessageData
    {
        public Vector3 playerPosition;
        public int heatlth;
    }

    private void Awake()
    {
        //Making sure the script is passed down to multiplayer scene and 
        //deleting duplicate if found any.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject); 
        }
    }

    //Function for initiating TCP client handshake.
    public async void StartClient()
    {
        try
        {
            //fetching the ip address from dns server.
            firebaseManager.GetData(ServerIp.text);
            await Task.Delay(3000);

            //initiating TCP Client.
            tcpClient = new TcpClient();
            Debug.Log("Connecting to server at " + firebaseManager.ipaddress + " : " + port);
            await tcpClient.ConnectAsync(firebaseManager.ipaddress, port);
            Debug.Log("Connected to server at " + firebaseManager.ipaddress);
            //stream = tcpClient.GetStream();
            isConnected = true;

            //Activating Listeners.
            RecieveUDPMessage();
            StartCoroutine(SendTDPMessages());
            ReceiveTDPMessage();

            //Activating timers for checking packet loss.
            StartCoroutine(MessageTimer());

            //After successfully connecting to server, 
            //go to multiplayer game scene.
            SceneManager.LoadScene(1);
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }

    //Function to stop/close the connection to server.
    public void StopClient()
    {
        if (stream != null)
        {
            stream.Close();
            stream = null;
        }
        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
    }

    //Function to recieve data using UDP Protocol
    public async void RecieveUDPMessage()
    {
        using (UdpClient udpClient = new UdpClient(port2))
        {
            try
            {
                while (true)
                {

                    // Asynchronously wait for UDP message
                    UdpReceiveResult result = await udpClient.ReceiveAsync();

                    // fetch the received message
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    //And do accordingly to the message recieved.
                    if (receivedMessage.Contains("PlayerMovement"))
                    {
                        var messages = receivedMessage.Split(':');
                        if (messages[1] == "MoveForward")
                            playerController.MoveForward();
                        else if (messages[1] == "MoveBackward")
                            playerController.MoveBackward();
                        else if (messages[1] == "Punch")
                            playerController.Punch();
                        else if (messages[1] == "Kick")
                            playerController.Kick();
                        else
                            playerController.Defend();
                    }
                    else
                    {
                        if (playerMultiplayerController != null)
                        {

                            Debug.Log("received Message: " + receivedMessage);

                            if (receivedMessage == "Hit")
                            {
                                playerMultiplayerController.TakeDamage();
                            }
                            else
                            {
                                playerMultiplayerController.HandleAnimation(receivedMessage);
                            }
                        }
                        else
                            Debug.LogError("player multiplayer controller is null");
                    }

                    //reseting the UDPMessageTimer, so that we know 
                    //if encounter lag or packet loss.
                    UDPMessageTimer = 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving UDP message: " + e.Message);
            }
        }
    }

    //Functon to send data to Server using UDP Protocol
    public void SendUDPMessageToServer(string message)
    {
        try
        {
            //initializing udp
            UdpClient udpClient2 = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);

            //fetching ip address from the TCP Handshake.
            var clientip = tcpClient.Client.RemoteEndPoint.ToString().Split(":");
            udpClient2.Send(data, data.Length, clientip[0], port3);
            
            //close the udp after done
            udpClient2.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    //Function to send message to server using TCP Protocol
    public void SendTDPMessageToServer(string message)
    {
        try
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                //Initiating network stream
                NetworkStream stream = tcpClient.GetStream();
                if (stream != null && stream.CanWrite)
                {
                    //encoding message/data.
                    byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                }
                else
                {
                    Debug.LogError("Cannot send message/data using stream.");
                }
            }
            else
            {
                Debug.LogError("Not connected to Server");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    //Function for occasionaly sending data to client using TCP Protocol
    //to verfiy packet loss.
    public IEnumerator SendTDPMessages()
    {
        while (true)
        {
            //Sends only important data through TCP Protocol and then verifies and do accordingl.
            yield return new WaitForSeconds(1f);
            if (playerController != null)
            {
                //Initializing data struct and adding information
                //converting the struct into a string and then 
                //sent to Server.
                MessageData messageData = new MessageData();
                messageData.playerPosition = playerController.gameObject.transform.position;
                messageData.heatlth = playerController.health;

                var jsonData = JsonUtility.ToJson(messageData);

                SendTDPMessageToServer(jsonData);
            }
        }
    }

    //Function for Receieve TDP Message from client.
    public async void ReceiveTDPMessage()
    {
        try
        {
            while(true)
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    //Initiates network stream if we are connected to server.
                    NetworkStream stream = tcpClient.GetStream();
                    if (stream != null && stream.CanRead)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                            //processing the recieved data/message.
                            string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            MessageData messageData;
                            messageData = JsonUtility.FromJson<MessageData>(receivedMessage);
                            playerMultiplayerController.VerficationAndActions(messageData.playerPosition, messageData.heatlth);
                            //Debug.LogWarning("client TDP Message: " + receivedMessage);
                        }
                        else
                        {
                            //Debug.LogWarning("No data received or invalide length");
                        }

                        //TDP message timer resets,
                        //used for checking packet log and latency.
                        TDPMessageTimer = 0;
                        Debug.LogWarning("tdp rec:" + TDPMessageTimer);
                    }
                    else
                    {
                        Debug.LogError("Cannot read data/message from the stream.");
                    }
                }
                else
                {
                    Debug.LogError("Not connected to the server.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    //Function to call the corutine which routinely sends important 
    //data to server using TCP protocol.
    //used for compensating packet loss.
    public void StartTDPVerifications()
    {
        StartCoroutine(SendTDPMessages());
    }

    //Timer function which keep checking the last time client recieved a message
    //from server.
    private IEnumerator MessageTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            TDPMessageTimer++;
            UDPMessageTimer++;
            
            //if we haven't recieved TDP Message from server for more than 10 seconds,
            //then end the game in draw
            if (TDPMessageTimer > 10)
            {
                playerMultiplayerController.Lose();
                playerController.Lose();
                multiplayerGameManager.GameEnded("Draw");
                TDPMessageTimer = 0;
            }

            //if we haven't recieved UDP message from server for more than 15 seconds,
            //this can be due to lag/latency or packetloss.
            //Do a prediction of the player movement and send the movement we did to server
            //so it can update over there as well.
            if (UDPMessageTimer > 15)
            {
                var prediction = playerAIController.GetASingleReaction();
                playerMultiplayerController.HandleAnimation(prediction);
                SendUDPMessageToServer("PlayerMovement:" + prediction);
                UDPMessageTimer = 0;
            }

            //if the game is ended, stop the loop
            if(multiplayerGameManager != null && multiplayerGameManager.isGameEnded)
            {
                break;
            }    
        }
    }

   
}
