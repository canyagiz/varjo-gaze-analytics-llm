using SQLite4Unity3d;
using System.IO;
using UnityEngine;

public static class LLMDatabase
{
    private static SQLiteConnection db;

    public static void Initialize()
    {
        if (db != null) return;

        string dbPath = Path.Combine(Application.persistentDataPath, "llm_logs.db");
        db = new SQLiteConnection(dbPath);

        db.CreateTable<LLMLogEntry>();
        Debug.Log($"[LLMDatabase] Initialized at {dbPath}");
    }

    public static void InsertLog(LLMLogEntry entry)
    {
        if (db == null) Initialize();

        db.Insert(entry);
        Debug.Log($"[LLMDatabase] Inserted log for object: {entry.objectName}");
    }
}
