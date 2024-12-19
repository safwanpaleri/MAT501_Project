using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;


//Script that deals with code related to Firebase Database
//Firebase database is used as a DNS holder, where the ip address and DNS will be saved
//The player can fetch the ip using the roomcode and can be used to connect to networking.
public class FirebaseManager : MonoBehaviour
{
    //Cache variables
    private DatabaseReference dbReference;
    [HideInInspector] public string ipaddress;

    // Start is called before the first frame update
    void Start()
    {
        //initializing Firebase realtime databse, where we are storing dns data.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                Debug.LogError($"Could not initialize Firebase: {task.Result}");
            }
        });
    }

    //Function for sending data to firebase database
    //used to send the room code and ip-address.
    public void SendData(string ip, string roomcode)
    {
        StartCoroutine(SendData_Coroutine(ip, roomcode));
    }

    //Coroutine function of sending data to firebase.
    private IEnumerator SendData_Coroutine(string ip, string roomcode)
    {
        var task = dbReference.Child("DNS").Child(roomcode).SetValueAsync(ip);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCompletedSuccessfully)
            Debug.Log("Added ip to DNS");
        else
            Debug.LogError("Adding to database failed");

    }

    //Functiob for fetching data from firebase database
    //we will send a room code and its ip address will be returned 
    //if it is present in the database.
    public void GetData(string roomcode)
    {
        StartCoroutine(GetData_Coroutine(roomcode));
    }

    //Coroutine function of fetching data from firebase
    private IEnumerator GetData_Coroutine(string roomcode)
    {
        var task = dbReference.Child("DNS").Child(roomcode).GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("Fetched ip using roomcode: " + task.Result.Value);
            ipaddress = task.Result.Value.ToString();
        }
        else
            Debug.LogError("fetching dns failed: " + task.Result.ToString());
    }

}
