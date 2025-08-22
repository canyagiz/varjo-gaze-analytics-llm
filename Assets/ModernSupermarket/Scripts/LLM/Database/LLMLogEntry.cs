using SQLite4Unity3d;

public class LLMLogEntry
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    public int sessionId { get; set; }

    public string objectName { get; set; }
    public float area { get; set; }
    public float volume { get; set; }
    public string visualInsight { get; set; }
    public string llmReply { get; set; }
    public string timestamp { get; set; }


}
