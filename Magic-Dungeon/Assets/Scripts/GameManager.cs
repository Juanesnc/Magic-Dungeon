using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI points;

    private PlayerMovement pm;

    private void Start() {
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    private void Update() {
        points.text = "Score: " + pm.points;
    }
}
