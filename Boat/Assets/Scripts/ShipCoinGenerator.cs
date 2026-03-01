using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCoinGenerator : MonoBehaviour
{
    [Header("Coin Settings")]
    public float coinsPerSecond = 1f;
    public float minimunSpeedToEarn = 0.5f;

    [Header("Speed Bonus")]
    public bool enableSpeedBonus = true;
    public float bonusSpeedThreshold = 15f;
    public float bonusMultiplier = 2f;

    //Had some troubles added this for debugging
    [Header("Debug")]
    public bool showDebugMessages = false;

    private Rigidbody rb;
    private ShipController shipController;
    private float coinAccumulator = 0f;
    private float debugTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        shipController = GetComponent<ShipController>();

        if(rb == null)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        float currentSpeed = GetShipSpeed();

        if(showDebugMessages)
        {
            debugTimer += Time.deltaTime;
            if(debugTimer >= 1f)
            {
                string msg = "Ship SPeed: " + currentSpeed.ToString("F2");

                if(GameManager.Instance != null)
                {
                    msg += "Coins: " + GameManager.Instance.coins.ToString("F0");
                }

                if (currentSpeed >= minimunSpeedToEarn)
                {
                    msg += " (Earning)";
                }else
                {
                    msg += " (Too Slow)";
                }

                Debug.Log(msg);
                debugTimer = 0f;
            }
        }
        if(currentSpeed >= minimunSpeedToEarn)
        {
            float coinRate = coinsPerSecond;

            if(enableSpeedBonus && currentSpeed >= bonusSpeedThreshold)
            {
                coinRate += bonusMultiplier;

                if(showDebugMessages && Time.frameCount % 60 == 0)
                {
                    Debug.Log("Speed Bonus Active! Coin Rate is now: " + coinRate); 
                }
            }
            coinAccumulator += coinRate * Time.deltaTime;

            while(coinAccumulator >= 1f)
            {
                coinAccumulator -= 1f;
                GiveCoin();
            }
        }
    }

    float GetShipSpeed()
    {
        if(shipController != null)
        {
            return Mathf.Abs(shipController.GetCurrentSpeed());
        }

        if(rb != null)
        {
            return rb.velocity.magnitude;
        }

        return 0f;
    }

    void GiveCoin()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.coins += 1f;

            if(showDebugMessages)
            {
                Debug.Log("Coin total: " + GameManager.Instance.coins);
            }
        }
        else
        {
            if(showDebugMessages)
            {
                Debug.LogWarning("No GameManager.");
            }
        }
    }
}
