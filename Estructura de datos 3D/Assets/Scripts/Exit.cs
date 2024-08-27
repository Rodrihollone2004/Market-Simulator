using System.Collections;
using UnityEngine;

public class Exit : MonoBehaviour
{
    private ClientManager clientManager;

    private void Start()
    {
        clientManager = FindObjectOfType<ClientManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Client"))
        {
            Debug.Log("Cliente ha llegado al punto de salida: " + other.gameObject.name);
            Destroy(other.gameObject);
            if (clientManager != null)
            {
                clientManager.NotifyClientReachedExit();
            }
        }
    }
}
