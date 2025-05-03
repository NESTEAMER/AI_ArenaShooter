using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerShoot : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    //health stuff
    public float currentHealth = 100f;
    public TextMeshProUGUI healthText;


    // Start is called before the first frame update
    void Start()
    {
        healthText.text = "Health: "+ currentHealth.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFire(InputValue value)
    {
        if(value.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = firePoint.up * projectileSpeed;
        Destroy(projectile, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            currentHealth -= 10;
            healthText.text = "Health: "+ currentHealth.ToString();
            Destroy(other);

            if(currentHealth <= 0)
            {
                SceneManager.LoadScene("Game");
            }
        }
    }
}
