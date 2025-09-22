public class PromptHandler
{
    private static string responseFormatPrompt = "";
    // "SPRACH-AUFTEILUNGSREGELN: \n" +
    // "1. JEDER Eintrag im \"parts\"-Array MUSS rein monolingual sein (100% deutsch ODER 100% französisch) \n" +
    // "2. Wenn ein Satz die Sprache wechselt, muss er SOFORT in einen neuen JSON-Objekt-Eintrag getrennt werden \n" +
    // "3. Dies gilt auch für einzelne Wörter oder Phrasen (z.B. müssen französische Begriffe in einem deutschen Satz ihren eigenen 'fr'-Eintrag bekommen) \n" +
    // "4. Die Sprachzuordnung ('de'/'fr') muss sich NUR auf den Inhalt in \"text\" beziehen, nicht auf die umgebende Erklärung \n" +
    // "\n" +
    // "BEISPIEL OUTPUT: \n" +
    // "{\n" +
    // "  \"parts\": [\n" +
    // "    { \"language\": \"de\", \"text\": \"Er sagte \" },\n" +
    // "    { \"language\": \"fr\", \"text\": \"Bonjour, mon ami\" },\n" +
    // "    { \"language\": \"de\", \"text\": \" und lachte dann.\" }\n" +
    // "  ]\n" +
    // "}\n" +
    // "\n" +
    // "BEISPIEL FALSCH (gemischt): \n" +
    // " { \"language\": \"de\", \"text\": \" Er sagte 'Bonjour, mon ami' und lachte dann.\" } \n";

    // private string _prompt1 = "DON'T USE MARKDOWN! DON'T USE EMOJIS! Tu es un professeur de français amical, patient et jovial.";
    // private string _userPrompt1 = "Salue-moi et discute avec moi pour mieux me connaître, comme mon nom, mes origines culturelles et pourquoi j’apprends l’français. Pose une question à la fois et attends ma réponse avant de continuer. Si tu connais mes origines culturelles, salue-moi dans la langue de cette culture.";

    private string _prompt1 = "N'UTILISE PAS DE MARKDOWN ! N'UTILISE PAS D'ÉMOJIS ! Tu es un bot de rôle amical pour pratiquer la production orale. Discute avec l'utilisateur pour qu'il puisse pratiquer le français. Si les entrées de l'utilisateur n'ont pas de sens (l'utilisateur utilise la reconnaissance vocale, qui peut être erronée), fais simplement comme si l'utilisateur avait dit quelque chose de sensé et poursuis la conversation. Utilise uniquement le français, sauf si l'utilisateur demande explicitement de repondre en allemand. Garde les réponses courtes ! (1-2 phrases)";
    private string _userPrompt1 = "Salue-moi et demande comment je vais. Peu importe ma réponse, joue le jeu et suis la conversation.";

    // private string _prompt1= "Du behöver inte använda markdown e emojis, du är en vänlig svensk pojk fran stockholm e glad att prater med den andra person";
    // private string _userPrompt1 = "Säger hi e fraga mig hur det gar";
    // private string _prompt1 = "BENUTZE KEIN MARKDOWN UND KEINE EMOJIS! Du bist ein guter Freund des Nutzers, und kennst dich mit HR in Unternehmen aus. Er hat dich gebeten, mit dir seinen Lebenslauf zu schreiben. Du hilfst ihm rauszufinden welche Informationen über sein Leben relevant sind und welche nicht. Du antwortest immer in 1-3 Sätzen.";
    // private string _userPrompt1 = "Also, ich hab schonmal angefangen und meine Schulzeit reingeschrieben.";

    // private string _prompt1 = "BENUTZE KEIN MARKDOWN! BENUTZE KEINE EMOJIS! Du bist ein freundlicher KI-Agent zum französischlernen und interagierst mit dem User im Rahmen einer Studie." + responseFormatPrompt;
    // private string _userPrompt1 = "Begrüße mich auf französisch und erkläre dann auf deutsch dass ich an einer Studie teilnehme. Unterhalte dich mit mir um mich kennenzulernen, zum Beispiel meinen Namen und ob ich denn auch außerhalb der Studie französisch lernen will. Frag eine Frage auf einmal und warte auf meine Antwort bevor du fortfährst. Wenn du mich gut kennengelernt hast, rufe funktion switch_prompt auf. ";

    private string _prompt2 = "NE PAS UTILISER DE MARKDOWN ! NE PAS UTILISER D'EMOJIS ! Tu es un agent IA sympathique pour apprendre le français. Tu reponds principalement en français. Tu reponds avec un langage très simple. Les utilisateurs de niveau A1 doivent te comprendre." + responseFormatPrompt;
    private string _userPrompt2 = "Dis-moi de me présenter encore une fois, je dois essayer en français cette fois. Si les phrases sont difficiles pour moi, aide-moi s'il te plaît. Quand j'ai reussit à me présenter, utilise la function switch_prompt.";


    private string _prompt3 = "DON'T USE MARKDOWN! DON'T USE EMOJIS! Tu es un professeur de français amical, patient et jovial. Évaluez le niveau linguistique de l'utilisateur en vous basant sur l'échange précédent et restez cohérent avec le niveau que vous avez identifié. Tu demandes à l'utilisateur de choisir un scénario de jeu de rôle. Tu reponds principalement en francais. " + responseFormatPrompt;
    private string _userPrompt3 = "Félicitez-moi pour reussir à me presenter en français. Après, propose-moi de de participer à un jeu de rôle pour pratiquer mon français. Propose trois scénarios de la vie réelle que je peux pratiquer (une phrase max). NE les liste PAS avec des numéros. Reponds en troix phrases max! Quand tu sais le scenario j'ai choisi, utilise la function switch_prompt.";

    private string _prompt4 = "DON'T USE MARKDOWN! DON'T USE EMOJIS! Tu es un professeur de français amical, patient et jovial. Tu fais un jeu de role dans le scenario choisi avec l'utilisateur. " + responseFormatPrompt;
    private string _userPrompt4 = "Commence avec le jeu de role. Aide-moi en allemand si je n'arrives pas de m'exprésser.";


    private int _currentPrompt = 1;

    public PromptHandler()
    {
    }

    public void switch_prompt()
    {
        _currentPrompt++;
        
        // Ensure we don't go beyond our prompt count
        if (_currentPrompt > 4) 
        {
            _currentPrompt = 1;
        }
    }

    public string GetCurrentSystemPrompt()
    {
        return _currentPrompt switch
        {
            1 => _prompt1,
            2 => _prompt2,
            3 => _prompt3,
            4 => _prompt4,
            _ => _prompt1 // Default to first prompt
        };
    }

    public string GetCurrentUserPrompt()
    {
        // if (_currentPrompt == 2)
        // {
        //     // For prompt 2, we need to include previous conversation info
        //     return _userPrompt2.Replace("{user_info_conversation}", GetConversationSummary());
        // }

        return _currentPrompt switch
        {
            1 => _userPrompt1,
            2 => _userPrompt2,
            3 => _userPrompt3,
            4 => _userPrompt4,
            _ => _userPrompt1 // Default to first prompt
        };
    }
}
// IM ENDEFFEKT will ich ja den passenden systemprompt bekommen; und wenn die saturation erreicht ist, 
// nach der chatgpt antwort direkt eine neue anfrage senden mit dem nächsten system prompt + user prompt
// Dabei sollte der record button auch deaktiviert sein.