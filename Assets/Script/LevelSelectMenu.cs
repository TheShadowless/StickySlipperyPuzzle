using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelSelectMenu : MonoBehaviour
{
    private static Sprite cachedWhiteSprite;
    private bool isBuilding;

    private readonly string[] levelScenes =
    {
        "Level_1",
        "Level_2",
        "Level_3",
        "",
        "",
        ""
    };

    private void OnEnable()
    {
        if (Application.isPlaying)
            AnalyticsManager.Instance?.OnLevelStart();

        BuildMenu();
    }

    private void BuildMenu()
    {
        if (isBuilding)
            return;

        RectTransform canvasRect = transform as RectTransform;
        if (canvasRect == null)
            return;

        isBuilding = true;
        ClearChildren(canvasRect);

        CreateBackground(canvasRect);
        RectTransform board = CreateBoard(canvasRect);
        CreateTitle(board);
        CreateLevelButtons(board);
        CreateSideButton(canvasRect, "<", new Vector2(-300f, 0f), () => SceneManager.LoadScene("MainMenu"), true);
        CreateSideButton(canvasRect, ">", new Vector2(300f, 0f), null, false);
        CreateHint(canvasRect);
        isBuilding = false;
    }

    private void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(parent.GetChild(i).gameObject);
            else
                DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private void CreateBackground(RectTransform parent)
    {
        GameObject background = CreateImageObject("Background", parent, new Color(0.96f, 0.64f, 0.79f), Vector2.zero, new Vector2(1280f, 720f));
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = GetWhiteSprite();
        backgroundImage.type = Image.Type.Sliced;
    }

    private RectTransform CreateBoard(RectTransform parent)
    {
        GameObject shadow = CreateImageObject("BoardShadow", parent, new Color(0.81f, 0.56f, 0.7f, 0.45f), new Vector2(8f, -10f), new Vector2(700f, 470f));
        shadow.GetComponent<Image>().sprite = GetWhiteSprite();

        GameObject frame = CreateImageObject("BoardFrame", parent, new Color(0.98f, 0.97f, 0.95f), Vector2.zero, new Vector2(700f, 470f));
        Image frameImage = frame.GetComponent<Image>();
        frameImage.sprite = GetWhiteSprite();

        AddOutline(frame, new Color(0.95f, 0.84f, 0.65f), new Vector2(6f, -6f));

        GameObject innerPanel = CreateImageObject("InnerPanel", frame.transform as RectTransform, new Color(0.99f, 0.92f, 0.77f), new Vector2(0f, -18f), new Vector2(600f, 330f));
        Image innerPanelImage = innerPanel.GetComponent<Image>();
        innerPanelImage.sprite = GetWhiteSprite();
        AddOutline(innerPanel, new Color(0.95f, 0.73f, 0.44f), new Vector2(4f, -4f));

        return innerPanel.transform as RectTransform;
    }

    private void CreateTitle(RectTransform parent)
    {
        GameObject ribbonShadow = CreateImageObject("TitleShadow", parent, new Color(0.62f, 0.1f, 0.16f, 0.45f), new Vector2(0f, 175f), new Vector2(320f, 92f));
        ribbonShadow.GetComponent<Image>().sprite = GetWhiteSprite();

        GameObject ribbon = CreateImageObject("TitleRibbon", parent, new Color(0.9f, 0.14f, 0.2f), new Vector2(0f, 184f), new Vector2(320f, 92f));
        ribbon.GetComponent<Image>().sprite = GetWhiteSprite();
        AddOutline(ribbon, new Color(1f, 0.33f, 0.4f), new Vector2(4f, -4f));

        CreateText("TitleText", ribbon.transform as RectTransform, "LEVEL SELECT", 38, new Color(1f, 0.99f, 0.95f), Vector2.zero, new Vector2(280f, 60f));
    }

    private void CreateLevelButtons(RectTransform parent)
    {
        float startX = -165f;
        float startY = 70f;
        float xSpacing = 165f;
        float ySpacing = 160f;
        int unlockedLevels = GameProgress.GetUnlockedLevels();

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int column = i % 3;
            Vector2 position = new Vector2(startX + (column * xSpacing), startY - (row * ySpacing));
            bool unlocked = !string.IsNullOrEmpty(levelScenes[i]) && i < unlockedLevels;
            string label = unlocked ? (i + 1).ToString() : "LOCK";

            try
            {
                CreateLevelButton(parent, position, label, levelScenes[i], unlocked);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to build level button {label}: {ex.Message}", this);
            }
        }
    }

    private void CreateLevelButton(RectTransform parent, Vector2 anchoredPosition, string label, string sceneName, bool unlocked)
    {
        Color shadowColor = unlocked ? new Color(0.18f, 0.67f, 0.82f, 0.8f) : new Color(0.55f, 0.63f, 0.7f, 0.55f);
        Color buttonColor = unlocked ? new Color(0.28f, 0.86f, 0.98f) : new Color(0.77f, 0.83f, 0.88f);
        Color highlightColor = unlocked ? new Color(0.64f, 0.96f, 1f) : new Color(0.86f, 0.89f, 0.92f);

        GameObject shadow = CreateImageObject($"LevelButtonShadow_{label}", parent, shadowColor, anchoredPosition + new Vector2(0f, -10f), new Vector2(120f, 120f));
        shadow.GetComponent<Image>().sprite = GetWhiteSprite();

        GameObject buttonObject = new GameObject($"LevelButton_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = 5;
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(120f, 120f);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = unlocked;

        if (unlocked)
            button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));

        AddOutline(buttonObject, Color.white, new Vector2(6f, 6f));

        Text buttonText = CreateText($"Label_{label}", rect, label, unlocked ? 44 : 24, Color.white, Vector2.zero, new Vector2(100f, 70f));
        if (!unlocked)
            buttonText.color = new Color(0.35f, 0.46f, 0.55f);
    }

    private void CreateSideButton(RectTransform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action, bool interactable)
    {
        GameObject buttonObject = new GameObject($"SideButton_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = 5;
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(86f, 86f);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = interactable ? new Color(0.28f, 0.86f, 0.98f) : new Color(0.77f, 0.83f, 0.88f, 0.8f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = interactable;

        if (action != null)
            button.onClick.AddListener(action);

        AddOutline(buttonObject, Color.white, new Vector2(4f, 4f));
        CreateText($"SideLabel_{label}", rect, label, 40, Color.white, Vector2.zero, new Vector2(60f, 60f));
    }

    private void CreateHint(RectTransform parent)
    {
        Text hint = CreateText("HintText", parent, "Choose a stage to play", 24, new Color(0.52f, 0.22f, 0.34f), new Vector2(0f, -290f), new Vector2(320f, 40f));
        hint.alignment = TextAnchor.MiddleCenter;
    }

    private GameObject CreateImageObject(string name, RectTransform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = color;

        return gameObject;
    }

    private Text CreateText(string name, RectTransform parent, string content, int fontSize, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        textObject.layer = 5;
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        Outline outline = textObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.13f, 0.33f, 0.45f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private void AddOutline(GameObject gameObject, Color color, Vector2 distance)
    {
        Outline outline = gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite != null)
            return cachedWhiteSprite;

        cachedWhiteSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        return cachedWhiteSprite;
    }
}
