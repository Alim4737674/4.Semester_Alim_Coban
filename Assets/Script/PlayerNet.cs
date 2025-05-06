using UnityEngine;
using Unity.Netcode;

public class PlayerNet : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public NetworkVariable<int> score = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(x, z, 0);
        if (movement.magnitude > 0) 
        {
            MovingServerRPC(movement);
        }
        score.Value += 1;
    }

    [ServerRpc]
    void MovingServerRPC(Vector3 movement)
    {
        transform.position += movement * Time.deltaTime * 3;
        MovingClientRPC(transform.position);
    }

    [ClientRpc]
    void MovingClientRPC(Vector3 position)
    {
        transform.position = position;
    }
}
