using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    
    private DatabaseReference dbRef;

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        if(Instance == null )
        { Instance = this; }
    }

    public async void CreateUser(string name)
    {
        DataSnapshot snapshot = await (UserSnapshot(name));

        //Si ya existe, no se crea
        if (snapshot.Exists)
        {
            UnityEngine.Debug.LogWarning("Ya existe este usuario");
            return;
        }

        //Crear usuario
        User newUser = new User(name, 0, 0);
        string json = JsonUtility.ToJson(newUser);

        await dbRef.Child("users").Child(name).SetRawJsonValueAsync(json);
    }

    private async Task<DataSnapshot> UserSnapshot(string name)
    {
        DatabaseReference userRef = dbRef.Child("users").Child(name);

        DataSnapshot snapshot = await userRef.GetValueAsync();

        return snapshot;
    }

    public async void AddWin(string name)
    {
        await ModifyWins(name);
    }

    private async Task ModifyWins(string name)
    {
        DatabaseReference userRef = dbRef.Child("users").Child(name);
        DataSnapshot snapshot = await userRef.Child("wins").GetValueAsync();

        int currentWins = snapshot.Exists ? int.Parse(snapshot.Value.ToString()) : 0;

        int newWins = currentWins + 1;

        await userRef.UpdateChildrenAsync(new System.Collections.Generic.Dictionary<string, object>{
            { "wins", newWins }
        });

        UnityEngine.Debug.Log("Win added correctamente");
    }

    public async void AddLoose(string name)
    {
        await ModifyLoose(name);
    }

    private async Task ModifyLoose(string name)
    {
        DatabaseReference userRef = dbRef.Child("users").Child(name);
        DataSnapshot snapshot = await userRef.Child("looses").GetValueAsync();

        int currentLooses = snapshot.Exists ? int.Parse(snapshot.Value.ToString()) : 0;

        int newLooses = currentLooses + 1;

        await userRef.UpdateChildrenAsync(new System.Collections.Generic.Dictionary<string, object>{
            { "looses", newLooses }
        });

        UnityEngine.Debug.Log("looses added correctamente");
    }
}

public class User
{
    public string name;
    public int wins;
    public int looses;

    public User(string name, int wins, int looses)
    {
        this.name = name;
        this.wins = wins;
        this.looses = looses;
    }
}