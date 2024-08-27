using System.Collections;
using UnityEngine;

public class Client : MonoBehaviour
{
    private bool hasPickedUpItem = false;
    private bool isMovingToExit = false;
    private Transform Exit;
    [SerializeField] private Transform itemHolder;


    public void SetExit(Transform exit)
    {
        this.Exit = exit;
    }

    public bool IsMovingToExit()
    {
        return isMovingToExit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") && !hasPickedUpItem)
        {
            PickUpItem(other.gameObject);
            hasPickedUpItem = true;

            if (!isMovingToExit)
            {
                isMovingToExit = true;
                StartCoroutine(MoveToExit());
            }
        }
    }

    private void PickUpItem(GameObject item)
    {
        item.transform.GetComponentInParent<Sections>().RemoveItem();
        item.transform.SetParent(itemHolder); 
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.GetComponent<Collider>().enabled = false;
        
    }

    public IEnumerator MoveToExit()
    {
        if (Exit == null)
        {
            Debug.LogError("El punto de salida no está asignado.");
            yield break;
        }

        float speed = 3f;
        float rotationSpeed = 5f;

        yield return new WaitUntil(() =>
        {

            if (this == null || gameObject == null || Exit == null)
            {
                return true; 
            }


            Vector3 directionToExit = (Exit.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToExit);

            transform.position = Vector3.MoveTowards(transform.position, Exit.position, speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            return Vector3.Distance(transform.position, Exit.position) <= 0.1f; 
        });

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
