using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireBulletOnActivate : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn;
    public float fireSpeed = 20;

    // Start is called before the first frame update
    void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(FireBullet);
    }

    public void FireBullet(ActivateEventArgs args)
    {
        GameObject spawnBullet = Instantiate(bullet);
        spawnBullet.transform.position = bulletSpawn.position;
        spawnBullet.GetComponent<Rigidbody>().velocity = bulletSpawn.forward * fireSpeed;
        Destroy(spawnBullet, 5.0f);
    }
}
