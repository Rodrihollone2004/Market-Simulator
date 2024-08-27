using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float distanceBetweenClients = 2f;
    [SerializeField] private Transform exitPoint;
    private List<Sections> sections = new List<Sections>();
    private Queue<GameObject> clients = new Queue<GameObject>();
    private Vector3 nextPosition;
    private bool isMoving = false;

    private void Start()
    {
        if (clientPrefab == null)
        {
            Debug.LogError("clientPrefab no está asignado.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint no está asignado.");
            return;
        }

        nextPosition = spawnPoint.position;
        StartCoroutine(SpawnClients());

        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
        {
            sections.AddRange(shelf.Sections);
        }

        if (sections.Count == 0)
        {
            Debug.LogWarning("No se han encontrado secciones en el estante.");
        }
    }

    private void SpawnClient()
    {
        if (sections.Count > 0)
        {
            GameObject client = Instantiate(clientPrefab, nextPosition, Quaternion.identity);
            clients.Enqueue(client);
            nextPosition += Vector3.back * distanceBetweenClients;

            Client clientScript = client.GetComponent<Client>();
            if (clientScript != null)
            {
                clientScript.SetExit(exitPoint);
            }

            if (!isMoving)
            {
                isMoving = true;
                StartCoroutine(MoveClient());
            }
        }
        else
        {
            Debug.LogWarning("No se han encontrado secciones para agregar clientes.");
        }

    }
    private IEnumerator SpawnClients()
    {
        while (true)
        {
            if (clients.Count < MaxClients)
            {
                SpawnClient();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    private const int MaxClients = 10; 



    private IEnumerator MoveClient()
    {
        while (true)
        {
            if (clients.Count == 0)
            {
                isMoving = false;
                yield break;
            }

            GameObject client = clients.Peek();

            if (client == null || !client.activeInHierarchy)
            {
                clients.Dequeue();
                continue;
            }

            Client clientScript = client.GetComponent<Client>();
            if (clientScript == null)
            {
                clients.Dequeue();
                continue;
            }

            int sectionIndex = -1;
            for (int i = 0; i < sections.Count; i++)
            {
                sectionIndex = Random.Range(0, sections.Count);
                if (sections[sectionIndex].HasItems())
                {
                    break;
                }
            }

            if (sectionIndex == -1 || !sections[sectionIndex].HasItems())
            {
                Debug.LogWarning("No se encontraron secciones con items disponibles.");
                yield break;
            }


            Transform objective = sections[sectionIndex].transform;
            float speed = 5f;

            while (client != null && client.activeInHierarchy && Vector3.Distance(client.transform.position, objective.position) > 0.1f && !clientScript.IsMovingToExit())
            {
                if (client == null || !client.activeInHierarchy)
                {
                    break;
                }

                client.transform.position = Vector3.MoveTowards(client.transform.position, objective.position, speed * Time.deltaTime);
                yield return null;
            }


            if (clientScript != null)
            {
                clientScript.SetExit(exitPoint);
                StartCoroutine(clientScript.MoveToExit());
            }

          
            yield return new WaitUntil(() => client == null || !client.activeInHierarchy);

       
            clients.Dequeue(); 

            UpdateClientPositions();

            if (clients.Count > 0)
            {
                StartCoroutine(MoveClient());
                yield break;
            }
        }
    }
    private void UpdateClientPositions()
    {
        Vector3 currentPosition = spawnPoint.position;

        foreach (GameObject client in clients)
        {
            if (client != null && client.activeInHierarchy)
            {
                client.transform.position = currentPosition;
                currentPosition += Vector3.back * distanceBetweenClients;
            }
        }

        nextPosition = currentPosition;
    }


    public void NotifyClientReachedExit()
    {
        if (!isMoving && clients.Count > 0)
        {
            isMoving = true;
            StartCoroutine(MoveClient());
        }
    }
}
