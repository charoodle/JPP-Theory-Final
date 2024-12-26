using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


public class JsonSaveLoadTest : MonoBehaviour
{
    [SerializeField] bool saveInfo;

    [SerializeField]
    [TextArea(20,25)]
    string wouldBeOnDisk;


    public class TestObjectSerialization
    {
        // Serializable
        public string playerName = "asdf";
        public float lookSens = 1.45f;
        public float screenShakeMultiplier = 0.5f;

        public int num_int = 5;
        public double num_double = 0.12345678901234567890;
        public List<Player> players = 
        new List<Player>
            (new Player[]
                {
                    new Player("p0", 100f),
                    new Player("p1", 200f),
                    new Player("p3", 300f),
                }
            );

        
        // Nonserializable
        protected float num_float_prot = 3f;
        [SerializeField]
        protected float num_float_prot_serializefield = 3f;

        // Helper class
        public class Player
        {
            public string name;
            public float health;
            public Player(string name, float health)
            {
                this.name = name;
                this.health = health;
            }
        }
    }

    private void OnValidate()
    {
        if(saveInfo && Application.isPlaying)
        {
            saveInfo = false;
            TestObjectSerialization charInfo = new TestObjectSerialization();
            Save(charInfo);
        }
    }

    public void Save<T>(T obj)
    {
        // Tested, looks ok
        // Serialize the object into a string, visible in inspector
        string str = JsonConvert.SerializeObject(obj, Formatting.Indented);
        wouldBeOnDisk = str;
    }

    public T Load<T>(T obj)
    {
        // TODO: (Untested)
        // Load the object from the string
        T deserializedObject = JsonConvert.DeserializeObject<T>(wouldBeOnDisk);
        return deserializedObject;
    }
}
