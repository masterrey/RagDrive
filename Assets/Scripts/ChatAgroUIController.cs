using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatAgroUIController : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    public StyleSheet styleSheet; // arraste o .uss aqui se preferir (opcional)

    VisualElement root;

    // Main
    Button btnChatbot, btnPhoto, btnWeather, btnTips, btnNewChat;
    VisualElement recentList;

    // Bottom nav
    Button navHome, navChat, navProfile;

    // Chat overlay
    VisualElement chatScreen;
    Button btnBack, btnSend, btnClear;
    TextField inputField;
    ScrollView chatScroll;
    VisualElement chatMessages;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        // Query elements
        btnChatbot = root.Q<Button>("BtnChatbot");
        btnPhoto = root.Q<Button>("BtnPhoto");
        btnWeather = root.Q<Button>("BtnWeather");
        btnTips = root.Q<Button>("BtnTips");
        btnNewChat = root.Q<Button>("BtnNewChat");

        recentList = root.Q<VisualElement>("RecentList");

        navHome = root.Q<Button>("NavHome");
        navChat = root.Q<Button>("NavChat");
        navProfile = root.Q<Button>("NavProfile");

        chatScreen = root.Q<VisualElement>("ChatScreen");
        btnBack = root.Q<Button>("BtnBack");
        btnSend = root.Q<Button>("BtnSend");
        btnClear = root.Q<Button>("BtnClear");
        inputField = root.Q<TextField>("InputField");
        chatScroll = root.Q<ScrollView>("ChatScroll");
        chatMessages = root.Q<VisualElement>("ChatMessages");

        WireEvents();
        SeedRecentChats();
        SeedWelcomeMessage();
        SetActiveNav(navHome);
        HideChat();
    }

    void WireEvents()
    {
        btnChatbot.clicked += ShowChat;
        navChat.clicked += ShowChat;

        btnBack.clicked += HideChat;
        btnClear.clicked += () =>
        {
            chatMessages.Clear();
            SeedWelcomeMessage();
            ScrollToBottom();
        };

        btnSend.clicked += SendUserMessage;

        // Enter para enviar
        inputField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SendUserMessage();
                evt.StopPropagation();
            }
        });

        btnNewChat.clicked += () =>
        {
            ShowChat();
            AddBotBubble("Nova conversa iniciada. Como posso ajudar na sua lavoura hoje?");
        };

        btnPhoto.clicked += () => Toast("Placeholder: Diagnóstico por foto (frontend)");
        btnWeather.clicked += () => Toast("Placeholder: Clima (frontend)");
        btnTips.clicked += () => Toast("Placeholder: Recomendações (frontend)");

        navHome.clicked += () => { SetActiveNav(navHome); HideChat(); };
        navProfile.clicked += () => { SetActiveNav(navProfile); Toast("Placeholder: Perfil (frontend)"); };
    }

    void SeedRecentChats()
    {
        recentList.Clear();
        AddRecent("Milho: adubação de cobertura", "Última mensagem: ontem");
        AddRecent("Pragas na soja", "Última mensagem: 3 dias atrás");
        AddRecent("Irrigação: gotejamento", "Última mensagem: semana passada");
    }

    void AddRecent(string title, string subtitle)
    {
        var item = new VisualElement();
        item.AddToClassList("recent-item");

        var t = new Label(title);
        t.AddToClassList("recent-title");

        var s = new Label(subtitle);
        s.AddToClassList("recent-sub");

        item.Add(t);
        item.Add(s);

        item.RegisterCallback<ClickEvent>(_ =>
        {
            ShowChat();
            AddBotBubble($"Abrindo conversa: \"{title}\". (frontend) Pergunte algo para continuar.");
        });

        recentList.Add(item);
    }

    void SeedWelcomeMessage()
    {
        AddBotBubble("Oi! Eu sou o Chat Agro (frontend). Pergunte sobre pragas, manejo, adubação, plantio e colheita.");
    }

    void ShowChat()
    {
        SetActiveNav(navChat);
        chatScreen.RemoveFromClassList("hidden");
        inputField.Focus();
        ScrollToBottom();
    }

    void HideChat()
    {
        chatScreen.AddToClassList("hidden");
    }

    void SendUserMessage()
    {
        var text = (inputField.value ?? "").Trim();
        if (string.IsNullOrEmpty(text))
            return;

        AddUserBubble(text);
        inputField.value = "";

        // Resposta fake (sem backend)
        AddBotBubble(FakeBotReply(text));
        ScrollToBottom();
    }

    string FakeBotReply(string userText)
    {
        var t = userText.ToLowerInvariant();

        if (t.Contains("soja")) return "Soja: você quer falar de pragas, fertilidade do solo ou calendário de aplicação?";
        if (t.Contains("milho")) return "Milho: me diga a fase da cultura e a região, e eu te ajudo com manejo (placeholder).";
        if (t.Contains("praga") || t.Contains("lagarta")) return "Pragas: descreva sintomas e estágio. (frontend) Depois você liga no backend para diagnóstico real.";
        if (t.Contains("adubo") || t.Contains("adub")) return "Adubação: qual cultura, análise de solo (pH, P, K) e objetivo de produtividade?";
        if (t.Contains("clima")) return "Clima: (frontend) aqui entraria API de previsão por GPS/município.";
        return "Entendi. Para eu ajudar melhor: qual cultura, região e qual o problema/objetivo específico?";
    }

    void AddUserBubble(string text) => AddBubble(text, isUser: true);
    void AddBotBubble(string text) => AddBubble(text, isUser: false);

    void AddBubble(string text, bool isUser)
    {
        var bubble = new Label(text);
        bubble.AddToClassList("bubble");
        bubble.AddToClassList(isUser ? "user" : "bot");
        chatMessages.Add(bubble);
    }

    void ScrollToBottom()
    {
        // Próximo frame para garantir layout atualizado
        root.schedule.Execute(() =>
        {
            chatScroll.scrollOffset = new Vector2(0, float.MaxValue);
        }).ExecuteLater(1);
    }

    void SetActiveNav(Button active)
    {
        navHome.RemoveFromClassList("active");
        navChat.RemoveFromClassList("active");
        navProfile.RemoveFromClassList("active");

        active.AddToClassList("active");
    }

    void Toast(string msg)
    {
        Debug.Log("[ChatAgroUI] " + msg);
        // Se quiser, dá pra criar um toast visual também (overlay pequeno).
    }
}
