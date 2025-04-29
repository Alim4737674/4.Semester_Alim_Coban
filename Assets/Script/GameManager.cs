using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.StartHost();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
