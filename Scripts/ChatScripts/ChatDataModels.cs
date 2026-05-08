using System;

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatRequest
{
    public string model = "qwen-plus";
    public Message[] messages;
    public float temperature = 0.85f;
    public float top_p = 0.9f;
    public int max_tokens = 800;
    public float presence_penalty = 0.3f;
    public float frequency_penalty = 0.3f;
}

[Serializable]
public class ChatResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}