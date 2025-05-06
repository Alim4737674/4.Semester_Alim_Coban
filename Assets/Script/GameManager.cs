using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public bool isClient;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isClient)
        {
            NetworkManager.Singleton.StartClient();
        }
        else
        {
            NetworkManager.Singleton.StartHost();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
