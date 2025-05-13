using UnityEngine;
using Unity.Netcode;

public class PlayerNet : NetworkBehaviour
{

    public GameObject pillar;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateObjectServerRPC();
        }
    }

    [ServerRpc]
    public void CreateObjectServerRPC()
    {
        Gameobjekt obj = Instantiate(pillar);
        obj.GetComponent<NetworkObject>().Spawn();
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