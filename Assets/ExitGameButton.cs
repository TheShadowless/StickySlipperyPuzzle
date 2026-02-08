using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ExitGameButton : MonoBehaviour
{
    [SerializeField] private Button exitButton; // ปุ่ม UI ที่จะใช้กดออกเกม

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        // ถ้าอยู่ใน Unity Editor ให้หยุดการเล่นเกม
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ถ้าเป็นเกมที่ Build แล้ว จะออกเกมจริง ๆ
        Application.Quit();
#endif
    }
}