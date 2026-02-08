using UnityEngine;
using UnityEngine.SceneManagement; // ใช้สำหรับจัดการ Scene
using UnityEngine.UI;              // ใช้สำหรับ UI Button

public class LoadNextScene : MonoBehaviour
{
    [SerializeField] private Button nextSceneButton; // ปุ่ม UI ที่เราจะกด
    [SerializeField] private string sceneName;       // ชื่อฉากที่จะโหลด

    void Start()
    {
        // เชื่อมปุ่มกับฟังก์ชัน
        if (nextSceneButton != null)
        {
            nextSceneButton.onClick.AddListener(LoadScene);
        }
    }

    void LoadScene()
    {
        // โหลดฉากใหม่ตามชื่อที่กำหนด
        SceneManager.LoadScene(sceneName);
    }
}
