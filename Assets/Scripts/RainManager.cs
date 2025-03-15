using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainManager : MonoBehaviour
{
    private GameManager _gm;
    public bool isRain;
    // Start is called before the first frame update
    void Start()
    {
        _gm = FindObjectOfType(typeof(GameManager)) as GameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            _gm.OnOffRain(isRain);
        }
    }
}
