
using System;

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
}

[Serializable]
public class DialogueData
{
    public DialogueLine[] dialogues;
}
