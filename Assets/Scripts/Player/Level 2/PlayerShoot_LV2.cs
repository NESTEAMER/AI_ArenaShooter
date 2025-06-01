using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerShoot_LV2 : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject gunShotNoise;
    public GameObject distractionPrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    //health stuff
    public float currentHealth = 100f;
    public TextMeshProUGUI healthText;

    public TextMeshProUGUI killText;
    public int killCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        healthText.text = "Health: " + currentHealth.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        killText.text = $"Kills: {killCount}";
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    public void OnThrow(InputValue value)
    {
        if (value.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Throw();
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        GameObject gunShot = Instantiate(gunShotNoise, firePoint.position, firePoint.rotation);
        Destroy(gunShot, 0.2f);

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = firePoint.up * projectileSpeed;
        Destroy(projectile, 3f);
    }

    void Throw()
    {
        GameObject projectile = Instantiate(distractionPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = firePoint.up * projectileSpeed;
        Destroy(projectile, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            currentHealth -= 10;
            healthText.text = "Health: " + currentHealth.ToString();
            Destroy(other.gameObject);

            if (currentHealth <= 0)
            {
                SceneManager.LoadScene("Game 2");
            }
        }
    }
    
    public void KillCount()
    {
        killCount++;
    }
}
