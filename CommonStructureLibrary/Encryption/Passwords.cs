﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CSL.Encryption
{
    public static class Passwords
    {
        private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        public static string RandomPassGen(int length = 16)
        {
            int templen = length/4 + (length %4 > 0?1:0);
            byte[] toReturn = new byte[templen * 3];
            RNG.GetBytes(toReturn);
            return Convert.ToBase64String(toReturn).Substring(0, length).Replace('/', '_').Replace('+', '-');
        }
        public static string FriendlyPassGen()
        {
            if(CheckAdjectives && CheckPokemon)
            {
                byte[] data = new byte[8];
                RNG.GetBytes(data);
                string word1 = Adjectives[data[0]];
                string word2 = Adjectives[data[1]];
                string word3 = Pokemon[data[2]];
                string word4 = Symbols[data[2] & 0x0F] + "" + (data[5] + ((data[2] & 0xF0) >> 4));
                return word1 + word2 + word3 + word4;
            }
            return null;
        }
        public static readonly string[] Adjectives = new string[]
{
            "Abandoned",
            "Abundant",
            "Accurate",
            "Adamant",
            "Addicted",
            "Adorable",
            "Afraid",
            "Alert",
            "Aloof",
            "Ambitious",
            "Ancient",
            "Angry",
            "Animated",
            "Annoying",
            "Anxious",
            "Arrogant",
            "Ashamed",
            "Awesome",
            "Awful",
            "Bad",
            "Barren",
            "Basal",
            "Based",
            "Baseless",
            "Bashful",
            "Basic",
            "Batty",
            "Beautiful",
            "Best",
            "Biased",
            "Big",
            "Bitter",
            "Bizarre",
            "Black",
            "Blue",
            "Bold",
            "Boring",
            "Brainy",
            "Brave",
            "Bright",
            "Broad",
            "Broken",
            "Busy",
            "Calm",
            "Capable",
            "Careful",
            "Careless",
            "Caring",
            "Cautious",
            "Charming",
            "Cheap",
            "Cheerful",
            "Chubby",
            "Clean",
            "Clever",
            "Clumsy",
            "Cold",
            "Colorful",
            "Concerned",
            "Confused",
            "Crowded",
            "Cruel",
            "Crystal",
            "Curious",
            "Curly",
            "Cute",
            "Daft",
            "Daily",
            "Dainty",
            "Damaged",
            "Damp",
            "Dampish",
            "Dangerous",
            "Dark",
            "Darned",
            "Dauntless",
            "Deep",
            "Defective",
            "Delicate",
            "Delicious",
            "Depressed",
            "Different",
            "Dirty",
            "Docile",
            "Dry",
            "Dusty",
            "Early",
            "Educated",
            "Efficient",
            "Elderly",
            "Elegant",
            "Empty",
            "Excellent",
            "Exciting",
            "Expensive",
            "Fabulous",
            "Fair",
            "Faithful",
            "Famous",
            "Fancy",
            "Fantastic",
            "Fast",
            "Fearful",
            "Fearless",
            "Filthy",
            "Foolish",
            "Forgetful",
            "Friendly",
            "Funny",
            "Gentle",
            "Glamorous",
            "Glorious",
            "Gold",
            "Gorgeous",
            "Graceful",
            "Grateful",
            "Great",
            "Greedy",
            "Green",
            "Handsome",
            "Happy",
            "Hardy",
            "Harsh",
            "Hasty",
            "Healthy",
            "Heavy",
            "Helpful",
            "Hilarious",
            "Horrible",
            "Hot",
            "Huge",
            "Humorous",
            "Hungry",
            "Ignorant",
            "Illegal",
            "Imaginary",
            "Impish",
            "Impolite",
            "Important",
            "Innocent",
            "Jealous",
            "Jolly",
            "Juicy",
            "Kind",
            "Large",
            "Lax",
            "Legal",
            "Light",
            "Literate",
            "Little",
            "Lively",
            "Lonely",
            "Loud",
            "Lovely",
            "Lucky",
            "Macho",
            "Magical",
            "Massive",
            "Mature",
            "Mean",
            "Meek",
            "Messy",
            "Mild",
            "Modern",
            "Modest",
            "Naive",
            "Narrow",
            "Nasty",
            "Naughty",
            "Nervous",
            "New",
            "Noisy",
            "Obedient",
            "Obnoxious",
            "Old",
            "Orange",
            "Peaceful",
            "Pink",
            "Polite",
            "Poor",
            "Powerful",
            "Precious",
            "Pretty",
            "Proud",
            "Purple",
            "Quick",
            "Quiet",
            "Quirky",
            "Rapid",
            "Rare",
            "Rash",
            "Red",
            "Relaxed",
            "Rich",
            "Romantic",
            "Royal",
            "Rude",
            "Sassy",
            "Secretive",
            "Selfish",
            "Serious",
            "Sharp",
            "Shiny",
            "Shocking",
            "Short",
            "Shy",
            "Silly",
            "Silver",
            "Sincere",
            "Skinny",
            "Slim",
            "Slow",
            "Small",
            "Soft",
            "Spicy",
            "Splendid",
            "Strong",
            "Sweet",
            "Tactful",
            "Talented",
            "Tall",
            "Tangible",
            "Tasteful",
            "Tasty",
            "Teachable",
            "Temperate",
            "Tenacious",
            "Tender",
            "Tense",
            "Terrible",
            "Terrific",
            "Terrific",
            "Thankful",
            "Thick",
            "Thin",
            "Timid",
            "Tiny",
            "Ugly",
            "Unique",
            "Untidy",
            "Upset",
            "Violent",
            "Violet",
            "Vulgar",
            "Warm",
            "Weak",
            "Wealthy",
            "Wide",
            "Wise",
            "Witty",
            "Wonderful",
            "Worried",
            "Yellow",
            "Young",
            "Youthful",
            "Zealous"
};
        public static readonly string[] Pokemon = new string[]
        {
            "Bulbasaur",
            "Ivysaur",
            "Venusaur",
            "Charmander",
            "Charmeleon",
            "Charizard",
            "Squirtle",
            "Wartortle",
            "Blastoise",
            "Caterpie",
            "Metapod",
            "Butterfree",
            "Weedle",
            "Kakuna",
            "Beedrill",
            "Pidgey",
            "Pidgeotto",
            "Pidgeot",
            "Rattata",
            "Raticate",
            "Spearow",
            "Fearow",
            "Ekans",
            "Arbok",
            "Pikachu",
            "Raichu",
            "Sandshrew",
            "Sandslash",
            "Nidoran",
            "Nidorina",
            "Nidoqueen",
            "Nidorino",
            "Nidoking",
            "Clefairy",
            "Clefable",
            "Vulpix",
            "Ninetales",
            "Jigglypuff",
            "Wigglytuff",
            "Zubat",
            "Golbat",
            "Oddish",
            "Gloom",
            "Vileplume",
            "Paras",
            "Parasect",
            "Venonat",
            "Venomoth",
            "Diglett",
            "Dugtrio",
            "Meowth",
            "Persian",
            "Psyduck",
            "Golduck",
            "Mankey",
            "Primeape",
            "Growlithe",
            "Arcanine",
            "Poliwag",
            "Poliwhirl",
            "Poliwrath",
            "Abra",
            "Kadabra",
            "Alakazam",
            "Machop",
            "Machoke",
            "Machamp",
            "Bellsprout",
            "Weepinbell",
            "Victreebel",
            "Tentacool",
            "Tentacruel",
            "Geodude",
            "Graveler",
            "Golem",
            "Ponyta",
            "Rapidash",
            "Slowpoke",
            "Slowbro",
            "Magnemite",
            "Magneton",
            "Farfetch'd",
            "Doduo",
            "Dodrio",
            "Seel",
            "Dewgong",
            "Grimer",
            "Muk",
            "Shellder",
            "Cloyster",
            "Gastly",
            "Haunter",
            "Gengar",
            "Onix",
            "Drowzee",
            "Hypno",
            "Krabby",
            "Kingler",
            "Voltorb",
            "Electrode",
            "Exeggcute",
            "Exeggutor",
            "Cubone",
            "Marowak",
            "Hitmonlee",
            "Hitmonchan",
            "Lickitung",
            "Koffing",
            "Weezing",
            "Rhyhorn",
            "Rhydon",
            "Chansey",
            "Tangela",
            "Kangaskhan",
            "Horsea",
            "Seadra",
            "Goldeen",
            "Seaking",
            "Staryu",
            "Starmie",
            "Scyther",
            "Jynx",
            "Electabuzz",
            "Magmar",
            "Pinsir",
            "Tauros",
            "Magikarp",
            "Gyarados",
            "Lapras",
            "Ditto",
            "Eevee",
            "Vaporeon",
            "Jolteon",
            "Flareon",
            "Porygon",
            "Omanyte",
            "Omastar",
            "Kabuto",
            "Kabutops",
            "Aerodactyl",
            "Snorlax",
            "Articuno",
            "Zapdos",
            "Moltres",
            "Dratini",
            "Dragonair",
            "Dragonite",
            "Mewtwo",
            "Mew",
            "Chikorita",
            "Bayleef",
            "Meganium",
            "Cyndaquil",
            "Quilava",
            "Typhlosion",
            "Totodile",
            "Croconaw",
            "Feraligatr",
            "Sentret",
            "Furret",
            "Hoothoot",
            "Noctowl",
            "Ledyba",
            "Ledian",
            "Spinarak",
            "Ariados",
            "Crobat",
            "Chinchou",
            "Lanturn",
            "Pichu",
            "Cleffa",
            "Igglybuff",
            "Togepi",
            "Togetic",
            "Natu",
            "Xatu",
            "Mareep",
            "Flaaffy",
            "Ampharos",
            "Bellossom",
            "Marill",
            "Azumarill",
            "Sudowoodo",
            "Politoed",
            "Hoppip",
            "Skiploom",
            "Jumpluff",
            "Aipom",
            "Sunkern",
            "Sunflora",
            "Yanma",
            "Wooper",
            "Quagsire",
            "Espeon",
            "Umbreon",
            "Murkrow",
            "Slowking",
            "Misdreavus",
            "Unown",
            "Wobbuffet",
            "Girafarig",
            "Pineco",
            "Forretress",
            "Dunsparce",
            "Gligar",
            "Steelix",
            "Snubbull",
            "Granbull",
            "Qwilfish",
            "Scizor",
            "Shuckle",
            "Heracross",
            "Sneasel",
            "Teddiursa",
            "Ursaring",
            "Slugma",
            "Magcargo",
            "Swinub",
            "Piloswine",
            "Corsola",
            "Remoraid",
            "Octillery",
            "Delibird",
            "Mantine",
            "Skarmory",
            "Houndour",
            "Houndoom",
            "Kingdra",
            "Phanpy",
            "Donphan",
            "Stantler",
            "Smeargle",
            "Tyrogue",
            "Hitmontop",
            "Smoochum",
            "Elekid",
            "Magby",
            "Miltank",
            "Blissey",
            "Raikou",
            "Entei",
            "Suicune",
            "Larvitar",
            "Pupitar",
            "Tyranitar",
            "Lugia",
            "Ho-Oh",
            "Celebi",
            "Regirock",
            "Regice",
            "Registeel",
            "Kyogre",
            "Groudon",
            "Rayquaza",
            "Jirachi",
            "Deoxys"
        };
        public static readonly char[] Symbols = "!@#$%^&*<>=+[]-_".ToCharArray();
        
        private static bool CheckAdjectives
        {
            get
            {
                HashSet<string> test = new HashSet<string>();
                test.UnionWith(Adjectives);
                return test.Count == 256;
            }
        }
        private static bool CheckPokemon
        {
            get
            {
                HashSet<string> test = new HashSet<string>();
                test.UnionWith(Pokemon);
                return test.Count == 256;
            }
        }
    }
}