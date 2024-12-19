using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

//Script which will handle server side of coding.
public class NetworkServer : MonoBehaviour
{
    //Cache Variables
    [HideInInspector] public NetworkServer instance;
    [HideInInspector] public TcpListener tcpListener;
    [HideInInspector] public TcpClient tcpClient;
    private NetworkStream stream;
    [HideInInspector] public bool isServer = false;
    [HideInInspector] public PlayerMultiplayerController playerMultiplayerController;
    [HideInInspector] public PlayerController2 playerController;
    [HideInInspector] public PlayerAIController playerAiController;
    [HideInInspector] public MultiplayerGameManager multiplayerGameManager;

    //variables
    [HideInInspector] public int port = 7777;
    [HideInInspector] public int port2 = 5001;
    [HideInInspector] public int port3 = 5002;

    [SerializeField] private InputField messageField;
    [SerializeField] private GameObject createServerUI;
    [SerializeField] private GameObject sendMessageUI;
    [SerializeField] private Text codeText;

    [SerializeField] private FirebaseManager firebaseManager;

    //Message struct
    [System.Serializable]
    public class MessageData
    {
        public Vector3 playerPosition;
        public int heatlth;
    }

    //timer variables.
    float TDPMessageTimer = 0;
    float UDPMessageTimer = 0;

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
    //Function for creating TCP server.
    //creates a unique room code which is passed to DNS along with its server address.
    public async void StartServer()
    {
        try
        {
            //setting IP address
            string localIP = "";
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = address.ToString();
                    break;
                }
            }

            //generating and saving unique code.
            var roomcode = GenerateRoomCode();
            firebaseManager.SendData(localIP, roomcode);
            tcpListener = new TcpListener(IPAddress.Parse(localIP), port);
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress, true);
            tcpListener.Start();
            codeText.text = "Joining Code: \n" + roomcode;
            tcpClient = await tcpListener.AcceptTcpClientAsync();
            
            createServerUI.SetActive(false);
            sendMessageUI.SetActive(true);
            //Activating Listeners.
            RecieveUDPMessage();
            StartCoroutine(SendTCPMessages());
            StartCoroutine(MessageTimer());
            ReceiveTCPMessage();
            isServer = true;

            //taking us to the muliplayer game scene.
            SceneManager.LoadScene(1);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    //Function for sending Message/data to client using TDP Protocol.
    public void SendTDPMessageToClient(string message)
    {
        try
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                //getting the network stream
                NetworkStream stream = tcpClient.GetStream();
                if (stream != null && stream.CanWrite)
                {
                    //processing the data/message before sending
                    byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                }
                else
                {
                    Debug.LogError("cannot send data using stream");
                }
            }
            else
            {
                Debug.LogError("client is not connected");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    //Function for sending Message/data to client using UDP Protocol,
    public void SendUDPMessageToClient(string message)
    {
        try
        {
            //intiating udp client
            //fetching ip from handshake
            //and sending message/data
            UdpClient udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            var clientip = tcpClient.Client.RemoteEndPoint.ToString().Split(":");            
            udpClient.Send(data, data.Length, clientip[0], port2);
            
            //closing the udp
            udpClient.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    //Recieve UDP message/data from Client
    public async void RecieveUDPMessage()
    {
        using (UdpClient udpClient2 = new UdpClient(port3))
        {
            try
            {
                while (true)
                {
                    // Asynchronously wait for a UDP message
                    UdpReceiveResult result = await udpClient2.ReceiveAsync();

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
                Debug.LogError("Error while receiving UDP message: " + e.Message);
            }
        }
    }

    //function to stop or end the connection to client.
    public void StopServer()
    {
        if(stream != null)
        {
            stream.Close();
            stream = null;
        }
        if(tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
        if(tcpListener != null)
        {
            tcpListener.Stop();
            tcpListener = null;
        }

    }

    //function to create a 6 digit alphanumeric code that can be used as DNS
    private static string GenerateRoomCode()
    {
        var characters = "abcdefghijklmnopqrstuvwxyz012345789";
        char[] result = new char[6];
        for (int i = 0; i < 6; i++)
        {
            result[i] = characters[UnityEngine.Random.Range(0,characters.Length)];
        }
        return new string(result);
    }

    //Function to verify the data are synced over the server and client.
    public void StartTCPVerifications()
    {
        StartCoroutine(SendTCPMessages());
    }

    //Coroutine which send important data for verification to client using TCP protocol
    public IEnumerator SendTCPMessages()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (playerController != null)
            {
                MessageData messageData = new MessageData();
                messageData.playerPosition = playerController.gameObject.transform.position;
                messageData.heatlth = playerController.health;

                var jsonData = JsonUtility.ToJson(messageData);

                SendTDPMessageToClient(jsonData);
            }
        }
    }

    //Function which recieves messages/data recieved from client using TCP Protocol.
    public async void ReceiveTCPMessage()
    {
        try
        {
            while(true)
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    //Setting up stream
                    NetworkStream stream = tcpClient.GetStream();
                    if (stream != null && stream.CanRead)
                    {
                        //processing recieved message/data
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                            string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            MessageData messageData;
                            messageData = JsonUtility.FromJson<MessageData>(receivedMessage);
                            playerMultiplayerController.VerficationAndActions(messageData.playerPosition, messageData.heatlth);
                            //resetting Message timer
                            TDPMessageTimer = 0;
                        }
                        else
                        {
                            Debug.LogWarning("invalid length.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Cannot read from the stream.");
                    }
                }
                else
                {
                    Debug.LogError("Not connected to client.");
                }

                //leaving the loop if the game is ended.
                if (multiplayerGameManager != null && multiplayerGameManager.isGameEnded)
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    //Timer function which keep checking the last time client recieved a message
    //from client.
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
                Debug.LogWarning("Last Message recieved 10 seconds ago");
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
                Debug.LogWarning("Last Message recieved 10 seconds ago");
                var prediction = playerAiController.GetASingleReaction();
                Debug.LogWarning("Prediction: " + prediction);
                playerMultiplayerController.HandleAnimation(prediction);
                SendUDPMessageToClient("PlayerMovement:" + prediction);
                UDPMessageTimer = 0;
            }

            //if the game is ended, stop the loop
            if (multiplayerGameManager != null && multiplayerGameManager.isGameEnded)
                break;
        }

    }

   
}
