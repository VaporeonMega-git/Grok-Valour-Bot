using System.Collections;
using System.Net.Http;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace Pkmn {
    public static class Pkmn
    {

        private static JsonElement? pokedex = null;

        private static async Task<JsonElement> requestPokedex()
        {
            if (pokedex == null) {
                using HttpClient client = new HttpClient();

                string json = await client.GetStringAsync("https://play.pokemonshowdown.com/data/pokedex.json");

                JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
                pokedex = data;
                
                return data;
            } else {
                return (JsonElement) pokedex;
            }
        }

        private static async Task<String> pokedexEntry(JsonElement pokemon)
        {
            int natdex = pokemon.GetProperty("num").GetInt32();
            string name = pokemon.GetProperty("name").GetString();
            List<string> types = GetStringList(pokemon.GetProperty("types"));
            
            int hp = pokemon.GetProperty("baseStats").GetProperty("hp").GetInt32();
            int atk = pokemon.GetProperty("baseStats").GetProperty("atk").GetInt32();
            int def = pokemon.GetProperty("baseStats").GetProperty("def").GetInt32();
            int spa = pokemon.GetProperty("baseStats").GetProperty("spa").GetInt32();
            int spd = pokemon.GetProperty("baseStats").GetProperty("spd").GetInt32();
            int spe = pokemon.GetProperty("baseStats").GetProperty("spe").GetInt32();
            int bst = hp + atk + def + spa + spd + spe;

            List<string> abilities = [];
            if (pokemon.GetProperty("abilities").TryGetProperty("0", out JsonElement ability0)) abilities.Add(ability0.GetString());
            if (pokemon.GetProperty("abilities").TryGetProperty("1", out JsonElement ability1)) abilities.Add(ability1.GetString());
            if (pokemon.GetProperty("abilities").TryGetProperty("H", out JsonElement abilityH)) abilities.Add(abilityH.GetString());

            double heightm = pokemon.GetProperty("heightm").GetDouble();
            double weightkg = pokemon.GetProperty("weightkg").GetDouble();

            string output = $"{name} #{natdex}";
            if (types.Count == 1) {
                output += $"\nType: {types[0]}";
            } else {
                output += $"\nTypes: {types[0]}, {types[1]}";
            }
            output += $"\nHeight: {heightm}m";
            output += $"\nWeight: {weightkg}kg";
            output += $"\nAbilities: ";
            foreach (string ability in abilities)
            {
                output += ability + ", ";
            }
            output = output.Substring(0, output.Length - 2);
            output += $"\nStats:";
            output += $"\n- HP: {hp}";
            output += $"\n- ATK: {atk}";
            output += $"\n- DEF: {def}";
            output += $"\n- SPA: {spa}";
            output += $"\n- SPD: {spd}";
            output += $"\n- SPE: {spe}";
            output += $"\n- BST: {bst}";
            
            return output;
        }

        public static async Task<string?> pokedexEntry(string pokemonName)
        {
            JsonElement data = await requestPokedex();
            pokemonName = pokemonName.ToLower();

            // by name
            foreach (JsonProperty property in data.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                if (key == pokemonName)
                {
                    return await pokedexEntry(value);
                }
            }

            // by natdex
            foreach (JsonProperty property in data.EnumerateObject()) {
                string key = property.Name;
                JsonElement value = property.Value;

                if (value.GetProperty("num").GetInt32().ToString() == pokemonName)
                {
                    return await pokedexEntry(value);
                }
            }

            return null;
        }

        public static List<string> GetStringList(JsonElement element)
        {
            var result = new List<string>();

            foreach (var item in element.EnumerateArray())
            {
                result.Add(item.GetString()!);
            }

            return result;
        }
    }
}