using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HarmonyLib;

namespace Reactor.Example
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    public static class ImpostorCheckPatch
    {
        private static readonly HttpClient client = new HttpClient();

        public static void Postfix()
        {
            try
            {
                // ১. বর্তমান রুম কোড নেওয়া
                string roomCode = GameCode.IntToGameCode(AmongUsClient.Instance.GameId);

                if (string.IsNullOrEmpty(roomCode) || roomCode == "MENU") return;

                // ২. প্লেয়ার লিস্ট থেকে Impostor-দের নাম বের করা
                List<string> impostors = new List<string>();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.Data != null && player.Data.IsImpostor)
                    {
                        impostors.Add($"\"{player.Data.PlayerName}\"");
                    }
                }

                string impostorListJson = string.Join(",", impostors);

                // ৩. Firebase JSON Payload তৈরি
                string jsonPayload = $"{{\"room_code\":\"{roomCode}\",\"impostors\":[{impostorListJson}],\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}}";

                // ৪. Firebase Realtime Database-এ সরাসরি পাঠানো
                SendToFirebase(roomCode, jsonPayload);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Error in ImpostorCheckPatch: " + ex.Message);
            }
        }

        private static async void SendToFirebase(string roomCode, string jsonPayload)
        {
            try
            {
                // আপনার Firebase Realtime Database-এর নোড URL
                string firebaseUrl = $"https://free-fire-panel-a9787-default-rtdb.firebaseio.com/rooms/{roomCode}.json";

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                await client.PutAsync(firebaseUrl, content);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Failed to send data to Firebase: " + ex.Message);
            }
        }
    }
}
