using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Gun : MonoBehaviour
{
    public FirstPersonController Player;

    public Animator ArmAnim;
    public Animator Anim;
    public GameObject Crosshair;
    public GameObject TheGun;

    public float MaxAmmo;
    public float ClipSize;
    public float LoadedBullets;

    public Text MaxAmmoText;
    public Text LoadedText;

    public ParticleSystem mussleFlash;
    public GameObject ImpactEffect;
    public AudioSource FireSound;
    public AudioSource ReloadSound;

    public float fireRate = 15f;
    public float ImpactForce = 30;
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;

    public Transform camTransform;
    public float shakeDuration = 0f;
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;

    public Transform GunPos;
    public Transform LowerPos;
    public Transform HipPos;

    public GameObject HitMarker;

    public bool IsRunning;

    private float nextTimeToFire = 0f;

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    private void Start()
    {

    }


    public void Sprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Anim.enabled = true;
            Anim.Play("Sprint");
        }
        
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Anim.enabled = true;
            Anim.Play("Idle");
            StartCoroutine("DisableAnimator");
        }
    }
        void Update()
        {
            Inspect();
            Sprint();
            LoadedText.text = LoadedBullets.ToString();
            MaxAmmoText.text = MaxAmmo.ToString();


            if (LoadedBullets > 0)
            {
                if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
                {
                    nextTimeToFire = Time.time + 1f / fireRate;
                    Shoot();
                }
            }

        if (Player.m_IsWalking == true)
        {
            if (MaxAmmo > 0)
            {
                if (Input.GetButtonDown("Reload"))
                {
                    Anim.enabled = true;
                    Anim.Play("Reload");
                    ArmAnim.Play("Reload");
                }
            }
        }
        }


    void Shoot()
    {
        if (Player.m_IsWalking == true)
        {


            LoadedBullets -= 1;

            if (shakeDuration > 0)
            {
                camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

                shakeDuration -= Time.deltaTime * decreaseFactor;
            }
            else
            {
                shakeDuration = 0f;
                camTransform.localPosition = originalPos;
            }

            mussleFlash.Play();
            FireSound.Play();
            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward
                , out hit, range))
            {
                Debug.Log(hit.transform.name);
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.OnHit(hit.point, hit.normal, damage);
                    Instantiate(HitMarker);
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * ImpactForce);
                }
                Instantiate(ImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

        public void PlayReloadSound()
        {
            ReloadSound.Play();
        }

    public void Reload()
    {
        
            if (MaxAmmo < ClipSize)
            {
                LoadedBullets = MaxAmmo;
            }
            else
            {
                MaxAmmo -= ClipSize;
                MaxAmmo += LoadedBullets;
                LoadedBullets = ClipSize;
            }
        Anim.Play("Idle");
        StartCoroutine("DisableAnimator");

    }


    public void Inspect()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ArmAnim.Play("Inspect");
        }

    }

    private IEnumerator DisableAnimator()
    {
        yield return new WaitForSeconds(0.2f);
        Anim.enabled = false;
    }
 } 
